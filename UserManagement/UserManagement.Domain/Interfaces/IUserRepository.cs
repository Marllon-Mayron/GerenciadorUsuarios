using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);

        Task<(IEnumerable<User> Users, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize);

        Task<(int Active, int Inactive, int Admin, int User)> GetUserStatisticsAsync();
    }
}