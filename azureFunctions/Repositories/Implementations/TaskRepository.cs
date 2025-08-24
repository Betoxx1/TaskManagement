using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.Repositories.Interfaces;
using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement.Repositories.Implementations
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _dbContext;

        public TaskRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TaskModel> GetByIdAsync(int id)
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskModel>> GetAllAsync()
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskModel>> GetByUserIdAsync(string userId)
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskModel>> GetByStatusAsync(TaskStatus status)
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskModel>> GetByPriorityAsync(TaskPriority priority)
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.Priority == priority)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskModel>> GetByCategoryAsync(string category)
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.Category == category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskModel>> GetOverdueTasksAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.DueDate.HasValue && t.DueDate < now && t.Status != TaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskModel>> GetTasksDueSoonAsync(int days = 7)
        {
            var now = DateTime.UtcNow;
            var dueDate = now.AddDays(days);
            return await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.DueDate.HasValue && 
                           t.DueDate >= now && 
                           t.DueDate <= dueDate && 
                           t.Status != TaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskModel>> FilterTasksAsync(string userId, TaskStatus? status, TaskPriority? priority, string category, DateTime? dueDateFrom, DateTime? dueDateTo)
        {
            var query = _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }

            if (dueDateFrom.HasValue)
            {
                query = query.Where(t => t.DueDate.HasValue && t.DueDate >= dueDateFrom.Value);
            }

            if (dueDateTo.HasValue)
            {
                query = query.Where(t => t.DueDate.HasValue && t.DueDate <= dueDateTo.Value);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<TaskModel> CreateAsync(TaskModel task)
        {
            task.CreatedAt = DateTime.UtcNow;
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();
            return task;
        }

        public async Task<TaskModel> UpdateAsync(TaskModel task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _dbContext.Tasks.FindAsync(id);
            if (task == null)
                return false;

            _dbContext.Tasks.Remove(task);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .AnyAsync(t => t.Id == id);
        }

        public async Task<int> GetCountByUserAsync(string userId)
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .CountAsync(t => t.UserId == userId);
        }
    }
} 