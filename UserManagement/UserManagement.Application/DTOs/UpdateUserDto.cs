using System.ComponentModel.DataAnnotations;
using UserManagement.Domain.Enums;

namespace UserManagement.Application.DTOs
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        public UserRole? Role { get; set; }
    }
}