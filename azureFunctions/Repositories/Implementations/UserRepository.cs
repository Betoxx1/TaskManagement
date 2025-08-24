using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.Repositories.Interfaces;

namespace TaskManagement.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserModel> GetByIdAsync(string id)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserModel> GetByEmailAsync(string email)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<UserModel>> GetAllAsync()
        {
            return await _dbContext.Users
                .AsNoTracking()
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserModel>> GetActiveUsersAsync()
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserModel>> GetByRoleAsync(string role)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Role == role)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserModel>> GetByDepartmentAsync(string department)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Department == department)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<UserModel> CreateAsync(UserModel user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<UserModel> UpdateAsync(UserModel user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return false;

            _dbContext.Users.Remove(user);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == id);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UpdateLastLoginAsync(string id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return false;

            user.LastLoginAt = DateTime.UtcNow;
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
    }
} 