using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;

namespace TechRent.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var now = DateTime.Now;

            var activeBooking = await _context.Bookings
                .Include(b => b.Equipment)
                .Where(b => b.UserId == user.Id
                    && b.Status == "Подтверждено"
                    && b.StartDate <= now
                    && b.EndDate >= now)
                .OrderBy(b => b.EndDate)
                .FirstOrDefaultAsync();

            var upcomingBookings = await _context.Bookings
                .Include(b => b.Equipment)
                .Where(b => b.UserId == user.Id
                    && b.Status == "Подтверждено"
                    && b.StartDate > now)
                .OrderBy(b => b.StartDate)
                .ToListAsync();

            var historyBookings = await _context.Bookings
                .Include(b => b.Equipment)
                .Where(b => b.UserId == user.Id
                    && b.Status == "Подтверждено"
                    && b.EndDate < now)
                .OrderByDescending(b => b.EndDate)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? user.Email?.Split('@')[0] ?? "User",
                UserEmail = user.Email,
                IsPremium = false,
                ActiveBooking = activeBooking,
                UpcomingBookings = upcomingBookings,
                HistoryBookings = historyBookings
            };

            return View(viewModel);
        }

        // GET: Profile/Favorites
        public async Task<IActionResult> Favorites()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var favorites = await _context.Favorites
                .Include(f => f.Equipment)
                    .ThenInclude(e => e.Category)
                .Include(f => f.Equipment.Reviews)
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.AddedDate)
                .ToListAsync();

            // Для каждого оборудования вычисляем средний рейтинг
            foreach (var favorite in favorites)
            {
                if (favorite.Equipment.Reviews != null && favorite.Equipment.Reviews.Any())
                {
                    favorite.Equipment.AverageRating = favorite.Equipment.Reviews.Average(r => r.Rating);
                }
            }

            return View(favorites);
        }
    }
}