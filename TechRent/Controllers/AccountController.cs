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
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Аккаунт ещё не подтверждён. Проверьте почту и перейдите по ссылке из письма.");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Аккаунт временно заблокирован из-за большого числа попыток входа. Попробуйте позже.");
                }
                else if (result.RequiresTwoFactor)
                {
                    ModelState.AddModelError(string.Empty, "Для входа требуется двухфакторная аутентификация.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неверный email или пароль. Проверьте введённые данные.");
                }
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
                if (!model.TermsAccepted)
                {
                    ModelState.AddModelError(string.Empty, "Необходимо согласиться с условиями использования и политикой конфиденциальности.");
                    return View(model);
                }

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Пользователь с таким email уже зарегистрирован. Войдите в аккаунт или восстановите пароль.");
                    return View(model);
                }

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
                            "Добро пожаловать в RentSport! Подтвердите email",
                            $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Добро пожаловать в RentSport, {model.FirstName}!</h2>
                        <p>Спасибо за регистрацию. Пожалуйста, подтвердите ваш email адрес, нажав на ссылку ниже:</p>
                        <p><a href='{confirmationLink}' style='background-color: #ff7a1a; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Подтвердить email</a></p>
                        <p>Если кнопка не работает, скопируйте и вставьте эту ссылку в браузер:</p>
                        <p>{confirmationLink}</p>
                        <p>С уважением,<br>Команда RentSport</p>
                    </body>
                    </html>"
                        );
                        TempData["SuccessMessage"] = "Регистрация успешна! Проверьте вашу почту и подтвердите аккаунт по ссылке из письма.";
                    }
                    catch (Exception ex)
                    {
                        TempData["SuccessMessage"] = "Регистрация успешна! Не удалось отправить письмо с подтверждением — обратитесь в поддержку support@rentsport.by.";
                        Console.WriteLine($"Email error: {ex.Message}");
                    }
                    return RedirectToAction("Auth", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, GetIdentityErrorMessage(error));
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
                TempData["ErrorMessage"] = "Ссылка подтверждения недействительна. Зарегистрируйтесь заново или обратитесь в поддержку.";
                return RedirectToAction("Auth", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден. Возможно, аккаунт был удалён.";
                return RedirectToAction("Auth", "Account");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Email успешно подтверждён! Теперь вы можете войти в аккаунт.";
                return RedirectToAction("Auth", "Account");
            }

            TempData["ErrorMessage"] = "Не удалось подтвердить email. Возможно, срок действия ссылки истёк — запросите новое письмо.";
            return RedirectToAction("Auth", "Account");
        }

        private static string GetIdentityErrorMessage(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateEmail" or "DuplicateUserName" => "Пользователь с таким email уже зарегистрирован.",
                "InvalidEmail" or "InvalidUserName" => "Введите корректный адрес электронной почты.",
                "PasswordTooShort" => "Пароль слишком короткий — минимум 6 символов.",
                "PasswordRequiresNonAlphanumeric" => "Пароль должен содержать хотя бы один специальный символ.",
                "PasswordRequiresDigit" => "Пароль должен содержать хотя бы одну цифру.",
                "PasswordRequiresLower" => "Пароль должен содержать хотя бы одну строчную букву.",
                "PasswordRequiresUpper" => "Пароль должен содержать хотя бы одну заглавную букву.",
                "PasswordRequiresUniqueChars" => "Пароль должен содержать больше уникальных символов.",
                _ => error.Description
            };
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