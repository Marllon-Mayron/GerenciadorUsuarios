namespace UserManagement.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RateLimitAttribute : Attribute
    {
        public int PermitLimit { get; set; } = 100; // Número máximo de requisições
        public int WindowInSeconds { get; set; } = 60; // Janela de tempo em segundos
        public int QueueLimit { get; set; } = 0; // Limite de fila (0 = sem fila)
        public string PolicyName { get; set; } = string.Empty; // Nome da política (opcional)

        public RateLimitAttribute()
        {
        }

        public RateLimitAttribute(int permitLimit, int windowInSeconds)
        {
            PermitLimit = permitLimit;
            WindowInSeconds = windowInSeconds;
        }
    }
}