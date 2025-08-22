using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TaskManagement.Factories;
using TaskManagement.Models;
using TaskManagement.Repositories.Interfaces;
using Dapper;
using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement.Repositories.Implementations
{
    public class TaskRepository : ITaskRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public TaskRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<TaskModel> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Tasks WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<TaskModel>(sql, new { Id = id });
        }

        public async Task<IEnumerable<TaskModel>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Tasks ORDER BY CreatedAt DESC";
            return await connection.QueryAsync<TaskModel>(sql);
        }

        public async Task<IEnumerable<TaskModel>> GetByUserIdAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Tasks WHERE UserId = @UserId ORDER BY CreatedAt DESC";
            return await connection.QueryAsync<TaskModel>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<TaskModel>> GetByStatusAsync(TaskStatus status)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Tasks WHERE Status = @Status ORDER BY CreatedAt DESC";
            return await connection.QueryAsync<TaskModel>(sql, new { Status = status });
        }

        public async Task<IEnumerable<TaskModel>> GetByPriorityAsync(TaskPriority priority)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Tasks WHERE Priority = @Priority ORDER BY CreatedAt DESC";
            return await connection.QueryAsync<TaskModel>(sql, new { Priority = priority });
        }

        public async Task<IEnumerable<TaskModel>> GetByCategoryAsync(string category)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Tasks WHERE Category = @Category ORDER BY CreatedAt DESC";
            return await connection.QueryAsync<TaskModel>(sql, new { Category = category });
        }

        public async Task<IEnumerable<TaskModel>> GetOverdueTasksAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT * FROM Tasks WHERE DueDate < @Now AND Status != @CompletedStatus ORDER BY DueDate ASC";
            return await connection.QueryAsync<TaskModel>(sql, new { Now = DateTime.Now, CompletedStatus = TaskStatus.Completed });
        }

        public async Task<IEnumerable<TaskModel>> GetTasksDueSoonAsync(int days = 7)
        {
            using var connection = _connectionFactory.CreateConnection();
            var dueDate = DateTime.Now.AddDays(days);
            var sql = @"SELECT * FROM Tasks WHERE DueDate BETWEEN @Now AND @DueDate AND Status != @CompletedStatus ORDER BY DueDate ASC";
            return await connection.QueryAsync<TaskModel>(sql, new { Now = DateTime.Now, DueDate = dueDate, CompletedStatus = TaskStatus.Completed });
        }

        public async Task<IEnumerable<TaskModel>> FilterTasksAsync(string userId, TaskStatus? status, TaskPriority? priority, string category, DateTime? dueDateFrom, DateTime? dueDateTo)
        {
            using var connection = _connectionFactory.CreateConnection();
            var conditions = new List<string> { "UserId = @UserId" };
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            if (status.HasValue)
            {
                conditions.Add("Status = @Status");
                parameters.Add("Status", status.Value);
            }

            if (priority.HasValue)
            {
                conditions.Add("Priority = @Priority");
                parameters.Add("Priority", priority.Value);
            }

            if (!string.IsNullOrEmpty(category))
            {
                conditions.Add("Category = @Category");
                parameters.Add("Category", category);
            }

            if (dueDateFrom.HasValue)
            {
                conditions.Add("DueDate >= @DueDateFrom");
                parameters.Add("DueDateFrom", dueDateFrom.Value);
            }

            if (dueDateTo.HasValue)
            {
                conditions.Add("DueDate <= @DueDateTo");
                parameters.Add("DueDateTo", dueDateTo.Value);
            }

            var sql = $"SELECT * FROM Tasks WHERE {string.Join(" AND ", conditions)} ORDER BY CreatedAt DESC";
            return await connection.QueryAsync<TaskModel>(sql, parameters);
        }

        public async Task<TaskModel> CreateAsync(TaskModel task)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO Tasks (Title, Description, Status, Priority, CreatedAt, DueDate, UserId, Category, Tags)
                VALUES (@Title, @Description, @Status, @Priority, @CreatedAt, @DueDate, @UserId, @Category, @Tags);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            
            task.CreatedAt = DateTime.Now;
            var id = await connection.QuerySingleAsync<int>(sql, task);
            task.Id = id;
            return task;
        }

        public async Task<TaskModel> UpdateAsync(TaskModel task)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE Tasks 
                SET Title = @Title, Description = @Description, Status = @Status, Priority = @Priority, 
                    UpdatedAt = @UpdatedAt, DueDate = @DueDate, Category = @Category, Tags = @Tags
                WHERE Id = @Id";
            
            task.UpdatedAt = DateTime.Now;
            await connection.ExecuteAsync(sql, task);
            return task;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"DELETE FROM Tasks WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT COUNT(1) FROM Tasks WHERE Id = @Id";
            var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<int> GetCountByUserAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT COUNT(*) FROM Tasks WHERE UserId = @UserId";
            return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
        }
    }
} 