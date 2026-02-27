using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;

namespace TechRent.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                // Проверяем, существует ли оборудование
                var equipment = await _context.Equipments.FindAsync(request.EquipmentId);
                if (equipment == null)
                {
                    return NotFound(new { success = false, message = "Equipment not found" });
                }

                // Проверяем, не оставлял ли пользователь уже отзыв
                var existingReview = await _context.Reviews
                    .AnyAsync(r => r.UserId == user.Id && r.EquipmentId == request.EquipmentId);

                if (existingReview)
                {
                    return BadRequest(new { success = false, message = "You have already reviewed this equipment" });
                }

                // Проверяем, арендовал ли пользователь это оборудование
                var hasRented = await _context.Bookings
                    .AnyAsync(b => b.UserId == user.Id
                        && b.EquipmentId == request.EquipmentId
                        && b.Status == "Подтверждено");

                // Создаем отзыв
                var review = new Review
                {
                    UserId = user.Id,
                    EquipmentId = request.EquipmentId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Review submitted successfully",
                    reviewId = review.Id,
                    hasRented = hasRented // Отправляем информацию, арендовал ли пользователь
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class CreateReviewRequest
        {
            public int EquipmentId { get; set; }
            public int Rating { get; set; }
            public string? Comment { get; set; }
        }
    }
}