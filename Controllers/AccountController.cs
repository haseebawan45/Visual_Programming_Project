using AuthWebApp.Models;
using AuthWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AuthWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Get the user to determine their role
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        // Check their role and redirect accordingly
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Student"))
                        {
                            return RedirectToAction("Dashboard", "Student");
                        }
                        else if (roles.Contains("Faculty"))
                        {
                            return RedirectToAction("Dashboard", "Faculty");
                        }
                        else if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                    }
                    
                    // If no specific role or returnUrl, use the RedirectToLocal method
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
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
                        user.StudentId = model.StudentId ?? $"S{new System.Random().Next(10000, 99999)}";
                        user.Program = model.Program;
                        user.Semester = model.Semester;
                        break;
                    case UserRole.Faculty:
                        user.FacultyId = model.FacultyId ?? $"F{new System.Random().Next(10000, 99999)}";
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
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    // Redirect based on role
                    switch (model.Role)
                    {
                        case UserRole.Student:
                            return RedirectToAction("Dashboard", "Student");
                        case UserRole.Faculty:
                            return RedirectToAction("Dashboard", "Faculty");
                        case UserRole.Admin:
                            return RedirectToAction("Dashboard", "Admin");
                        default:
                            return RedirectToLocal(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // Get the currently signed in user
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student"))
                {
                    return RedirectToAction("Dashboard", "Student");
                }
                else if (User.IsInRole("Faculty"))
                {
                    return RedirectToAction("Dashboard", "Faculty");
                }
                else if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            
            // Default redirect
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
} 