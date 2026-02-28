using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasDefaultValue(UserRole.User);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.ToTable("Users");
            });
        }
    }
}