using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthWebApp.Data;
using AuthWebApp.Models;
using AuthWebApp.ViewModels;

namespace AuthWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalStudents = await _userManager.GetUsersInRoleAsync("Student");
            var totalFaculty = await _userManager.GetUsersInRoleAsync("Faculty");
            
            var totalCourses = await _context.Courses.CountAsync();
            var totalRegistrations = await _context.CourseRegistrations.CountAsync();
            
            // Calculate fee statistics
            var feeRecords = await _context.FeeRecords.ToListAsync();
            var totalFeesCollected = feeRecords
                .Where(f => f.Status == FeeStatus.Paid)
                .Sum(f => f.Amount);
                
            var totalOutstandingFees = feeRecords
                .Where(f => f.Status == FeeStatus.Pending || f.Status == FeeStatus.Overdue)
                .Sum(f => f.Amount);
                
            var model = new AdminDashboardViewModel
            {
                TotalStudents = totalStudents.Count,
                TotalFaculty = totalFaculty.Count,
                TotalCourses = totalCourses,
                TotalRegistrations = totalRegistrations,
                TotalFeesCollected = totalFeesCollected,
                TotalOutstandingFees = totalOutstandingFees
            };
            
            return View(model);
        }
        
        #region User Management
        
        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var faculty = await _userManager.GetUsersInRoleAsync("Faculty");
            
            var model = new UserManagementViewModel
            {
                Students = students,
                Faculty = faculty
            };
            
            return View(model);
        }
        
        // GET: /Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View(new CreateUserViewModel());
        }
        
        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role
                };
                
                // Set role-specific properties
                switch (model.Role)
                {
                    case UserRole.Student:
                        user.StudentId = model.StudentId ?? $"S{new Random().Next(10000, 99999)}";
                        user.Program = model.Program;
                        user.Semester = model.Semester;
                        break;
                    case UserRole.Faculty:
                        user.FacultyId = model.FacultyId ?? $"F{new Random().Next(10000, 99999)}";
                        user.Department = model.Department;
                        user.Position = model.Position;
                        break;
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Create roles if they don't exist
                    string roleName = model.Role.ToString();
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                    
                    // Add user to role
                    await _userManager.AddToRoleAsync(user, roleName);
                    
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            return View(model);
        }
        
        // GET: /Admin/EditUser/{id}
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            
            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = roles.FirstOrDefault(),
                StudentId = user.StudentId,
                Program = user.Program,
                Semester = user.Semester,
                FacultyId = user.FacultyId,
                Department = user.Department,
                Position = user.Position
            };
            
            return View(model);
        }
        
        // POST: /Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }
                
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;
                
                // Update student-specific properties
                if (model.Role == "Student")
                {
                    user.StudentId = model.StudentId;
                    user.Program = model.Program;
                    user.Semester = model.Semester;
                }
                // Update faculty-specific properties
                else if (model.Role == "Faculty")
                {
                    user.FacultyId = model.FacultyId;
                    user.Department = model.Department;
                    user.Position = model.Position;
                }
                
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Users));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            return View(model);
        }
        
        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Users));
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return RedirectToAction(nameof(Users));
        }
        
        #endregion
        
        #region Course Management
        
        // GET: /Admin/Courses
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Faculty)
                .ToListAsync();
                
            var availableFaculty = await _userManager.GetUsersInRoleAsync("Faculty");
            
            var model = new CourseManagementViewModel
            {
                Courses = courses,
                AvailableFaculty = availableFaculty
            };
            
            return View(model);
        }
        
        // GET: /Admin/CreateCourse
        public async Task<IActionResult> CreateCourse()
        {
            var availableFaculty = await _userManager.GetUsersInRoleAsync("Faculty");
            ViewBag.AvailableFaculty = availableFaculty;
            
            return View(new CreateCourseViewModel());
        }
        
        // POST: /Admin/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CreateCourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = new Course
                {
                    CourseCode = model.CourseCode,
                    Title = model.Title,
                    Description = model.Description,
                    CreditHours = model.CreditHours,
                    Semester = model.Semester,
                    MaxCapacity = model.MaxCapacity,
                    Prerequisites = model.Prerequisites,
                    FacultyId = model.FacultyId
                };
                
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Courses));
            }
            
            var availableFaculty = await _userManager.GetUsersInRoleAsync("Faculty");
            ViewBag.AvailableFaculty = availableFaculty;
            
            return View(model);
        }
        
        // GET: /Admin/EditCourse/{id}
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            
            var model = new EditCourseViewModel
            {
                CourseId = course.CourseId,
                CourseCode = course.CourseCode,
                Title = course.Title,
                Description = course.Description,
                CreditHours = course.CreditHours,
                Semester = course.Semester,
                MaxCapacity = course.MaxCapacity,
                Prerequisites = course.Prerequisites,
                FacultyId = course.FacultyId
            };
            
            var availableFaculty = await _userManager.GetUsersInRoleAsync("Faculty");
            ViewBag.AvailableFaculty = availableFaculty;
            
            return View(model);
        }
        
        // POST: /Admin/EditCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(EditCourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course == null)
                {
                    return NotFound();
                }
                
                course.CourseCode = model.CourseCode;
                course.Title = model.Title;
                course.Description = model.Description;
                course.CreditHours = model.CreditHours;
                course.Semester = model.Semester;
                course.MaxCapacity = model.MaxCapacity;
                course.Prerequisites = model.Prerequisites;
                course.FacultyId = model.FacultyId;
                
                _context.Update(course);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Courses));
            }
            
            var availableFaculty = await _userManager.GetUsersInRoleAsync("Faculty");
            ViewBag.AvailableFaculty = availableFaculty;
            
            return View(model);
        }
        
        // POST: /Admin/DeleteCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Courses));
        }
        
        #endregion
        
        #region Reports
        
        // GET: /Admin/RegistrationReport
        public async Task<IActionResult> RegistrationReport(string semester = null)
        {
            // If no semester is specified, use the current one
            if (string.IsNullOrEmpty(semester))
            {
                // Get the most recent semester from courses
                semester = await _context.Courses
                    .OrderByDescending(c => c.Semester)
                    .Select(c => c.Semester)
                    .FirstOrDefaultAsync() ?? "Current";
            }
            
            // Get all courses for the selected semester
            var courses = await _context.Courses
                .Where(c => c.Semester == semester)
                .ToListAsync();
                
            // Get registration counts for each course
            var courseReports = new List<CourseRegistrationReport>();
            
            foreach (var course in courses)
            {
                var registeredCount = await _context.CourseRegistrations
                    .CountAsync(cr => cr.CourseId == course.CourseId && cr.Status == RegistrationStatus.Approved);
                    
                var report = new CourseRegistrationReport
                {
                    CourseId = course.CourseId,
                    CourseCode = course.CourseCode,
                    CourseTitle = course.Title,
                    TotalSeats = course.MaxCapacity,
                    RegisteredStudents = registeredCount,
                    AvailableSeats = course.MaxCapacity - registeredCount,
                    FillRate = (registeredCount * 100.0) / course.MaxCapacity
                };
                
                courseReports.Add(report);
            }
            
            // Get all available semesters for dropdown
            var availableSemesters = await _context.Courses
                .Select(c => c.Semester)
                .Distinct()
                .ToListAsync();
                
            ViewBag.AvailableSemesters = availableSemesters;
            
            var model = new RegistrationReportViewModel
            {
                Semester = semester,
                CourseRegistrations = courseReports
            };
            
            return View(model);
        }
        
        // GET: /Admin/FeeReport
        public async Task<IActionResult> FeeReport(string semester = null)
        {
            // If no semester is specified, use the current one
            if (string.IsNullOrEmpty(semester))
            {
                // Get the most recent semester from fee records
                semester = await _context.FeeRecords
                    .OrderByDescending(f => f.Semester)
                    .Select(f => f.Semester)
                    .FirstOrDefaultAsync() ?? "Current";
            }
            
            // Get all fee records for the selected semester
            var feeRecords = await _context.FeeRecords
                .Include(f => f.Student)
                .Where(f => f.Semester == semester)
                .ToListAsync();
                
            // Calculate fee statistics
            var totalCharged = feeRecords.Sum(f => f.Amount);
            var totalPaid = feeRecords
                .Where(f => f.Status == FeeStatus.Paid)
                .Sum(f => f.Amount);
                
            var totalOutstanding = feeRecords
                .Where(f => f.Status == FeeStatus.Pending || f.Status == FeeStatus.Overdue)
                .Sum(f => f.Amount);
                
            var collectionRate = totalCharged > 0 ? (totalPaid * 100.0m) / totalCharged : 0;
            
            // Group fee records by student
            var feeDetails = feeRecords
                .GroupBy(f => f.StudentId)
                .Select(g => new FeeReportDetail
                {
                    StudentId = g.Key,
                    StudentName = $"{g.First().Student.FirstName} {g.First().Student.LastName}",
                    AmountCharged = g.Sum(f => f.Amount),
                    AmountPaid = g.Where(f => f.Status == FeeStatus.Paid).Sum(f => f.Amount),
                    AmountOutstanding = g.Where(f => f.Status == FeeStatus.Pending || f.Status == FeeStatus.Overdue).Sum(f => f.Amount),
                    Status = g.Any(f => f.Status == FeeStatus.Overdue) ? FeeStatus.Overdue :
                            g.Any(f => f.Status == FeeStatus.Pending) ? FeeStatus.Pending : FeeStatus.Paid
                })
                .ToList();
                
            // Get all available semesters for dropdown
            var availableSemesters = await _context.FeeRecords
                .Select(f => f.Semester)
                .Distinct()
                .ToListAsync();
                
            ViewBag.AvailableSemesters = availableSemesters;
            
            var model = new FeeReportViewModel
            {
                Semester = semester,
                TotalFeesCharged = totalCharged,
                TotalFeesPaid = totalPaid,
                TotalFeesOutstanding = totalOutstanding,
                CollectionRate = (double)collectionRate,
                FeeDetails = feeDetails
            };
            
            return View(model);
        }
        
        #endregion
    }
} 