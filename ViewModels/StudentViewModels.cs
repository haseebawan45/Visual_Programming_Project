using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using AuthWebApp.Models;

namespace AuthWebApp.ViewModels
{
    public class StudentDashboardViewModel
    {
        public ApplicationUser Student { get; set; }
        public IEnumerable<CourseRegistration> RegisteredCourses { get; set; }
        public IEnumerable<Assignment> PendingAssignments { get; set; }
        public IEnumerable<FeeRecord> OutstandingFees { get; set; }
    }
    
    public class EditStudentProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Student ID")]
        public string StudentId { get; set; }
        
        [Display(Name = "Program/Major")]
        public string Program { get; set; }
        
        [Display(Name = "Current Semester")]
        [Range(1, 12)]
        public int? Semester { get; set; }
    }
    
    public class SubmitAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
        
        [Display(Name = "Course")]
        public string CourseTitle { get; set; }
        
        [Required]
        [Display(Name = "Assignment File")]
        public IFormFile File { get; set; }
    }
} 