using System;
using System.Collections.Generic;
using System.Text;

using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
