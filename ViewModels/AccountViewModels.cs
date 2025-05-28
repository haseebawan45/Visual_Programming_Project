using System.ComponentModel.DataAnnotations;
using AuthWebApp.Models;

namespace AuthWebApp.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
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

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        
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
} 