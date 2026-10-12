using BusinessCRM.Data;
using BusinessCRM.Models;
using BusinessCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessCRM.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(UserRoles.SuperAdmin) || roles.Contains(UserRoles.Admin) || roles.Contains(UserRoles.Manager))
            {
                return RedirectToAction(nameof(Admin));
            }

            return RedirectToAction(nameof(Employee));
        }

        // GET: /Dashboard/Admin
        [Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Admin}")]
        public async Task<IActionResult> Admin()
        {
            var totalEmployees = await _userManager.Users.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var recentCustomers = await _context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalEmployees = totalEmployees,
                TotalCustomers = totalCustomers,
                TodayClockedIn = 0, // Placeholder for Phase 5 Attendance module
                PendingTasks = 0,    // Placeholder for Phase 7 Task module
                RecentCustomers = recentCustomers
            };

            return View(viewModel);
        }

        // GET: /Dashboard/Employee
        [Authorize]
        public async Task<IActionResult> Employee()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var viewModel = new EmployeeDashboardViewModel
            {
                FullName = user.FullName,
                UserName = user.UserName ?? "User",
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? UserRoles.Employee,
                IsClockedIn = false, // Placeholder for Phase 5 Attendance module
                ClockInTime = null,
                AssignedTasksCount = 0
            };

            return View(viewModel);
        }
    }
}
