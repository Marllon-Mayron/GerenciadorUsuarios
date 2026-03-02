using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserManagement.Application.Services;
using UserManagement.Domain.Interfaces;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories;
using UserManagement.Infrastructure.Services;
using UserManagement.API.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();

// Configuracao Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });
});

// Configurar Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "MINHA-CHAVE-SECRETA-MUITO-FORTE-COM-32-CARACTERES!";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "UserManagementAPI",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "UserManagementClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "Role"
    };

    // Eventos para debug do JWT
    x.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Token inválido: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token válido!");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<UserService>();

// Configuracao do cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// ==================== CONFIGURAÇÃO DO RATE LIMITING ====================
builder.Services.AddRateLimiter(options =>
{
    // Política global (fallback)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext =>
        {
            // Usa UserId para autenticados, IP para anônimos
            string clientId = httpContext.User.Identity?.IsAuthenticated == true
                ? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "user"
                : httpContext.Connection.RemoteIpAddress?.ToString() ?? "ip";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: clientId,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
        });

    // Políticas nomeadas para diferentes tipos de endpoint
    options.AddFixedWindowLimiter("public", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("authenticated", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("admin", limiterOptions =>
    {
        limiterOptions.PermitLimit = 200;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("sensitive", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    // Personalizar resposta quando o limite é atingido
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers["Retry-After"] = "60";

        // Versão corrigida - sem usar MetadataName
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Muitas requisições. Por favor, aguarde um momento.",
            retryAfterSeconds = 60
        }, cancellationToken);
    };
});

var app = builder.Build();

// INICIALIZADOR DO BANCO
using (var scope = app.Services.CreateScope())
{
    try
    {
        var initializer = new DatabaseInitializer(
            scope.ServiceProvider,
            scope.ServiceProvider.GetRequiredService<ILogger<DatabaseInitializer>>()
        );

        await initializer.InitializeAsync();
        Console.WriteLine("Banco de dados inicializado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao inicializar banco de dados: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API V1");
    });

    // Middleware para log de requisições
    app.Use(async (context, next) =>
    {
        Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
        Console.WriteLine($"Origin: {context.Request.Headers["Origin"]}");
        Console.WriteLine($"Content-Type: {context.Request.Headers["Content-Type"]}");
        await next();
    });

    // app.UseHttpsRedirection();
}
else
{
    app.UseHttpsRedirection();
}

// ORDEM CORRETA DOS MIDDLEWARES
app.UseCors("AllowAngularApp");
app.UseRateLimiter(); // Rate limiting ANTES da autenticação
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CustomRateLimitingMiddleware>(); // Nosso middleware customizado

app.MapControllers();

// Criar/Migrar database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        Console.WriteLine("Database criada/verificada com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao criar/verificar database: {ex.Message}");
    }
}

Console.WriteLine("API iniciada com sucesso!");
Console.WriteLine($"URLs:");
Console.WriteLine($"  - Backend: http://localhost:5249");
Console.WriteLine($"  - Swagger: http://localhost:5249/swagger");
Console.WriteLine($"  - Frontend Angular: http://localhost:4200");

app.Run();