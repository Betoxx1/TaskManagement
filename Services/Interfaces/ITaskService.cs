using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.DTOs;
using TaskManagement.Models;
using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement.Services.Interfaces
{
    public interface ITaskService
    {
        Task<TaskResponseDto> GetTaskByIdAsync(int id, string userId);
        Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(string userId);
        Task<IEnumerable<TaskResponseDto>> FilterTasksAsync(string userId, TaskStatus? status, TaskPriority? priority, string category, DateTime? dueDateFrom, DateTime? dueDateTo);
        Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto, string userId);
        Task<TaskResponseDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, string userId);
        Task<bool> DeleteTaskAsync(int id, string userId);
        Task<IEnumerable<TaskResponseDto>> GetOverdueTasksAsync(string userId);
        Task<IEnumerable<TaskResponseDto>> GetTasksDueSoonAsync(string userId, int days = 7);
        Task<IEnumerable<TaskResponseDto>> GetTasksByStatusAsync(string userId, TaskStatus status);
        Task<IEnumerable<TaskResponseDto>> GetTasksByPriorityAsync(string userId, TaskPriority priority);
        Task<IEnumerable<TaskResponseDto>> GetTasksByCategoryAsync(string userId, string category);
        Task<bool> MarkTaskAsCompletedAsync(int id, string userId);
        Task<bool> MarkTaskAsInProgressAsync(int id, string userId);
        Task<TaskStatisticsDto> GetTaskStatisticsAsync(string userId);
    }
    
    public class TaskStatisticsDto
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int TasksDueSoon { get; set; }
        public double CompletionRate { get; set; }
    }
} 