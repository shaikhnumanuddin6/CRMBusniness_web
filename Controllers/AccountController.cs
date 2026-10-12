using BusinessCRM.Data;
using BusinessCRM.Models;
using BusinessCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BusinessCRM.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // If already authenticated, redirect immediately to their dashboard
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    return await RedirectToDashboardAsync(currentUser, returnUrl);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ViewData["ReturnUrl"] = model.ReturnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Locate user by Email OR UserName
            var user = await _userManager.FindByEmailAsync(model.EmailOrUsername)
                       ?? await _userManager.FindByNameAsync(model.EmailOrUsername);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials. Please try again.");
                return View(model);
            }

            // 2. Check active status
            if (!user.IsActive)
            {
                _logger.LogWarning("Deactivated user {UserName} attempted sign in.", user.UserName);
                ModelState.AddModelError(string.Empty, "This account has been deactivated. Please contact your system administrator.");
                return View(model);
            }

            // 3. Attempt password sign-in
            var result = await _signInManager.PasswordSignInAsync(
                userName: user.UserName!,
                password: model.Password,
                isPersistent: model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);
                return await RedirectToDashboardAsync(user, model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account {UserName} is locked out.", user.UserName);
                ModelState.AddModelError(string.Empty, "This account is temporarily locked due to multiple failed login attempts. Please try again later.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials. Please try again.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {UserName} signed out.", userName);

            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<IActionResult> RedirectToDashboardAsync(ApplicationUser user, string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(UserRoles.SuperAdmin) || roles.Contains(UserRoles.Admin) || roles.Contains(UserRoles.Manager))
            {
                return RedirectToAction("Admin", "Dashboard");
            }

            return RedirectToAction("Employee", "Dashboard");
        }
    }
}
