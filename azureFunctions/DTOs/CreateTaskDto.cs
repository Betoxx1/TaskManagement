using System;
using System.ComponentModel.DataAnnotations;
using TaskManagement.Models;
using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        public DateTime? DueDate { get; set; }
        
        [StringLength(100)]
        public string Category { get; set; }
        
        [StringLength(500)]
        public string Tags { get; set; }
    }
} 