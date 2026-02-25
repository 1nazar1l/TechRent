using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace TechRent.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Product/Index/5
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipments
                .Include(e => e.Category)
                .Include(e => e.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null)
            {
                return NotFound();
            }

            // Calculate average rating
            if (equipment.Reviews != null && equipment.Reviews.Any())
            {
                equipment.AverageRating = equipment.Reviews.Average(r => r.Rating);
            }

            // Check if item is in user's favorites
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var isFavorite = await _context.Favorites
                    .AnyAsync(f => f.UserId == user.Id && f.EquipmentId == equipment.Id);
                equipment.IsFavorite = isFavorite;
            }

            // Get related equipment (same category)
            var relatedEquipment = await _context.Equipments
                .Where(e => e.CategoryId == equipment.CategoryId && e.Id != equipment.Id)
                .Include(e => e.Reviews)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedEquipment = relatedEquipment;

            return View(equipment);
        }
    }
}