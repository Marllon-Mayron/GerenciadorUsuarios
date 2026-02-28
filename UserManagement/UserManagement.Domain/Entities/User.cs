using System;
using System.Data;
using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Entities
{
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Status = UserStatus.Ativo;
            Role = UserRole.User;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserStatus Status { get; set; }

        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            Status = UserStatus.Ativo;
            UpdateTimestamp();
        }
        public bool IsAdmin()
        {
            return Role == UserRole.Admin;
        }

        public bool IsUser()
        {
            return Role == UserRole.User;
        }

        public void Deactivate()
        {
            Status = UserStatus.Inativo;
            UpdateTimestamp();
        }

        public bool CanAuthenticate()
        {
            return Status == UserStatus.Ativo;
        }
    }
}