using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using AuthWebApp.Models;

namespace AuthWebApp.ViewModels
{
    public class FacultyDashboardViewModel
    {
        public ApplicationUser Faculty { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public int TotalStudents { get; set; }
        public int PendingAssignments { get; set; }
        public IEnumerable<Announcement> RecentAnnouncements { get; set; }
    }
    
    public class FacultyCoursesViewModel
    {
        public IEnumerable<Course> Courses { get; set; }
    }
    
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; }
        public IEnumerable<ApplicationUser> EnrolledStudents { get; set; }
        public IEnumerable<Assignment> Assignments { get; set; }
        public IEnumerable<CourseContent> CourseContents { get; set; }
        public IEnumerable<Announcement> Announcements { get; set; }
    }
    
    public class CreateAssignmentViewModel
    {
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
    }
    
    public class GradeAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public string FilePath { get; set; }
        
        [Required]
        [Range(0, 100)]
        public double Grade { get; set; }
        
        [StringLength(500)]
        public string Feedback { get; set; }
    }
    
    public class UploadContentViewModel
    {
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public ContentType Type { get; set; }
        
        [Required]
        public IFormFile File { get; set; }
    }
    
    public class CreateAnnouncementViewModel
    {
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
    }
    
    public class RecordAttendanceViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        
        public List<StudentAttendanceViewModel> Students { get; set; }
    }
    
    public class StudentAttendanceViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public AttendanceStatus Status { get; set; }
        public string Remarks { get; set; }
    }
    
    public class AttendanceReportViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public List<AttendanceReportItemViewModel> AttendanceReports { get; set; }
    }
    
    public class AttendanceReportItemViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public int TotalClasses { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int Excused { get; set; }
        public double AttendancePercentage { get; set; }
    }
} 