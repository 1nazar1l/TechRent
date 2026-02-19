using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            // Получаем все оборудование для отображения на главной
            var equipmentList = _context.Equipments.ToList();
            return View(equipmentList);
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