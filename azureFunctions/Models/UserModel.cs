using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models
{
    public class UserModel
    {
        [Required]
        [StringLength(100)]
        public string Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }
        
        [StringLength(50)]
        public string Role { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; }
        
        [StringLength(255)]
        public string Department { get; set; }
        
        [StringLength(500)]
        public string ProfilePictureUrl { get; set; }
    }
} 