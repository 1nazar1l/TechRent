using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;

namespace TechRent.Controllers
{
    [Authorize(Roles = "Admin,Supplier")] // Или просто [Authorize], если роль Supplier не создана
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Здесь можно фильтровать по текущему пользователю-поставщику
            // Пока показываем все оборудование для демонстрации
            var equipment = await _context.Equipments
                .Include(e => e.Category)
                .Include(e => e.Bookings)
                .ToListAsync();

            var categories = await _context.Categories.ToListAsync();

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
                TotalEquipment = equipment.Count,
                ActiveRentals = equipment.Sum(e => e.Bookings?.Count(b => b.Status == "Подтверждено" && b.StartDate <= DateTime.Now && b.EndDate >= DateTime.Now) ?? 0),
                TotalEarnings = equipment.Sum(e => e.Bookings?.Where(b => b.Status == "Подтверждено" || b.Status == "Завершено").Sum(b => b.TotalPrice) ?? 0),
                AverageRating = 4.5, // Временное значение, потом можно вычислить из отзывов
                Equipment = supplierEquipment,
                Categories = categories
            };

            return View(viewModel);
        }

        // GET: Supplier/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Equipment equipment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(equipment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(equipment);
        }

        // GET: Supplier/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(equipment);
        }

        // POST: Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Equipment equipment)
        {
            if (id != equipment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(equipment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(equipment);
        }

        // GET: Supplier/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null) return NotFound();

            return View(equipment);
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment != null)
            {
                _context.Equipments.Remove(equipment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Supplier/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var equipment = await _context.Equipments
                .Include(e => e.Category)
                .Include(e => e.Reviews)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null) return NotFound();

            return View(equipment);
        }
    }
}