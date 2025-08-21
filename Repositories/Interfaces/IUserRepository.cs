using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Models;

namespace TaskManagement.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserModel> GetByIdAsync(string id);
        Task<UserModel> GetByEmailAsync(string email);
        Task<IEnumerable<UserModel>> GetAllAsync();
        Task<IEnumerable<UserModel>> GetActiveUsersAsync();
        Task<IEnumerable<UserModel>> GetByRoleAsync(string role);
        Task<IEnumerable<UserModel>> GetByDepartmentAsync(string department);
        Task<UserModel> CreateAsync(UserModel user);
        Task<UserModel> UpdateAsync(UserModel user);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> UpdateLastLoginAsync(string id);
    }
} 