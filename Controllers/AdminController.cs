using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WhiteSoft.Models;
using WhiteSoft.Services;

namespace WhiteSoft.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class AdminController : Controller
    {
        //* Superadmin could manage all users (including admins) and roles in the system
        // Including editing users, adding new articles, or even look to app logs. *//

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService _logService;
        private readonly ApplicationDbContext _insuranceContext;

        public AdminController(UserManager<ApplicationUser> userManager,
                               ApplicationDbContext context,
                               RoleManager<IdentityRole> roleManager,
                               ILogService logService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
            _insuranceContext = context;
        }

        [HttpGet]
        [ActionName("Index")]
        public async Task<IActionResult> ManageAdmins()
        {
            var admins = await _userManager.GetUsersInRoleAsync("admin");
            var superadmins = await _userManager.GetUsersInRoleAsync("superadmin");

            var combinedUsers = admins.Union(superadmins).ToList();

            var model = new List<AdminUserViewModel>();

            foreach (var user in combinedUsers)
            {
                // XSS protection
                user.ApplyXssProtection();

                var isSuperAdmin = await _userManager.IsInRoleAsync(user, "superadmin");
                var isAdmin = await _userManager.IsInRoleAsync(user, "admin") || isSuperAdmin;

                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    IsAdmin = isAdmin,
                    IsSuperAdmin = isSuperAdmin
                });
            }

            return View(model);

        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var model = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                var userId = await _userManager.FindByIdAsync(user.Id);
                var isSuperAdmin = await _userManager.IsInRoleAsync(userId!, "superadmin");
                var isAdmin = await _userManager.IsInRoleAsync(userId!, "admin") || isSuperAdmin;

                // XSS protection
                user.ApplyXssProtection();

                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    IsAdmin = isAdmin,
                    IsSuperAdmin = isSuperAdmin
                });
            }

            return View(model);
        }

        [HttpPost("promote/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string id)
        {
            var user = await LoadUserAsync(id);
            if (user == null) return NotFound();

            // XSS protection
            user.ApplyXssProtection();

            // Verify, if the role "admin" exists
            if (!await _roleManager.RoleExistsAsync("admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("admin"));
            }

            // Adds the role admin
            await _userManager.AddToRoleAsync(user, "admin");
            await _userManager.AddToRoleAsync(user, "admin");
            await _logService.LogAsync("Warning", $"Uživatel {user.UserName} byl povýšen na admina.", User.Identity?.Name);

            TempData["Warning"] = $"Uživatel {user.UserName} povýšen na admina.";
            return RedirectToAction("ManageUsers");
        }
        [HttpPost]
        public async Task<IActionResult> DemoteFromAdmin(string id)
        {
            var user = await LoadUserAsync(id);
            if (user == null) return NotFound();

            // XSS protection
            user.ApplyXssProtection();

            await _userManager.RemoveFromRoleAsync(user, "admin");

            await _logService.LogAsync("Warning", $"Adminovi {user.UserName} byla odebrána admin práva.", User.Identity?.Name);

            TempData["Warning"] = $"Admin role odebrána uživateli {user.UserName}.";
            return RedirectToAction("Index");
        }


        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await LoadUserAsync(id);
            if (user == null) return NotFound();

            // XSS protection
            user.ApplyXssProtection();

            await _userManager.DeleteAsync(user);
            await _logService.LogAsync("Warning", $"Uživatel {user.UserName} byl smazán.", User.Identity?.Name);

            TempData["Warning"] = $"Uživatel {user.UserName} byl odstraněn.";
            return RedirectToAction("Index");
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await LoadUserAsync(id);
            if (user == null) return NotFound();

            // XSS protection
            user.ApplyXssProtection();

            var model = new EditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            };

            ViewBag.IsAdminEditing = true;
            return View("~/Views/Auth/Edit.cshtml", model); // It using the same view as edit method
        }



        [ActionName("Create")]
        public IActionResult CreateUserByAdmin(string role)
        {
            var model = new RegisterViewModel
            {
                Role = role // Superadmin could registering "user" or "admin"
            };

            ViewBag.IsAdminCreating = true; // For form or view logic
            return View("~/Views/Auth/Register.cshtml", model); // It using the same view as register method
        }

        public async Task<IActionResult> Logs()
        {
            var logs = await _insuranceContext.LogEntries.OrderByDescending(l => l.Timestamp).Take(100).ToListAsync();
            return View(logs);
        }

        private async Task<ApplicationUser?> LoadUserAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

    }
}
