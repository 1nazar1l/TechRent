using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using TechRent.Models.Entities;

namespace TechRent.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Equipment
        public async Task<IActionResult> Index(
            string searchString = "",
            int? categoryId = null,
            bool? availableOnly = null,
            bool? lowStockOnly = null,
            bool? outOfStockOnly = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string rating = "",
            int page = 1,
            int pageSize = 10)
        {
            var equipmentQuery = _context.Equipments
                .Include(e => e.Category)
                .Include(e => e.Reviews)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                equipmentQuery = equipmentQuery.Where(e =>
                    e.Name.ToLower().Contains(searchString) ||
                    e.Id.ToString().Contains(searchString) ||
                    (e.Category != null && e.Category.Name.ToLower().Contains(searchString)));
            }

            // Category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                equipmentQuery = equipmentQuery.Where(e => e.CategoryId == categoryId);
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

            // Stock status filters
            if (availableOnly == true)
            {
                equipmentQuery = equipmentQuery.Where(e => e.AvailableQuantity > 0);
            }
            if (lowStockOnly == true)
            {
                equipmentQuery = equipmentQuery.Where(e => e.AvailableQuantity > 0 && e.AvailableQuantity <= 5);
            }
            if (outOfStockOnly == true)
            {
                equipmentQuery = equipmentQuery.Where(e => e.AvailableQuantity == 0);
            }

            // Rating filter
            if (!string.IsNullOrEmpty(rating))
            {
                switch (rating)
                {
                    case "5stars":
                        equipmentQuery = equipmentQuery.Where(e =>
                            e.Reviews != null && e.Reviews.Any() &&
                            e.Reviews.Average(r => r.Rating) >= 4.5);
                        break;
                    case "4+stars":
                        equipmentQuery = equipmentQuery.Where(e =>
                            e.Reviews != null && e.Reviews.Any() &&
                            e.Reviews.Average(r => r.Rating) >= 4);
                        break;
                    case "3+stars":
                        equipmentQuery = equipmentQuery.Where(e =>
                            e.Reviews != null && e.Reviews.Any() &&
                            e.Reviews.Average(r => r.Rating) >= 3);
                        break;
                    case "2+stars":
                        equipmentQuery = equipmentQuery.Where(e =>
                            e.Reviews != null && e.Reviews.Any() &&
                            e.Reviews.Average(r => r.Rating) >= 2);
                        break;
                }
            }

            // Get total count for pagination
            var totalItems = await equipmentQuery.CountAsync();

            // Apply pagination
            var equipment = await equipmentQuery
                .OrderBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Store filter values in ViewBag for the view
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.AvailableOnly = availableOnly;
            ViewBag.LowStockOnly = lowStockOnly;
            ViewBag.OutOfStockOnly = outOfStockOnly;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SelectedRating = rating;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(equipment);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipments
                .Include(e => e.Category) // Добавить для загрузки категории
                .FirstOrDefaultAsync(m => m.Id == id);

            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // POST: Admin/Delete/5
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

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipments
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", equipment.CategoryId);
            return View(equipment);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Equipment equipment)
        {
            if (id != equipment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(equipment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipmentExists(equipment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", equipment.CategoryId);
            return View(equipment);
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Create
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

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", equipment.CategoryId);
            return View(equipment);
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipments.Any(e => e.Id == id);
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Equipments)
                .ToListAsync();
            return View(categories);
        }

        // GET: Admin/EditCategory/5
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Categories));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(category);
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        // GET: Admin/CreateCategory
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // GET: Admin/DeleteCategory/5
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Equipments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/DeleteCategory/5
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Equipments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                if (category.Equipments != null && category.Equipments.Any())
                {
                    TempData["Error"] = "Cannot delete category that has equipment assigned to it.";
                    return RedirectToAction(nameof(Categories));
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Categories));
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string phoneNumber, string role,
            string emailConfirmed, string phoneNumberConfirmed, string twoFactorEnabled)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update basic info - проверяем значение "true" от чекбокса
            user.PhoneNumber = phoneNumber;
            user.EmailConfirmed = emailConfirmed == "true";
            user.PhoneNumberConfirmed = phoneNumberConfirmed == "true";
            user.TwoFactorEnabled = twoFactorEnabled == "true";

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(user);
            }

            // Update role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/DeleteUser/5
        public async Task<IActionResult> DeleteUser(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user is trying to delete themselves
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Error deleting user.";
            }

            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email, string username, string password,
            string confirmPassword, string phoneNumber, string role, bool emailConfirmed, bool phoneNumberConfirmed)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View();
            }

            var user = new IdentityUser
            {
                UserName = username,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = emailConfirmed,
                PhoneNumberConfirmed = phoneNumberConfirmed
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        // GET: Admin/Reviews
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Equipment)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reviews);
        }

        // GET: Admin/ReviewDetails/5
        public async Task<IActionResult> ReviewDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: Admin/DeleteReview/5
        public async Task<IActionResult> DeleteReview(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Admin/DeleteReview/5
        [HttpPost, ActionName("DeleteReview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReviewConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Reviews));
        }

        // GET: Admin/EditReview/5
        public async Task<IActionResult> EditReview(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Admin/EditReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(int id, Review review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Сохраняем оригинальные значения CreatedAt, UserId, EquipmentId
                    var existingReview = await _context.Reviews
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (existingReview != null)
                    {
                        review.CreatedAt = existingReview.CreatedAt;
                        review.UserId = existingReview.UserId;
                        review.EquipmentId = existingReview.EquipmentId;
                    }

                    _context.Update(review);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Reviews));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(review);
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }

        // GET: Admin/CreateReview
        public async Task<IActionResult> CreateReview()
        {
            ViewBag.Users = await _context.Users.ToListAsync();
            ViewBag.Equipments = await _context.Equipments.ToListAsync();
            return View();
        }

        // POST: Admin/CreateReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(Review review)
        {
            if (ModelState.IsValid)
            {
                review.CreatedAt = DateTime.Now;
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Reviews));
            }

            ViewBag.Users = await _context.Users.ToListAsync();
            ViewBag.Equipments = await _context.Equipments.ToListAsync();
            return View(review);
        }
    }
}