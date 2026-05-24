using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Добавить этот using
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;

namespace TechRent.Controllers
{
    [Authorize(Roles = "Admin,Supplier")]
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SupplierController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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

            // Получаем оборудование только этого поставщика
            var equipment = await _context.Equipments
                .Include(e => e.Category)
                .Include(e => e.Bookings)
                .Include(e => e.Reviews)
                .Where(e => e.SupplierId == user.Id)
                .ToListAsync();

            // Получаем офис этого поставщика
            var office = await _context.Offices
                .FirstOrDefaultAsync(o => o.SupplierId == user.Id);

            // Получаем категории
            var categories = await _context.Categories.ToListAsync();

            // Вычисляем статистику
            var totalEquipment = equipment.Count;

            var activeRentals = equipment
                .SelectMany(e => e.Bookings ?? new List<Booking>())
                .Count(b => b.Status == "Подтверждено"
                    && b.StartDate <= DateTime.Now
                    && b.EndDate >= DateTime.Now);

            var totalEarnings = equipment
                .SelectMany(e => e.Bookings ?? new List<Booking>())
                .Where(b => b.Status == "Подтверждено" || b.Status == "Завершено")
                .Sum(b => (decimal?)b.TotalPrice) ?? 0;

            var allReviews = equipment
                .Where(e => e.Reviews != null && e.Reviews.Any())
                .SelectMany(e => e.Reviews)
                .ToList();

            var averageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0;

            // Формируем список оборудования для отображения
            var supplierEquipment = equipment.Select(e => new SupplierEquipmentItem
            {
                Id = e.Id,
                Name = e.Name,
                ImageUrl = e.ImageUrl,
                Category = e.Category,
                PricePerDay = e.PricePerDay,
                Deposit = e.Deposit,
                AvailableQuantity = e.AvailableQuantity,
                TotalRentals = e.Bookings?.Count(b => b.Status == "Подтверждено" || b.Status == "Завершено") ?? 0
            }).ToList();

            var viewModel = new SupplierViewModel
            {
                TotalEquipment = totalEquipment,
                ActiveRentals = activeRentals,
                TotalEarnings = totalEarnings,
                AverageRating = averageRating,
                Equipment = supplierEquipment,
                Categories = categories,
                Office = office
            };

            return View(viewModel);
        }

        // GET: Supplier/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Equipment equipment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                equipment.SupplierId = user.Id;
                _context.Add(equipment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Оборудование успешно добавлено";
                return RedirectToAction(nameof(Index));
            }
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", equipment.CategoryId);
            return View(equipment);
        }

        // GET: Supplier/CreateOffice
        [HttpGet]
        public IActionResult CreateOffice()
        {
            return View();
        }

        // POST: Supplier/CreateOffice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOffice(Office office)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                // Убеждаемся, что координаты не null
                if (office.Latitude == 0 && office.Longitude == 0)
                {
                    // Если координаты не заданы, ставим координаты Минска по умолчанию
                    office.Latitude = 53.9045;
                    office.Longitude = 27.5615;
                }

                office.SupplierId = user.Id;
                _context.Offices.Add(office);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Офис успешно добавлен";
                return RedirectToAction(nameof(Index));
            }
            return View(office);
        }

        // GET: Supplier/EditOffice
        [HttpGet]
        public async Task<IActionResult> EditOffice()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var office = await _context.Offices.FirstOrDefaultAsync(o => o.SupplierId == user.Id);
            if (office == null)
            {
                return RedirectToAction(nameof(CreateOffice));
            }
            return View(office);
        }

        // POST: Supplier/EditOffice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOffice(Office office)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Проверяем, что офис принадлежит этому поставщику
            var existingOffice = await _context.Offices
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == office.Id && o.SupplierId == user.Id);

            if (existingOffice == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                office.SupplierId = user.Id;
                _context.Update(office);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Офис успешно обновлен";
                return RedirectToAction(nameof(Index));
            }
            return View(office);
        }

        // GET: Supplier/DeleteOffice
        [HttpGet]
        public async Task<IActionResult> DeleteOffice()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var office = await _context.Offices.FirstOrDefaultAsync(o => o.SupplierId == user.Id);
            if (office == null)
            {
                return NotFound();
            }

            return View(office);
        }

        // POST: Supplier/DeleteOffice
        [HttpPost, ActionName("DeleteOffice")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOfficeConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var office = await _context.Offices
                .FirstOrDefaultAsync(o => o.Id == id && o.SupplierId == user.Id);

            if (office != null)
            {
                _context.Offices.Remove(office);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Офис успешно удален";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Supplier/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var equipment = await _context.Equipments
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null || equipment.SupplierId != user.Id) return NotFound();

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", equipment.CategoryId);
            return View(equipment);
        }

        // POST: Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Equipment equipment)
        {
            if (id != equipment.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var existingEquipment = await _context.Equipments
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingEquipment == null || existingEquipment.SupplierId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                equipment.SupplierId = user.Id;
                _context.Update(equipment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Оборудование успешно обновлено";
                return RedirectToAction(nameof(Index));
            }
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", equipment.CategoryId);
            return View(equipment);
        }

        // GET: Supplier/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var equipment = await _context.Equipments
                .Include(e => e.Category)
                .Include(e => e.Reviews)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null || equipment.SupplierId != user.Id) return NotFound();

            return View(equipment);
        }

        // GET: Supplier/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var equipment = await _context.Equipments
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null || equipment.SupplierId != user.Id) return NotFound();

            return View(equipment);
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var equipment = await _context.Equipments
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment != null && equipment.SupplierId == user.Id)
            {
                var hasActiveBookings = equipment.Bookings != null && equipment.Bookings.Any(b => b.Status == "Подтверждено" && b.EndDate >= DateTime.Now);

                if (hasActiveBookings)
                {
                    TempData["ErrorMessage"] = "Нельзя удалить оборудование с активными бронированиями";
                    return RedirectToAction(nameof(Index));
                }

                _context.Equipments.Remove(equipment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Оборудование успешно удалено";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}