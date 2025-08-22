using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.DTOs;
using TaskManagement.Mappings;
using TaskManagement.Models;
using TaskManagement.Repositories.Interfaces;
using TaskManagement.Services.Interfaces;
using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public async Task<TaskResponseDto> GetTaskByIdAsync(int id, string userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            
            if (task == null || task.UserId != userId)
                return null;

            return TaskMappings.ToResponseDto(task);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(string userId)
        {
            var tasks = await _taskRepository.GetByUserIdAsync(userId);
            return tasks.Select(TaskMappings.ToResponseDto);
        }

        public async Task<IEnumerable<TaskResponseDto>> FilterTasksAsync(string userId, TaskStatus? status, TaskPriority? priority, string category, DateTime? dueDateFrom, DateTime? dueDateTo)
        {
            var tasks = await _taskRepository.FilterTasksAsync(userId, status, priority, category, dueDateFrom, dueDateTo);
            return tasks.Select(TaskMappings.ToResponseDto);
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto, string userId)
        {
            // Verificar que el usuario existe
            var userExists = await _userRepository.ExistsAsync(userId);
            if (!userExists)
                throw new ArgumentException("Usuario no válido");

            var task = TaskMappings.FromCreateDto(createTaskDto, userId);
            var createdTask = await _taskRepository.CreateAsync(task);
            
            return TaskMappings.ToResponseDto(createdTask);
        }

        public async Task<TaskResponseDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, string userId)
        {
            var existingTask = await _taskRepository.GetByIdAsync(id);
            
            if (existingTask == null || existingTask.UserId != userId)
                return null;

            TaskMappings.UpdateFromDto(existingTask, updateTaskDto);
            var updatedTask = await _taskRepository.UpdateAsync(existingTask);
            
            return TaskMappings.ToResponseDto(updatedTask);
        }

        public async Task<bool> DeleteTaskAsync(int id, string userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            
            if (task == null || task.UserId != userId)
                return false;

            return await _taskRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetOverdueTasksAsync(string userId)
        {
            var allOverdueTasks = await _taskRepository.GetOverdueTasksAsync();
            var userOverdueTasks = allOverdueTasks.Where(t => t.UserId == userId);
            return userOverdueTasks.Select(TaskMappings.ToResponseDto);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetTasksDueSoonAsync(string userId, int days = 7)
        {
            var allTasksDueSoon = await _taskRepository.GetTasksDueSoonAsync(days);
            var userTasksDueSoon = allTasksDueSoon.Where(t => t.UserId == userId);
            return userTasksDueSoon.Select(TaskMappings.ToResponseDto);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetTasksByStatusAsync(string userId, TaskStatus status)
        {
            var allTasksByStatus = await _taskRepository.GetByStatusAsync(status);
            var userTasksByStatus = allTasksByStatus.Where(t => t.UserId == userId);
            return userTasksByStatus.Select(TaskMappings.ToResponseDto);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetTasksByPriorityAsync(string userId, TaskPriority priority)
        {
            var allTasksByPriority = await _taskRepository.GetByPriorityAsync(priority);
            var userTasksByPriority = allTasksByPriority.Where(t => t.UserId == userId);
            return userTasksByPriority.Select(TaskMappings.ToResponseDto);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetTasksByCategoryAsync(string userId, string category)
        {
            var allTasksByCategory = await _taskRepository.GetByCategoryAsync(category);
            var userTasksByCategory = allTasksByCategory.Where(t => t.UserId == userId);
            return userTasksByCategory.Select(TaskMappings.ToResponseDto);
        }

        public async Task<bool> MarkTaskAsCompletedAsync(int id, string userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            
            if (task == null || task.UserId != userId)
                return false;

            task.Status = TaskStatus.Completed;
            await _taskRepository.UpdateAsync(task);
            return true;
        }

        public async Task<bool> MarkTaskAsInProgressAsync(int id, string userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            
            if (task == null || task.UserId != userId)
                return false;

            task.Status = TaskStatus.InProgress;
            await _taskRepository.UpdateAsync(task);
            return true;
        }

        public async Task<TaskStatisticsDto> GetTaskStatisticsAsync(string userId)
        {
            var allTasks = await _taskRepository.GetByUserIdAsync(userId);
            var tasksList = allTasks.ToList();

            var totalTasks = tasksList.Count;
            var completedTasks = tasksList.Count(t => t.Status == TaskStatus.Completed);
            var pendingTasks = tasksList.Count(t => t.Status == TaskStatus.Pending);
            var inProgressTasks = tasksList.Count(t => t.Status == TaskStatus.InProgress);
            
            var overdueTasks = tasksList.Count(t => t.DueDate.HasValue && 
                                                   t.DueDate < DateTime.Now && 
                                                   t.Status != TaskStatus.Completed);
            
            var tasksDueSoon = tasksList.Count(t => t.DueDate.HasValue && 
                                                   t.DueDate >= DateTime.Now && 
                                                   t.DueDate <= DateTime.Now.AddDays(7) && 
                                                   t.Status != TaskStatus.Completed);

            var completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;

            return new TaskStatisticsDto
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks,
                InProgressTasks = inProgressTasks,
                OverdueTasks = overdueTasks,
                TasksDueSoon = tasksDueSoon,
                CompletionRate = Math.Round(completionRate, 2)
            };
        }
    }
} 