using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;
using Microsoft.AspNetCore.Identity; // Добавить

namespace TechRent.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // Добавить

        public EquipmentController(ApplicationDbContext context, UserManager<IdentityUser> userManager) // Изменить конструктор
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Equipment
        public async Task<IActionResult> Index(
            string searchString = "",
            List<int>? categoryIds = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? driveType = null,
            bool? inStockOnly = null,
            string sortOrder = "recommended",
            int page = 1,
            int pageSize = 6)
        {
            // Базовый запрос без Include для Count
            var equipmentQuery = _context.Equipments
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                equipmentQuery = equipmentQuery.Where(e =>
                    e.Name.ToLower().Contains(searchString) ||
                    e.Id.ToString().Contains(searchString) ||
                    (e.Description != null && e.Description.ToLower().Contains(searchString)));
            }

            // Category filter
            if (categoryIds != null && categoryIds.Any())
            {
                equipmentQuery = equipmentQuery.Where(e => categoryIds.Contains(e.CategoryId));
            }

            // Price range filter
            if (minPrice.HasValue)
            {
                equipmentQuery = equipmentQuery.Where(e => e.PricePerDay >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                equipmentQuery = equipmentQuery.Where(e => e.PricePerDay <= maxPrice);
            }

            // In stock filter
            if (inStockOnly == true)
            {
                equipmentQuery = equipmentQuery.Where(e => e.AvailableQuantity > 0);
            }

            // Get total count for pagination (без Include)
            var totalItems = await equipmentQuery.CountAsync();

            // Получаем все оборудование с категориями и отзывами
            var equipmentWithDetails = await _context.Equipments
                .Include(e => e.Category)
                .Include(e => e.Reviews)
                .Include(e => e.Favorites) // Добавить Include для Favorites
                .Where(e => true) // Здесь нужно применить те же фильтры
                .ToListAsync();

            // Применяем фильтры в памяти
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                equipmentWithDetails = equipmentWithDetails.Where(e =>
                    e.Name.ToLower().Contains(searchString) ||
                    e.Id.ToString().Contains(searchString) ||
                    (e.Description != null && e.Description.ToLower().Contains(searchString))).ToList();
            }

            if (categoryIds != null && categoryIds.Any())
            {
                equipmentWithDetails = equipmentWithDetails.Where(e => categoryIds.Contains(e.CategoryId)).ToList();
            }

            if (minPrice.HasValue)
            {
                equipmentWithDetails = equipmentWithDetails.Where(e => e.PricePerDay >= minPrice).ToList();
            }
            if (maxPrice.HasValue)
            {
                equipmentWithDetails = equipmentWithDetails.Where(e => e.PricePerDay <= maxPrice).ToList();
            }

            if (inStockOnly == true)
            {
                equipmentWithDetails = equipmentWithDetails.Where(e => e.AvailableQuantity > 0).ToList();
            }

            // Вычисляем средний рейтинг для каждого оборудования
            foreach (var item in equipmentWithDetails)
            {
                item.AverageRating = item.Reviews != null && item.Reviews.Any()
                    ? item.Reviews.Average(r => r.Rating)
                    : 0;
            }

            // Check favorites for logged in user
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userFavorites = await _context.Favorites
                    .Where(f => f.UserId == user.Id)
                    .Select(f => f.EquipmentId)
                    .ToListAsync();

                foreach (var item in equipmentWithDetails)
                {
                    item.IsFavorite = userFavorites.Contains(item.Id);
                }
            }

            // Sorting
            switch (sortOrder)
            {
                case "price_asc":
                    equipmentWithDetails = equipmentWithDetails.OrderBy(e => e.PricePerDay).ToList();
                    break;
                case "price_desc":
                    equipmentWithDetails = equipmentWithDetails.OrderByDescending(e => e.PricePerDay).ToList();
                    break;
                case "newest":
                    equipmentWithDetails = equipmentWithDetails.OrderByDescending(e => e.Id).ToList();
                    break;
                case "rating":
                    equipmentWithDetails = equipmentWithDetails.OrderByDescending(e => e.AverageRating).ThenBy(e => e.PricePerDay).ToList();
                    break;
                default: // recommended
                    equipmentWithDetails = equipmentWithDetails.OrderByDescending(e => e.AverageRating).ThenBy(e => e.PricePerDay).ToList();
                    break;
            }

            // Get categories with counts for filter sidebar
            var categories = await _context.Categories
                .Select(c => new {
                    Category = c,
                    Count = _context.Equipments.Count(e => e.CategoryId == c.Id)
                })
                .ToListAsync();

            // Apply pagination
            var equipment = equipmentWithDetails
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Store filter values in ViewBag for the view
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategoryIds = categoryIds ?? new List<int>();
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.DriveType = driveType;
            ViewBag.InStockOnly = inStockOnly;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(equipmentWithDetails.Count / (double)pageSize);
            ViewBag.TotalItems = equipmentWithDetails.Count;
            ViewBag.Categories = categories;

            return View(equipment);
        }
    }
}