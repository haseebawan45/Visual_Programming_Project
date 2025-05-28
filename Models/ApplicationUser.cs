using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace AuthWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add additional fields here if needed
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        // Role identifier (can be combined with ASP.NET Identity roles)
        public UserRole Role { get; set; }
        
        // Student specific properties
        public string? StudentId { get; set; }
        public string? Program { get; set; }
        public int? Semester { get; set; }
        public double? GPA { get; set; }
        
        // Faculty specific properties (will be used later)
        public string? FacultyId { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        
        // Navigation properties
        public virtual ICollection<CourseRegistration>? CourseRegistrations { get; set; }
        public virtual ICollection<Assignment>? Assignments { get; set; } // Assignments submitted by student
        public virtual ICollection<FeeRecord>? FeeRecords { get; set; }
    }
    
    public enum UserRole
    {
        Student,
        Faculty,
        Admin
    }
} 