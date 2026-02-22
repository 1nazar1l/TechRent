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
        public async Task<IActionResult> Categories(
            string searchString = "",
            int? minDisplayOrder = null,
            int? maxDisplayOrder = null,
            bool? hasEquipment = null,
            bool? emptyCategories = null,
            int page = 1,
            int pageSize = 10)
        {
            var categoriesQuery = _context.Categories
                .Include(c => c.Equipments)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                categoriesQuery = categoriesQuery.Where(c =>
                    c.Name.ToLower().Contains(searchString) ||
                    c.Id.ToString().Contains(searchString) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchString)));
            }

            // Display order range filter
            if (minDisplayOrder.HasValue)
            {
                categoriesQuery = categoriesQuery.Where(c => c.DisplayOrder >= minDisplayOrder);
            }
            if (maxDisplayOrder.HasValue)
            {
                categoriesQuery = categoriesQuery.Where(c => c.DisplayOrder <= maxDisplayOrder);
            }

            // Equipment count filters
            if (hasEquipment == true)
            {
                categoriesQuery = categoriesQuery.Where(c => c.Equipments != null && c.Equipments.Any());
            }
            if (emptyCategories == true)
            {
                categoriesQuery = categoriesQuery.Where(c => c.Equipments == null || !c.Equipments.Any());
            }

            // Get total count for pagination
            var totalItems = await categoriesQuery.CountAsync();

            // Apply pagination
            var categories = await categoriesQuery
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Store filter values in ViewBag for the view
            ViewBag.SearchString = searchString;
            ViewBag.MinDisplayOrder = minDisplayOrder;
            ViewBag.MaxDisplayOrder = maxDisplayOrder;
            ViewBag.HasEquipment = hasEquipment;
            ViewBag.EmptyCategories = emptyCategories;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

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
        public async Task<IActionResult> Users(
            string searchString = "",
            string role = "",
            bool? emailConfirmed = null,
            bool? hasPhone = null,
            bool? noPhone = null,
            int page = 1,
            int pageSize = 10)
        {
            var usersQuery = _context.Users.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.Email.ToLower().Contains(searchString) ||
                    u.UserName.ToLower().Contains(searchString) ||
                    u.Id.ToString().Contains(searchString) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchString)));
            }

            // Email confirmed filter
            if (emailConfirmed.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.EmailConfirmed == emailConfirmed);
            }

            // Phone filters
            if (hasPhone == true)
            {
                usersQuery = usersQuery.Where(u => !string.IsNullOrEmpty(u.PhoneNumber));
            }
            if (noPhone == true)
            {
                usersQuery = usersQuery.Where(u => string.IsNullOrEmpty(u.PhoneNumber));
            }

            // Get all users for role filtering (need to check roles in memory)
            var users = await usersQuery.ToListAsync();

            // Role filter (in memory because roles are not directly in IdentityUser)
            if (!string.IsNullOrEmpty(role))
            {
                var filteredUsers = new List<IdentityUser>();
                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (userRoles.Contains(role))
                    {
                        filteredUsers.Add(user);
                    }
                }
                users = filteredUsers;
            }

            // Get total count for pagination
            var totalItems = users.Count;

            // Apply pagination
            var pagedUsers = users
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Store filter values in ViewBag for the view
            ViewBag.SearchString = searchString;
            ViewBag.SelectedRole = role;
            ViewBag.EmailConfirmed = emailConfirmed;
            ViewBag.HasPhone = hasPhone;
            ViewBag.NoPhone = noPhone;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Roles = new[] { "Admin", "User" }; // Available roles

            return View(pagedUsers);
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
        // GET: Admin/Reviews
        public async Task<IActionResult> Reviews(
            string searchString = "",
            int? rating = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? hasComment = null,
            bool? noComment = null,
            int page = 1,
            int pageSize = 10)
        {
            var reviewsQuery = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Equipment)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                reviewsQuery = reviewsQuery.Where(r =>
                    r.Id.ToString().Contains(searchString) ||
                    (r.Comment != null && r.Comment.ToLower().Contains(searchString)) ||
                    (r.User != null && r.User.Email.ToLower().Contains(searchString)) ||
                    (r.Equipment != null && r.Equipment.Name.ToLower().Contains(searchString)));
            }

            // Rating filter
            if (rating.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.Rating == rating);
            }

            // Date range filter
            if (dateFrom.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.CreatedAt >= dateFrom);
            }
            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.AddDays(1);
                reviewsQuery = reviewsQuery.Where(r => r.CreatedAt < endDate);
            }

            // Comment filters
            if (hasComment == true)
            {
                reviewsQuery = reviewsQuery.Where(r => !string.IsNullOrEmpty(r.Comment));
            }
            if (noComment == true)
            {
                reviewsQuery = reviewsQuery.Where(r => string.IsNullOrEmpty(r.Comment));
            }

            // Get total count for pagination
            var totalItems = await reviewsQuery.CountAsync();

            // Apply pagination
            var reviews = await reviewsQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Store filter values in ViewBag for the view
            ViewBag.SearchString = searchString;
            ViewBag.SelectedRating = rating;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.HasComment = hasComment;
            ViewBag.NoComment = noComment;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

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