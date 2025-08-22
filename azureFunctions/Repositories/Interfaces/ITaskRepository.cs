using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Models;
using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskModel> GetByIdAsync(int id);
        Task<IEnumerable<TaskModel>> GetAllAsync();
        Task<IEnumerable<TaskModel>> GetByUserIdAsync(string userId);
        Task<IEnumerable<TaskModel>> GetByStatusAsync(TaskStatus status);
        Task<IEnumerable<TaskModel>> GetByPriorityAsync(TaskPriority priority);
        Task<IEnumerable<TaskModel>> GetByCategoryAsync(string category);
        Task<IEnumerable<TaskModel>> GetOverdueTasksAsync();
        Task<IEnumerable<TaskModel>> GetTasksDueSoonAsync(int days = 7);
        Task<IEnumerable<TaskModel>> FilterTasksAsync(string userId, TaskStatus? status, TaskPriority? priority, string category, DateTime? dueDateFrom, DateTime? dueDateTo);
        Task<TaskModel> CreateAsync(TaskModel task);
        Task<TaskModel> UpdateAsync(TaskModel task);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetCountByUserAsync(string userId);
    }
} 