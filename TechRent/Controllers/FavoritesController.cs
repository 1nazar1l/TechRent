using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;

namespace TechRent.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var existingFavorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.EquipmentId == request.EquipmentId);

                if (existingFavorite != null)
                {
                    // Remove from favorites
                    _context.Favorites.Remove(existingFavorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorite = false });
                }
                else
                {
                    // Add to favorites
                    var favorite = new Favorite
                    {
                        UserId = user.Id,
                        EquipmentId = request.EquipmentId,
                        AddedDate = DateTime.Now
                    };
                    _context.Favorites.Add(favorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorite = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class FavoriteRequest
        {
            public int EquipmentId { get; set; }
        }
    }
}