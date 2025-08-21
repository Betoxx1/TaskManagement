using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TaskManagement.Models;

namespace TaskManagement.Utils
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description ?? enumValue.ToString();
        }

        public static string GetTaskStatusDisplay(this TaskStatus status)
        {
            return status switch
            {
                TaskStatus.Pending => "Pendiente",
                TaskStatus.InProgress => "En Progreso",
                TaskStatus.Completed => "Completada",
                TaskStatus.Cancelled => "Cancelada",
                _ => status.ToString()
            };
        }

        public static string GetTaskPriorityDisplay(this TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => "Baja",
                TaskPriority.Medium => "Media",
                TaskPriority.High => "Alta",
                TaskPriority.Critical => "Crítica",
                _ => priority.ToString()
            };
        }

        public static string GetTaskStatusClass(this TaskStatus status)
        {
            return status switch
            {
                TaskStatus.Pending => "status-pending",
                TaskStatus.InProgress => "status-in-progress",
                TaskStatus.Completed => "status-completed",
                TaskStatus.Cancelled => "status-cancelled",
                _ => "status-unknown"
            };
        }

        public static string GetTaskPriorityClass(this TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => "priority-low",
                TaskPriority.Medium => "priority-medium",
                TaskPriority.High => "priority-high",
                TaskPriority.Critical => "priority-critical",
                _ => "priority-unknown"
            };
        }

        public static bool IsCompleted(this TaskStatus status)
        {
            return status == TaskStatus.Completed;
        }

        public static bool IsActive(this TaskStatus status)
        {
            return status is TaskStatus.Pending or TaskStatus.InProgress;
        }

        public static bool IsHighPriority(this TaskPriority priority)
        {
            return priority is TaskPriority.High or TaskPriority.Critical;
        }

        public static int GetPriorityWeight(this TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => 1,
                TaskPriority.Medium => 2,
                TaskPriority.High => 3,
                TaskPriority.Critical => 4,
                _ => 0
            };
        }
    }
} 