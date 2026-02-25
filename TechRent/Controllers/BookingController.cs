using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;

namespace TechRent.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBookedDates(int equipmentId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.EquipmentId == equipmentId && b.Status != "Отменено")
                .ToListAsync();

            var bookedDates = new List<string>();

            foreach (var booking in bookings)
            {
                var currentDate = booking.StartDate;
                while (currentDate <= booking.EndDate)
                {
                    bookedDates.Add(currentDate.ToString("yyyy-MM-dd"));
                    currentDate = currentDate.AddDays(1);
                }
            }

            return Json(bookedDates);
        }
    }
}