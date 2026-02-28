using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Infrastructure.Data
{
    public class DatabaseInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            // 1. Aplica todas as migrations pendentes (cria/atualiza estrutura)
            _logger.LogInformation("   Aplicando migrations pendentes...");
            await context.Database.MigrateAsync();
            _logger.LogInformation("   Migrations aplicadas com sucesso!");

            // 2. Executa seeds (dados iniciais) - Verifica se admin existe
            await SeedAdminUserAsync(context, passwordHasher);

            // 3. Outros seeds podem ser adicionados aqui
            await SeedTestUserAsync(context, passwordHasher);
        }

        private async Task SeedAdminUserAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            // Verifica se já existe algum admin
            var adminExists = await context.Users.AnyAsync(u => u.Role == UserRole.Admin);

            if (!adminExists)
            {
                _logger.LogInformation("  Nenhum administrador encontrado. Criando admin padrão...");

                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Administrador",
                    Email = "admin@gmail.com",
                    PasswordHash = passwordHasher.HashPassword("admin123456"),
                    Role = UserRole.Admin,
                    Status = UserStatus.Ativo,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();

                _logger.LogInformation("   Admin criado com sucesso!");
                _logger.LogInformation("   Email: admin@gmail.com");
                _logger.LogInformation("   Senha: admin123456");
            }
            else
            {
                _logger.LogInformation("   Administrador já existe. Pulando seed...");
            }
        }

        private async Task SeedTestUserAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            // Verifica se existe pelo menos um usuário comum
            var userExists = await context.Users.AnyAsync(u => u.Role == UserRole.User);

            if (!userExists)
            {
                _logger.LogInformation("   Nenhum usuário comum encontrado. Criando usuário de teste...");

                var testUser = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Usuário Teste",
                    Email = "user@email.com",
                    PasswordHash = passwordHasher.HashPassword("12345678"),
                    Role = UserRole.User,
                    Status = UserStatus.Ativo,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(testUser);
                await context.SaveChangesAsync();

                _logger.LogInformation("   Usuário de teste criado com sucesso!");
                _logger.LogInformation("   Email: user@email.com");
                _logger.LogInformation("   Senha: 12345678");
            }
        }
    }
}