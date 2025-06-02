using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using AuthWebApp.Data;
using AuthWebApp.Models;
using AuthWebApp.ViewModels;

namespace AuthWebApp.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FacultyController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Faculty/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Get faculty's courses
            var courses = await _context.Courses
                .Where(c => c.FacultyId == faculty.Id)
                .ToListAsync();
            
            // Get total number of students enrolled in faculty's courses
            var totalStudents = await _context.CourseRegistrations
                .Where(cr => cr.Course.FacultyId == faculty.Id && cr.Status == RegistrationStatus.Approved)
                .Select(cr => cr.StudentId)
                .Distinct()
                .CountAsync();
            
            // Get count of pending assignments (submitted but not graded)
            var pendingAssignments = await _context.Assignments
                .Where(a => a.Course.FacultyId == faculty.Id && a.SubmissionDate.HasValue && !a.Grade.HasValue)
                .CountAsync();
            
            // Get recent announcements
            var recentAnnouncements = await _context.Announcements
                .Include(a => a.Course)
                .Where(a => a.FacultyId == faculty.Id)
                .OrderByDescending(a => a.PostedDate)
                .Take(5)
                .ToListAsync();
            
            var model = new FacultyDashboardViewModel
            {
                Faculty = faculty,
                Courses = courses,
                TotalStudents = totalStudents,
                PendingAssignments = pendingAssignments,
                RecentAnnouncements = recentAnnouncements
            };
            
            return View(model);
        }

        // GET: /Faculty/MyCourses
        public async Task<IActionResult> MyCourses()
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            var courses = await _context.Courses
                .Where(c => c.FacultyId == faculty.Id)
                .ToListAsync();
            
            var model = new FacultyCoursesViewModel
            {
                Courses = courses
            };
            
            return View(model);
        }

        // GET: /Faculty/CourseDetails/{id}
        public async Task<IActionResult> CourseDetails(int id)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            // Get enrolled students
            var enrolledStudents = await _context.CourseRegistrations
                .Where(cr => cr.CourseId == id && cr.Status == RegistrationStatus.Approved)
                .Include(cr => cr.Student)
                .Select(cr => cr.Student)
                .ToListAsync();
            
            // Get assignments
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == id)
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();
            
            // Get course content
            var courseContents = await _context.CourseContents
                .Where(cc => cc.CourseId == id)
                .OrderByDescending(cc => cc.UploadDate)
                .ToListAsync();
            
            // Get announcements
            var announcements = await _context.Announcements
                .Where(a => a.CourseId == id)
                .OrderByDescending(a => a.PostedDate)
                .ToListAsync();
            
            var model = new CourseDetailsViewModel
            {
                Course = course,
                EnrolledStudents = enrolledStudents,
                Assignments = assignments,
                CourseContents = courseContents,
                Announcements = announcements
            };
            
            return View(model);
        }
        
        #region Assignments
        
        // GET: /Faculty/CreateAssignment/{courseId}
        public async Task<IActionResult> CreateAssignment(int courseId)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            var model = new CreateAssignmentViewModel
            {
                CourseId = courseId,
                DueDate = DateTime.Now.AddDays(7) // Default due date one week from now
            };
            
            return View(model);
        }
        
        // POST: /Faculty/CreateAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(CreateAssignmentViewModel model)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                var assignment = new Assignment
                {
                    CourseId = model.CourseId,
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate
                };
                
                _context.Assignments.Add(assignment);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });
            }
            
            return View(model);
        }
        
        // GET: /Faculty/Assignments
        public async Task<IActionResult> Assignments()
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course.FacultyId == faculty.Id)
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();
            
            return View(assignments);
        }
        
        // GET: /Faculty/GradeAssignment/{id}
        public async Task<IActionResult> GradeAssignment(int id)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.AssignmentId == id && a.Course.FacultyId == faculty.Id);
                
            if (assignment == null || assignment.StudentId == null || !assignment.SubmissionDate.HasValue)
            {
                return NotFound();
            }
            
            var model = new GradeAssignmentViewModel
            {
                AssignmentId = assignment.AssignmentId,
                AssignmentTitle = assignment.Title,
                StudentName = $"{assignment.Student.FirstName} {assignment.Student.LastName}",
                StudentId = assignment.Student.StudentId,
                SubmissionDate = assignment.SubmissionDate,
                FilePath = assignment.FilePath,
                Grade = assignment.Grade ?? 0,
                Feedback = assignment.Feedback
            };
            
            return View(model);
        }
        
        // POST: /Faculty/GradeAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeAssignment(GradeAssignmentViewModel model)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentId == model.AssignmentId && a.Course.FacultyId == faculty.Id);
                
            if (assignment == null)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                assignment.Grade = model.Grade;
                assignment.Feedback = model.Feedback;
                
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Assignments));
            }
            
            return View(model);
        }
        
        #endregion
        
        #region Course Content
        
        // GET: /Faculty/UploadContent/{courseId}
        public async Task<IActionResult> UploadContent(int courseId)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            var model = new UploadContentViewModel
            {
                CourseId = courseId
            };
            
            return View(model);
        }
        
        // POST: /Faculty/UploadContent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadContent(UploadContentViewModel model)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid && model.File != null && model.File.Length > 0)
            {
                // Save the file to the server
                var contentType = model.Type.ToString().ToLower();
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "content", contentType);
                Directory.CreateDirectory(uploadsFolder); // Create directory if it doesn't exist
                
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.File.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }
                
                var courseContent = new CourseContent
                {
                    CourseId = model.CourseId,
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.Type,
                    FilePath = $"/uploads/content/{contentType}/{uniqueFileName}",
                    UploadDate = DateTime.Now
                };
                
                _context.CourseContents.Add(courseContent);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });
            }
            
            return View(model);
        }
        
        #endregion
        
        #region Announcements
        
        // GET: /Faculty/CreateAnnouncement/{courseId}
        public async Task<IActionResult> CreateAnnouncement(int courseId)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            var model = new CreateAnnouncementViewModel
            {
                CourseId = courseId
            };
            
            return View(model);
        }
        
        // POST: /Faculty/CreateAnnouncement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(CreateAnnouncementViewModel model)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                var announcement = new Announcement
                {
                    CourseId = model.CourseId,
                    Title = model.Title,
                    Content = model.Content,
                    FacultyId = faculty.Id,
                    PostedDate = DateTime.Now
                };
                
                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });
            }
            
            return View(model);
        }
        
        #endregion
        
        #region Attendance
        
        // GET: /Faculty/RecordAttendance/{courseId}
        public async Task<IActionResult> RecordAttendance(int courseId)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            // Get enrolled students
            var enrolledStudents = await _context.CourseRegistrations
                .Where(cr => cr.CourseId == courseId && cr.Status == RegistrationStatus.Approved)
                .Include(cr => cr.Student)
                .Select(cr => cr.Student)
                .ToListAsync();
            
            var students = enrolledStudents.Select(s => new StudentAttendanceViewModel
            {
                StudentId = s.Id,
                StudentName = $"{s.FirstName} {s.LastName}",
                Status = AttendanceStatus.Present, // Default to present
                Remarks = ""
            }).ToList();
            
            var model = new RecordAttendanceViewModel
            {
                CourseId = courseId,
                CourseName = course.Title,
                Date = DateTime.Today,
                Students = students
            };
            
            return View(model);
        }
        
        // POST: /Faculty/RecordAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordAttendance(RecordAttendanceViewModel model)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                // Check if attendance already exists for this date and course
                var existingAttendance = await _context.Attendances
                    .Where(a => a.CourseId == model.CourseId && a.Date.Date == model.Date.Date)
                    .ToListAsync();
                
                if (existingAttendance.Any())
                {
                    // Delete existing attendance records
                    _context.Attendances.RemoveRange(existingAttendance);
                }
                
                // Save new attendance records
                foreach (var student in model.Students)
                {
                    var attendance = new Attendance
                    {
                        CourseId = model.CourseId,
                        StudentId = student.StudentId,
                        Date = model.Date,
                        Status = student.Status,
                        Remarks = student.Remarks
                    };
                    
                    _context.Attendances.Add(attendance);
                }
                
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });
            }
            
            return View(model);
        }
        
        // GET: /Faculty/AttendanceReport/{courseId}
        public async Task<IActionResult> AttendanceReport(int courseId)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            // Get enrolled students
            var enrolledStudents = await _context.CourseRegistrations
                .Where(cr => cr.CourseId == courseId && cr.Status == RegistrationStatus.Approved)
                .Include(cr => cr.Student)
                .Select(cr => cr.Student)
                .ToListAsync();
            
            // Get all attendance records for the course
            var attendanceRecords = await _context.Attendances
                .Where(a => a.CourseId == courseId)
                .ToListAsync();
            
            // Calculate attendance statistics for each student
            var totalClasses = attendanceRecords.Select(a => a.Date.Date).Distinct().Count();
            var attendanceReports = new List<AttendanceReportItemViewModel>();
            
            foreach (var student in enrolledStudents)
            {
                var studentAttendance = attendanceRecords
                    .Where(a => a.StudentId == student.Id)
                    .ToList();
                
                var present = studentAttendance.Count(a => a.Status == AttendanceStatus.Present);
                var absent = studentAttendance.Count(a => a.Status == AttendanceStatus.Absent);
                var late = studentAttendance.Count(a => a.Status == AttendanceStatus.Late);
                var excused = studentAttendance.Count(a => a.Status == AttendanceStatus.Excused);
                
                var attendancePercentage = totalClasses > 0 
                    ? (double)(present + late + excused) / totalClasses * 100 
                    : 0;
                
                attendanceReports.Add(new AttendanceReportItemViewModel
                {
                    StudentId = student.StudentId,
                    StudentName = $"{student.FirstName} {student.LastName}",
                    TotalClasses = totalClasses,
                    Present = present,
                    Absent = absent,
                    Late = late,
                    Excused = excused,
                    AttendancePercentage = Math.Round(attendancePercentage, 2)
                });
            }
            
            var model = new AttendanceReportViewModel
            {
                CourseId = courseId,
                CourseName = course.Title,
                AttendanceReports = attendanceReports
            };
            
            return View(model);
        }
        
        #endregion

        #region Grades
        
        // GET: /Faculty/Grades
        public async Task<IActionResult> Grades()
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Get all courses taught by this faculty
            var courses = await _context.Courses
                .Where(c => c.FacultyId == faculty.Id)
                .ToListAsync();
                
            var courseGradesList = new List<CourseGradesViewModel>();
            
            foreach (var course in courses)
            {
                // Get all students enrolled in this course
                var enrolledStudents = await _context.CourseRegistrations
                    .Where(cr => cr.CourseId == course.CourseId && cr.Status == RegistrationStatus.Approved)
                    .Include(cr => cr.Student)
                    .ToListAsync();
                
                // Get all assignments for this course
                var assignments = await _context.Assignments
                    .Where(a => a.CourseId == course.CourseId)
                    .ToListAsync();
                
                var studentGrades = new List<StudentGradeViewModel>();
                
                foreach (var enrollment in enrolledStudents)
                {
                    // Get all assignments submitted by this student for this course
                    var studentAssignments = assignments
                        .Where(a => a.StudentId == enrollment.StudentId)
                        .ToList();
                    
                    var assignmentGrades = new List<AssignmentGradeViewModel>();
                    double totalGrade = 0;
                    int gradedAssignmentsCount = 0;
                    
                    foreach (var assignment in assignments)
                    {
                        var studentAssignment = studentAssignments
                            .FirstOrDefault(sa => sa.AssignmentId == assignment.AssignmentId);
                        
                        var grade = studentAssignment?.Grade;
                        
                        assignmentGrades.Add(new AssignmentGradeViewModel
                        {
                            AssignmentId = assignment.AssignmentId,
                            AssignmentTitle = assignment.Title,
                            Grade = grade
                        });
                        
                        if (grade.HasValue)
                        {
                            totalGrade += grade.Value;
                            gradedAssignmentsCount++;
                        }
                    }
                    
                    // Calculate average grade
                    double averageGrade = gradedAssignmentsCount > 0 ? totalGrade / gradedAssignmentsCount : 0;
                    
                    studentGrades.Add(new StudentGradeViewModel
                    {
                        StudentId = enrollment.StudentId,
                        StudentName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
                        AssignmentGrades = assignmentGrades,
                        AverageGrade = Math.Round(averageGrade, 2),
                        FinalGrade = enrollment.Grade ?? ""
                    });
                }
                
                courseGradesList.Add(new CourseGradesViewModel
                {
                    CourseId = course.CourseId,
                    CourseCode = course.CourseCode,
                    CourseTitle = course.Title,
                    StudentGrades = studentGrades
                });
            }
            
            var model = new GradesViewModel
            {
                Courses = courseGradesList
            };
            
            return View(model);
        }
        
        // POST: /Faculty/UpdateFinalGrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFinalGrade(UpdateFinalGradeViewModel model)
        {
            var faculty = await GetCurrentFacultyUserAsync();
            
            // Verify the course belongs to this faculty
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId && c.FacultyId == faculty.Id);
                
            if (course == null)
            {
                return NotFound();
            }
            
            // Find the course registration
            var registration = await _context.CourseRegistrations
                .FirstOrDefaultAsync(cr => cr.CourseId == model.CourseId && 
                                          cr.StudentId == model.StudentId && 
                                          cr.Status == RegistrationStatus.Approved);
                
            if (registration == null)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                registration.Grade = model.FinalGrade;
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Grades));
            }
            
            return RedirectToAction(nameof(Grades));
        }
        
        #endregion

        // Helper method to get the current faculty user
        private async Task<ApplicationUser> GetCurrentFacultyUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException("Unable to load faculty user.");
            }
            return user;
        }
    }
} 