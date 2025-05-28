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
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Student/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await GetCurrentStudentUserAsync();
            
            // Get student's courses
            var registeredCourses = await _context.CourseRegistrations
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == user.Id && cr.Status == RegistrationStatus.Approved)
                .ToListAsync();
            
            // Get pending assignments
            var pendingAssignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.StudentId == user.Id && a.SubmissionDate == null && a.DueDate >= DateTime.Today)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
            
            // Get fee records
            var fees = await _context.FeeRecords
                .Where(f => f.StudentId == user.Id && f.Status != FeeStatus.Paid)
                .OrderBy(f => f.DueDate)
                .ToListAsync();
            
            // Create the dashboard view model
            var model = new StudentDashboardViewModel
            {
                Student = user,
                RegisteredCourses = registeredCourses,
                PendingAssignments = pendingAssignments,
                OutstandingFees = fees
            };
            
            return View(model);
        }

        // GET: /Student/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentStudentUserAsync();
            return View(user);
        }
        
        // GET: /Student/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var user = await GetCurrentStudentUserAsync();
            
            var model = new EditStudentProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                StudentId = user.StudentId,
                Program = user.Program,
                Semester = user.Semester
            };
            
            return View(model);
        }
        
        // POST: /Student/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditStudentProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentStudentUserAsync();
                
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Program = model.Program;
                user.Semester = model.Semester;
                
                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Profile));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            return View(model);
        }
        
        // GET: /Student/Courses
        public async Task<IActionResult> Courses()
        {
            var user = await GetCurrentStudentUserAsync();
            
            var registeredCourses = await _context.CourseRegistrations
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == user.Id)
                .ToListAsync();
            
            return View(registeredCourses);
        }
        
        // GET: /Student/CourseRegistration
        public async Task<IActionResult> CourseRegistration()
        {
            var user = await GetCurrentStudentUserAsync();
            
            // Get all available courses
            var courses = await _context.Courses
                .Where(c => c.Semester == user.Semester.ToString())
                .ToListAsync();
            
            // Get IDs of courses student is already registered for
            var registeredCourseIds = await _context.CourseRegistrations
                .Where(cr => cr.StudentId == user.Id)
                .Select(cr => cr.CourseId)
                .ToListAsync();
            
            // Filter out courses student is already registered for
            var availableCourses = courses.Where(c => !registeredCourseIds.Contains(c.CourseId)).ToList();
            
            return View(availableCourses);
        }
        
        // POST: /Student/RegisterCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCourse(int courseId)
        {
            var user = await GetCurrentStudentUserAsync();
            
            // Check if course exists
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }
            
            // Check if student is already registered for this course
            var existingRegistration = await _context.CourseRegistrations
                .FirstOrDefaultAsync(cr => cr.StudentId == user.Id && cr.CourseId == courseId);
                
            if (existingRegistration != null)
            {
                ModelState.AddModelError(string.Empty, "You are already registered for this course.");
                return RedirectToAction(nameof(CourseRegistration));
            }
            
            // Check course prerequisites
            if (!string.IsNullOrEmpty(course.Prerequisites))
            {
                var prerequisiteIds = course.Prerequisites.Split(',').Select(int.Parse).ToList();
                var completedCourseIds = await _context.CourseRegistrations
                    .Where(cr => cr.StudentId == user.Id && cr.Status == RegistrationStatus.Approved && !string.IsNullOrEmpty(cr.Grade))
                    .Select(cr => cr.CourseId)
                    .ToListAsync();
                
                if (!prerequisiteIds.All(id => completedCourseIds.Contains(id)))
                {
                    ModelState.AddModelError(string.Empty, "You do not meet the prerequisites for this course.");
                    return RedirectToAction(nameof(CourseRegistration));
                }
            }
            
            // Check if course has reached maximum capacity
            var currentRegistrations = await _context.CourseRegistrations
                .CountAsync(cr => cr.CourseId == courseId && (cr.Status == RegistrationStatus.Approved || cr.Status == RegistrationStatus.Pending));
                
            if (currentRegistrations >= course.MaxCapacity)
            {
                ModelState.AddModelError(string.Empty, "This course has reached its maximum capacity.");
                return RedirectToAction(nameof(CourseRegistration));
            }
            
            // Create the registration
            var registration = new CourseRegistration
            {
                CourseId = courseId,
                StudentId = user.Id,
                RegistrationDate = DateTime.Now,
                Status = RegistrationStatus.Pending
            };
            
            _context.CourseRegistrations.Add(registration);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Courses));
        }
        
        // GET: /Student/Assignments
        public async Task<IActionResult> Assignments()
        {
            var user = await GetCurrentStudentUserAsync();
            
            // Get assignments for the student's registered courses
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course.Registrations.Any(cr => cr.StudentId == user.Id && cr.Status == RegistrationStatus.Approved))
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();
            
            return View(assignments);
        }
        
        // GET: /Student/SubmitAssignment/{id}
        public async Task<IActionResult> SubmitAssignment(int id)
        {
            var user = await GetCurrentStudentUserAsync();
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);
                
            if (assignment == null)
            {
                return NotFound();
            }
            
            // Check if the student is registered for this course
            var isRegistered = await _context.CourseRegistrations
                .AnyAsync(cr => cr.StudentId == user.Id && cr.CourseId == assignment.CourseId && cr.Status == RegistrationStatus.Approved);
                
            if (!isRegistered)
            {
                return Forbid();
            }
            
            // Check if the student has already submitted this assignment
            if (assignment.StudentId == user.Id && assignment.SubmissionDate.HasValue)
            {
                return RedirectToAction(nameof(Assignments));
            }
            
            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = assignment.AssignmentId,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                CourseTitle = assignment.Course.Title
            };
            
            return View(model);
        }
        
        // POST: /Student/SubmitAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignment(SubmitAssignmentViewModel model)
        {
            var user = await GetCurrentStudentUserAsync();
            
            var assignment = await _context.Assignments.FindAsync(model.AssignmentId);
            if (assignment == null)
            {
                return NotFound();
            }
            
            if (model.File != null && model.File.Length > 0)
            {
                // Save the file to the server
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "assignments");
                Directory.CreateDirectory(uploadsFolder); // Create directory if it doesn't exist
                
                var uniqueFileName = $"{user.Id}_{assignment.AssignmentId}_{Path.GetFileName(model.File.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }
                
                // Update the assignment
                assignment.FilePath = $"/uploads/assignments/{uniqueFileName}";
                assignment.StudentId = user.Id;
                assignment.SubmissionDate = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Assignments));
            }
            
            ModelState.AddModelError(string.Empty, "Please select a file to upload.");
            return View(model);
        }
        
        // GET: /Student/Fees
        public async Task<IActionResult> Fees()
        {
            var user = await GetCurrentStudentUserAsync();
            
            var fees = await _context.FeeRecords
                .Where(f => f.StudentId == user.Id)
                .OrderByDescending(f => f.DueDate)
                .ToListAsync();
            
            return View(fees);
        }
        
        // Helper method to get the current student user
        private async Task<ApplicationUser> GetCurrentStudentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null || user.Role != UserRole.Student)
            {
                throw new InvalidOperationException("Current user is not a student");
            }
            
            return user;
        }
    }
} 