using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TechRent.Data;
using TechRent.Models.Entities;
using TechRent.Services;

namespace TechRent.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly TechRent.Services.IEmailSender _emailSender;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context,
            TechRent.Services.IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Account/Auth (страница входа)
        [HttpGet]
        public IActionResult Auth(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Auth
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Auth(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Добавляем роль "User" всем
                    await _userManager.AddToRoleAsync(user, "User");

                    // Если пользователь выбрал "Стать поставщиком", добавляем роль "Supplier"
                    if (model.IsSupplier)
                    {
                        await _userManager.AddToRoleAsync(user, "Supplier");
                    }

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

                    try
                    {
                        await _emailSender.SendEmailAsync(
                            model.Email,
                            "Добро пожаловать в TechRent! Подтвердите email",
                            $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Добро пожаловать в TechRent, {model.FirstName}!</h2>
                        <p>Спасибо за регистрацию. Пожалуйста, подтвердите ваш email адрес, нажав на ссылку ниже:</p>
                        <p><a href='{confirmationLink}' style='background-color: #3b82f6; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Подтвердить email</a></p>
                        <p>Если кнопка не работает, скопируйте и вставьте эту ссылку в браузер:</p>
                        <p>{confirmationLink}</p>
                        <p>С уважением,<br>Команда TechRent</p>
                    </body>
                    </html>"
                        );
                        TempData["SuccessMessage"] = "Регистрация успешна! Проверьте вашу почту для подтверждения аккаунта.";
                    }
                    catch (Exception ex)
                    {
                        TempData["SuccessMessage"] = "Регистрация успешна! Пожалуйста, свяжитесь с поддержкой для подтверждения аккаунта.";
                        Console.WriteLine($"Email error: {ex.Message}");
                    }
                    return RedirectToAction("Auth", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: Account/ConfirmEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Email confirmed successfully! You can now log in.";
                return RedirectToAction("Auth", "Account");
            }

            TempData["ErrorMessage"] = "Error confirming email. Please try again.";
            return RedirectToAction("Auth", "Account");
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}