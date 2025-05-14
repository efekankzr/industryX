using IndustryX.Domain.Entities;
using IndustryX.Infrastructure.Interfaces;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

            if (result.Succeeded)
            {
                ShowAlert("Welcome!", "You have successfully logged in.", "success");
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
                ShowAlert("Account Locked", "Your account is temporarily locked due to failed attempts.", "danger");
            else
                ShowAlert("Login Failed", "Invalid email or password.", "danger");

            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Firstname = model.Firstname,
                Surname = model.Surname
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Email Confirmation - IndustryX",
                    $"<p>Please confirm your email by <a href='{confirmationLink}'>clicking here</a>.</p>"
                );

                ShowAlert("Registration Successful", "Please check your email to confirm your account.", "info");
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ShowAlert("Error", error.Description, "danger");

            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ShowAlert("Email Confirmed", "Your email has been successfully verified.", "success");
                return RedirectToAction("Login");
            }

            ShowAlert("Confirmation Failed", "The confirmation link is invalid or expired.", "danger");
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            ShowAlert("Goodbye!", "You have been logged out.", "info");
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            ShowAlert("Access Denied", "You are not authorized to access this page.", "danger");
            return RedirectToAction("Login");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                ShowAlert("Notice", "If the email exists and is confirmed, a reset link has been sent.", "info");
                return RedirectToAction("Login");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

            await _emailService.SendEmailAsync(
                model.Email,
                "Reset Your Password - IndustryX",
                $"<p>To reset your password, <a href='{resetLink}'>click here</a>.</p>"
            );

            ShowAlert("Check Email", "Password reset instructions have been sent.", "info");
            return RedirectToAction("Login");
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return BadRequest();

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ShowAlert("Error", "Invalid request.", "danger");
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                ShowAlert("Password Reset", "Your password has been successfully reset.", "success");
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ShowAlert("Error", error.Description, "danger");

            return View(model);
        }
    }
}
