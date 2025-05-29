using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace AuthWebApp.Models
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime PostedDate { get; set; }
        
        // Faculty who posted the announcement
        public string FacultyId { get; set; }
        
        // Navigation properties
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        
        [ForeignKey("FacultyId")]
        public virtual ApplicationUser Faculty { get; set; }
    }
    
    public class CourseContent
    {
        [Key]
        public int ContentId { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        // Type of content (lecture notes, slides, video, etc.)
        [Required]
        public ContentType Type { get; set; }
        
        // Path to the content file
        [Required]
        public string FilePath { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime UploadDate { get; set; }
        
        // Navigation property
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
    
    public enum ContentType
    {
        LectureNotes,
        Slides,
        Video,
        Assignment,
        Other
    }
    
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        public string StudentId { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        
        [Required]
        public AttendanceStatus Status { get; set; }
        
        // Optional remarks
        public string Remarks { get; set; }
        
        // Navigation properties
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual ApplicationUser Student { get; set; }
    }
    
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Excused
    }
} 