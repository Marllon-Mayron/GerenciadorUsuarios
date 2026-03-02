// Middleware/CustomRateLimitingMiddleware.cs
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using UserManagement.API.Attributes;

namespace UserManagement.API.Middleware
{
    public class CustomRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CustomRateLimitingMiddleware> _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        // Configurações padrão por tipo de endpoint
        private readonly Dictionary<RateLimitCategory, RateLimitRule> _defaultRules = new()
        {
            [RateLimitCategory.Public] = new() { PermitLimit = 10, WindowInSeconds = 60 },      // 10 req/min para endpoints públicos
            [RateLimitCategory.Authenticated] = new() { PermitLimit = 100, WindowInSeconds = 60 }, // 100 req/min para autenticados
            [RateLimitCategory.Admin] = new() { PermitLimit = 200, WindowInSeconds = 60 },       // 200 req/min para admins
            [RateLimitCategory.Sensitive] = new() { PermitLimit = 5, WindowInSeconds = 60 }      // 5 req/min para operações sensíveis
        };

        public CustomRateLimitingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<CustomRateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Verifica se o endpoint tem [DisableRateLimit]
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<DisableRateLimitAttribute>() != null)
            {
                await _next(context);
                return;
            }

            // Obtém o atributo RateLimit do endpoint (se existir)
            var rateLimitAttr = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();

            // Determina a categoria do rate limit
            var category = GetRateLimitCategory(context, rateLimitAttr);

            // Obtém a chave única para o cliente (IP ou UserId)
            var clientKey = GetClientKey(context);

            // Aplica o rate limiting
            var rule = rateLimitAttr != null
                ? new RateLimitRule
                {
                    PermitLimit = rateLimitAttr.PermitLimit,
                    WindowInSeconds = rateLimitAttr.WindowInSeconds,
                    QueueLimit = rateLimitAttr.QueueLimit
                }
                : _defaultRules[category];

            var result = await IsRequestAllowedAsync(clientKey, rule, category.ToString());

            if (!result.Allowed)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = result.RetryAfter?.Seconds.ToString();

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Muitas requisições. Por favor, aguarde.",
                    retryAfterSeconds = result.RetryAfter?.Seconds,
                    limit = rule.PermitLimit,
                    windowInSeconds = rule.WindowInSeconds
                });
                return;
            }

            // Adiciona headers de rate limit na resposta
            context.Response.Headers["X-RateLimit-Limit"] = rule.PermitLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
            context.Response.Headers["X-RateLimit-Reset"] = result.ResetTime?.ToUnixTimeSeconds().ToString();

            await _next(context);
        }

        private RateLimitCategory GetRateLimitCategory(HttpContext context, RateLimitAttribute? attribute)
        {
            if (attribute != null)
                return RateLimitCategory.Custom;

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                if (context.User.IsInRole("Admin"))
                    return RateLimitCategory.Admin;
                return RateLimitCategory.Authenticated;
            }

            // Verifica se é um endpoint sensível (login, register, etc)
            var path = context.Request.Path.ToString().ToLower();
            if (path.Contains("/login") || path.Contains("/register"))
                return RateLimitCategory.Sensitive;

            return RateLimitCategory.Public;
        }

        private string GetClientKey(HttpContext context)
        {
            // Para usuários autenticados, usa o UserId
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    return $"user_{userId}";
            }

            // Para não autenticados, usa o IP
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip_{ip}";
        }

        private async Task<(bool Allowed, int Remaining, DateTimeOffset? ResetTime, TimeSpan? RetryAfter)>
            IsRequestAllowedAsync(string clientKey, RateLimitRule rule, string category)
        {
            var cacheKey = $"rate_limit_{clientKey}_{category}";
            var windowKey = $"{cacheKey}_window";

            await _semaphore.WaitAsync();
            try
            {
                var now = DateTimeOffset.UtcNow;
                var windowStart = _cache.Get<DateTimeOffset?>(windowKey) ?? now;
                var requestCount = _cache.Get<int?>(cacheKey) ?? 0;

                // Se passou da janela, reinicia
                if (now - windowStart > TimeSpan.FromSeconds(rule.WindowInSeconds))
                {
                    windowStart = now;
                    requestCount = 0;
                }

                // Verifica se atingiu o limite
                if (requestCount >= rule.PermitLimit)
                {
                    var resetTime = windowStart.AddSeconds(rule.WindowInSeconds);
                    var retryAfter = resetTime - now;

                    return (false, rule.PermitLimit - requestCount, resetTime, retryAfter);
                }

                // Incrementa o contador
                requestCount++;

                // Atualiza o cache
                _cache.Set(cacheKey, requestCount, TimeSpan.FromSeconds(rule.WindowInSeconds * 2));
                _cache.Set(windowKey, windowStart, TimeSpan.FromSeconds(rule.WindowInSeconds * 2));

                var remaining = rule.PermitLimit - requestCount;
                var reset = windowStart.AddSeconds(rule.WindowInSeconds);

                return (true, remaining, reset, null);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public class RateLimitRule
    {
        public int PermitLimit { get; set; }
        public int WindowInSeconds { get; set; }
        public int QueueLimit { get; set; }
    }

    public enum RateLimitCategory
    {
        Public,
        Authenticated,
        Admin,
        Sensitive,
        Custom
    }
}