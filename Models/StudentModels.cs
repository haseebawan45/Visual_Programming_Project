using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthWebApp.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(10)]
        public string CourseCode { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int CreditHours { get; set; }
        
        public string? Semester { get; set; }
        
        // Max capacity of students
        public int MaxCapacity { get; set; }
        
        // Prerequisites - comma separated course IDs
        public string? Prerequisites { get; set; }
        
        // Navigation properties
        public virtual ICollection<CourseRegistration>? Registrations { get; set; }
        public virtual ICollection<Assignment>? Assignments { get; set; }
        
        // Faculty who teaches this course
        public string? FacultyId { get; set; }
        
        [ForeignKey("FacultyId")]
        public virtual ApplicationUser? Faculty { get; set; }
    }
    
    public class CourseRegistration
    {
        [Key]
        public int RegistrationId { get; set; }
        
        public int CourseId { get; set; }
        
        public string StudentId { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; }
        
        public RegistrationStatus Status { get; set; }
        
        // Final grade for this course
        public string? Grade { get; set; }
        
        // Navigation properties
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual ApplicationUser? Student { get; set; }
    }
    
    public enum RegistrationStatus
    {
        Pending,
        Approved,
        Rejected,
        Withdrawn
    }
    
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }
        
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? SubmissionDate { get; set; }
        
        // Path to the submitted file
        public string? FilePath { get; set; }
        
        // Student who submitted this assignment
        public string? StudentId { get; set; }
        
        // Grade given by faculty
        public double? Grade { get; set; }
        
        // Feedback from faculty
        public string? Feedback { get; set; }
        
        // Navigation properties
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual ApplicationUser? Student { get; set; }
    }
    
    public class FeeRecord
    {
        [Key]
        public int FeeId { get; set; }
        
        public string StudentId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Semester { get; set; }
        
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }
        
        public FeeStatus Status { get; set; }
        
        // Payment reference number if paid
        public string? PaymentReference { get; set; }
        
        // Navigation property
        [ForeignKey("StudentId")]
        public virtual ApplicationUser? Student { get; set; }
    }
    
    public enum FeeStatus
    {
        Pending,
        Paid,
        Overdue,
        Waived
    }
} 