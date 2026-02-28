using System.ComponentModel.DataAnnotations;

namespace UserManagement.Application.DTOs
{
    public class ChangeStatusDto
    {
        [Required]
        public bool Activate { get; set; }
    }
}