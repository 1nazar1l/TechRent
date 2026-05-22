using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;

namespace TechRent.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Получаем оборудование с отзывами и сортируем по популярности
            // Популярность = средний рейтинг + количество бронирований
            var equipmentWithStats = await _context.Equipments
                .Include(e => e.Reviews)
                .Include(e => e.Bookings)
                .Select(e => new
                {
                    Equipment = e,
                    AverageRating = e.Reviews.Any() ? e.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = e.Reviews.Count(),
                    BookingCount = e.Bookings.Count(b => b.Status == "Подтверждено" || b.Status == "Завершено")
                })
                .ToListAsync();

            // Сортируем по комбинированному рейтингу:
            // 70% от среднего рейтинга + 30% от количества бронирований
            var popularEquipment = equipmentWithStats
                .OrderByDescending(e => (e.AverageRating * 0.7) + (Math.Min(e.BookingCount, 100) / 100.0 * 5 * 0.3))
                .ThenByDescending(e => e.BookingCount)
                .Select(e => e.Equipment)
                .Take(8) // Берем топ-8 популярных товаров
                .ToList();

            return View(popularEquipment);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TestData()
        {
            // Тестовый метод для просмотра данных
            var equipment = _context.Equipments.ToList();
            var reviews = _context.Reviews.ToList();

            ViewBag.EquipmentCount = equipment.Count;
            ViewBag.ReviewsCount = reviews.Count;

            return Content($"База данных содержит: {ViewBag.EquipmentCount} единиц оборудования и {ViewBag.ReviewsCount} отзывов. " +
                           $"Тестовый админ: admin@techrent.com / Admin123! ; " +
                           $"Тестовый пользователь: user@example.com / User123!");
        }
    }
}