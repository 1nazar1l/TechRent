using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;

namespace TechRent.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EquipmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Equipment
        public async Task<IActionResult> Index()
        {
            var equipment = await _context.Equipments.ToListAsync();
            return View(equipment);
        }
    }
}