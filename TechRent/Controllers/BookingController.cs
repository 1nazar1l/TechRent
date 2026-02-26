using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;

namespace TechRent.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Booking/Index - страница со всеми бронированиями пользователя
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var bookings = await _context.Bookings
                .Include(b => b.Equipment)
                    .ThenInclude(e => e.Category)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Booking/Details/5 - страница деталей конкретного бронирования
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var booking = await _context.Bookings
                .Include(b => b.Equipment)
                    .ThenInclude(e => e.Category)
                .Include(b => b.Delivery)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: api/Booking/GetBookedDates - используется на странице товара для календаря
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

        // POST: api/Booking/CreateBooking - используется на странице товара для оформления аренды
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var equipment = await _context.Equipments.FindAsync(request.EquipmentId);
                if (equipment == null)
                {
                    return NotFound(new { success = false, message = "Equipment not found" });
                }

                if (equipment.AvailableQuantity <= 0)
                {
                    return BadRequest(new { success = false, message = "Equipment is out of stock" });
                }

                // Check if dates are available
                var startDate = DateTime.Parse(request.StartDate);
                var endDate = DateTime.Parse(request.EndDate);

                var existingBookings = await _context.Bookings
                    .Where(b => b.EquipmentId == request.EquipmentId
                        && b.Status != "Отменено"
                        && ((b.StartDate <= endDate && b.EndDate >= startDate)))
                    .AnyAsync();

                if (existingBookings)
                {
                    return BadRequest(new { success = false, message = "Selected dates are not available" });
                }

                // Calculate days
                var days = (endDate - startDate).Days + 1;
                var totalPrice = equipment.PricePerDay * days;
                var serviceFee = 250;
                var totalDue = totalPrice + serviceFee + equipment.Deposit;

                // Create booking
                var booking = new Booking
                {
                    UserId = user.Id,
                    EquipmentId = request.EquipmentId,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPrice = totalPrice,
                    DepositPaid = equipment.Deposit,
                    Fine = 0,
                    Status = "Подтверждено"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Booking created successfully",
                    bookingId = booking.Id,
                    totalAmount = totalDue
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class CreateBookingRequest
        {
            public int EquipmentId { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
        }
    }
}