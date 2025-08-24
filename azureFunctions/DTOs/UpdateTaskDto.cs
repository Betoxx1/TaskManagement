using System;
using System.ComponentModel.DataAnnotations;
using TaskManagement.Models;
using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement.DTOs
{
    public class UpdateTaskDto
    {
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        public TaskStatus? Status { get; set; }
        
        public TaskPriority? Priority { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        [StringLength(100)]
        public string Category { get; set; }
        
        [StringLength(500)]
        public string Tags { get; set; }
    }
} 