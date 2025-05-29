using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using AuthWebApp.Models;

namespace AuthWebApp.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalFaculty { get; set; }
        public int TotalCourses { get; set; }
        public int TotalRegistrations { get; set; }
        public decimal TotalFeesCollected { get; set; }
        public decimal TotalOutstandingFees { get; set; }
    }
    
    public class UserManagementViewModel
    {
        public IEnumerable<ApplicationUser> Students { get; set; }
        public IEnumerable<ApplicationUser> Faculty { get; set; }
    }
    
    public class CreateUserViewModel
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

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        
        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }
        
        // Student-specific fields
        [Display(Name = "Student ID")]
        public string? StudentId { get; set; }
        
        [Display(Name = "Program/Major")]
        public string? Program { get; set; }
        
        [Display(Name = "Current Semester")]
        [Range(1, 12)]
        public int? Semester { get; set; }
        
        // Faculty-specific fields
        [Display(Name = "Faculty ID")]
        public string? FacultyId { get; set; }
        
        [Display(Name = "Department")]
        public string? Department { get; set; }
        
        [Display(Name = "Position")]
        public string? Position { get; set; }
    }
    
    public class EditUserViewModel
    {
        public string Id { get; set; }
        
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
        
        [Display(Name = "Role")]
        public string Role { get; set; }
        
        // Student-specific fields
        [Display(Name = "Student ID")]
        public string? StudentId { get; set; }
        
        [Display(Name = "Program/Major")]
        public string? Program { get; set; }
        
        [Display(Name = "Current Semester")]
        [Range(1, 12)]
        public int? Semester { get; set; }
        
        // Faculty-specific fields
        [Display(Name = "Faculty ID")]
        public string? FacultyId { get; set; }
        
        [Display(Name = "Department")]
        public string? Department { get; set; }
        
        [Display(Name = "Position")]
        public string? Position { get; set; }
    }
    
    public class CourseManagementViewModel
    {
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<ApplicationUser> AvailableFaculty { get; set; }
    }
    
    public class CreateCourseViewModel
    {
        [Required]
        [StringLength(10)]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Required]
        [Range(1, 6)]
        [Display(Name = "Credit Hours")]
        public int CreditHours { get; set; }
        
        [Required]
        [Display(Name = "Semester")]
        public string Semester { get; set; }
        
        [Required]
        [Range(1, 200)]
        [Display(Name = "Maximum Capacity")]
        public int MaxCapacity { get; set; }
        
        [Display(Name = "Prerequisites")]
        public string? Prerequisites { get; set; }
        
        [Required]
        [Display(Name = "Faculty")]
        public string FacultyId { get; set; }
    }
    
    public class EditCourseViewModel
    {
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(10)]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Required]
        [Range(1, 6)]
        [Display(Name = "Credit Hours")]
        public int CreditHours { get; set; }
        
        [Required]
        [Display(Name = "Semester")]
        public string Semester { get; set; }
        
        [Required]
        [Range(1, 200)]
        [Display(Name = "Maximum Capacity")]
        public int MaxCapacity { get; set; }
        
        [Display(Name = "Prerequisites")]
        public string? Prerequisites { get; set; }
        
        [Required]
        [Display(Name = "Faculty")]
        public string FacultyId { get; set; }
    }
    
    public class RegistrationReportViewModel
    {
        public string Semester { get; set; }
        public IEnumerable<CourseRegistrationReport> CourseRegistrations { get; set; }
    }
    
    public class CourseRegistrationReport
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public int TotalSeats { get; set; }
        public int RegisteredStudents { get; set; }
        public int AvailableSeats { get; set; }
        public double FillRate { get; set; }
    }
    
    public class FeeReportViewModel
    {
        public string Semester { get; set; }
        public decimal TotalFeesCharged { get; set; }
        public decimal TotalFeesPaid { get; set; }
        public decimal TotalFeesOutstanding { get; set; }
        public double CollectionRate { get; set; }
        public IEnumerable<FeeReportDetail> FeeDetails { get; set; }
    }
    
    public class FeeReportDetail
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public decimal AmountCharged { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountOutstanding { get; set; }
        public FeeStatus Status { get; set; }
    }
} 