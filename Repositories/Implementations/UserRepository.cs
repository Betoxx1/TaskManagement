using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TaskManagement.Factories;
using TaskManagement.Models;
using TaskManagement.Repositories.Interfaces;
using Dapper;

namespace TaskManagement.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public UserRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<UserModel> GetByIdAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Users WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<UserModel>(sql, new { Id = id });
        }

        public async Task<UserModel> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Users WHERE Email = @Email";
            return await connection.QueryFirstOrDefaultAsync<UserModel>(sql, new { Email = email });
        }

        public async Task<IEnumerable<UserModel>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Users ORDER BY Name";
            return await connection.QueryAsync<UserModel>(sql);
        }

        public async Task<IEnumerable<UserModel>> GetActiveUsersAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Users WHERE IsActive = 1 ORDER BY Name";
            return await connection.QueryAsync<UserModel>(sql);
        }

        public async Task<IEnumerable<UserModel>> GetByRoleAsync(string role)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Users WHERE Role = @Role ORDER BY Name";
            return await connection.QueryAsync<UserModel>(sql, new { Role = role });
        }

        public async Task<IEnumerable<UserModel>> GetByDepartmentAsync(string department)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Users WHERE Department = @Department ORDER BY Name";
            return await connection.QueryAsync<UserModel>(sql, new { Department = department });
        }

        public async Task<UserModel> CreateAsync(UserModel user)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO Users (Id, Name, Email, Role, CreatedAt, IsActive, Department, ProfilePictureUrl)
                VALUES (@Id, @Name, @Email, @Role, @CreatedAt, @IsActive, @Department, @ProfilePictureUrl)";
            
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;
            await connection.ExecuteAsync(sql, user);
            return user;
        }

        public async Task<UserModel> UpdateAsync(UserModel user)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE Users 
                SET Name = @Name, Email = @Email, Role = @Role, IsActive = @IsActive, 
                    Department = @Department, ProfilePictureUrl = @ProfilePictureUrl
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, user);
            return user;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"DELETE FROM Users WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT COUNT(1) FROM Users WHERE Id = @Id";
            var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT COUNT(1) FROM Users WHERE Email = @Email";
            var count = await connection.QuerySingleAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"UPDATE Users SET LastLoginAt = @LastLoginAt WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, LastLoginAt = DateTime.Now });
            return rowsAffected > 0;
        }
    }
} 