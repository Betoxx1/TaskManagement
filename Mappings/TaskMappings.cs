using System;
using System.Linq;
using TaskManagement.DTOs;
using TaskManagement.Models;
using TaskManagement.Utils;
using TaskStatus = TaskManagement.Models.TaskStatus;

namespace TaskManagement.Mappings
{
    public static class TaskMappings
    {
        public static TaskResponseDto ToResponseDto(TaskModel task)
        {
            if (task == null) return null;

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StatusDisplay = task.Status.GetTaskStatusDisplay(),
                Priority = task.Priority,
                PriorityDisplay = task.Priority.GetTaskPriorityDisplay(),
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                DueDate = task.DueDate,
                UserId = task.UserId,
                Category = task.Category,
                Tags = SplitTags(task.Tags),
                IsOverdue = IsTaskOverdue(task),
                DaysUntilDue = CalculateDaysUntilDue(task)
            };
        }

        public static TaskModel FromCreateDto(CreateTaskDto dto, string userId)
        {
            if (dto == null) return null;

            return new TaskModel
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                UserId = userId,
                Category = dto.Category,
                Tags = JoinTags(dto.Tags),
                CreatedAt = DateTime.Now
            };
        }

        public static void UpdateFromDto(TaskModel task, UpdateTaskDto dto)
        {
            if (task == null || dto == null) return;

            if (!string.IsNullOrEmpty(dto.Title))
                task.Title = dto.Title;

            if (dto.Description != null)
                task.Description = dto.Description;

            if (dto.Status.HasValue)
                task.Status = dto.Status.Value;

            if (dto.Priority.HasValue)
                task.Priority = dto.Priority.Value;

            if (dto.DueDate.HasValue)
                task.DueDate = dto.DueDate.Value;

            if (dto.Category != null)
                task.Category = dto.Category;

            if (dto.Tags != null)
                task.Tags = JoinTags(dto.Tags);

            task.UpdatedAt = DateTime.Now;
        }

        public static TaskModel ToModel(TaskResponseDto dto)
        {
            if (dto == null) return null;

            return new TaskModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                DueDate = dto.DueDate,
                UserId = dto.UserId,
                Category = dto.Category,
                Tags = JoinTags(dto.Tags)
            };
        }

        private static string[] SplitTags(string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
                return Array.Empty<string>();

            return tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(tag => tag.Trim())
                      .Where(tag => !string.IsNullOrEmpty(tag))
                      .ToArray();
        }

        private static string JoinTags(string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
                return string.Empty;

            return string.Join(",", SplitTags(tags));
        }

        private static string JoinTags(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return string.Empty;

            return string.Join(",", tags.Where(tag => !string.IsNullOrWhiteSpace(tag)));
        }

        private static bool IsTaskOverdue(TaskModel task)
        {
            if (!task.DueDate.HasValue || task.Status == TaskStatus.Completed)
                return false;

            return task.DueDate.Value.Date < DateTime.Now.Date;
        }

        private static int CalculateDaysUntilDue(TaskModel task)
        {
            if (!task.DueDate.HasValue || task.Status == TaskStatus.Completed)
                return int.MaxValue;

            var days = (task.DueDate.Value.Date - DateTime.Now.Date).Days;
            return Math.Max(days, int.MinValue);
        }

        public static IQueryable<TaskResponseDto> ProjectToResponseDto(IQueryable<TaskModel> query)
        {
            return query.Select(task => new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StatusDisplay = task.Status.GetTaskStatusDisplay(),
                Priority = task.Priority,
                PriorityDisplay = task.Priority.GetTaskPriorityDisplay(),
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                DueDate = task.DueDate,
                UserId = task.UserId,
                Category = task.Category,
                Tags = SplitTags(task.Tags),
                IsOverdue = task.DueDate.HasValue && 
                           task.DueDate.Value.Date < DateTime.Now.Date && 
                           task.Status != TaskStatus.Completed,
                DaysUntilDue = task.DueDate.HasValue && task.Status != TaskStatus.Completed
                    ? (task.DueDate.Value.Date - DateTime.Now.Date).Days
                    : int.MaxValue
            });
        }
    }
} 