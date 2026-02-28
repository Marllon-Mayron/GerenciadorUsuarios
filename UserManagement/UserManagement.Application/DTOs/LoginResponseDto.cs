using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Application.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
    }
}
