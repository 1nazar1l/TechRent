using Microsoft.AspNetCore.Identity;
using TechRent.Models.Entities;

namespace TechRent.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Создаем роли, если их нет
            string[] roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Создаем тестового администратора
            string adminEmail = "admin@techrent.com";
            string adminPassword = "Admin123!";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // 3. Создаем тестового обычного пользователя
            string userEmail = "user@example.com";
            string userPassword = "User123!";

            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var user = new IdentityUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, userPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }

            // 4. Добавляем тестовое оборудование, если таблица пуста
            if (!context.Equipments.Any())
            {
                var equipments = new List<Equipment>
                {
                    new Equipment
                    {
                        Name = "Бетономешалка 150л",
                        Type = "Бетономешалки",
                        Description = "Профессиональная бетономешалка объемом 150 литров. Идеально подходит для строительных работ.",
                        PricePerDay = 1500m,
                        Deposit = 5000m,
                        ImageUrl = "/images/betonomeshalka.jpg",
                        AvailableQuantity = 3
                    },
                    new Equipment
                    {
                        Name = "Перфоратор Bosch",
                        Type = "Электроинструмент",
                        Description = "Мощный перфоратор с функцией отбойника. В комплекте набор буров.",
                        PricePerDay = 800m,
                        Deposit = 3000m,
                        ImageUrl = "/images/perforator.jpg",
                        AvailableQuantity = 5
                    },
                    new Equipment
                    {
                        Name = "Строительные леса",
                        Type = "Леса и опалубка",
                        Description = "Комплект строительных лесов высотой 5м. Включает все необходимые элементы.",
                        PricePerDay = 2500m,
                        Deposit = 10000m,
                        ImageUrl = "/images/lesa.jpg",
                        AvailableQuantity = 2
                    },
                    new Equipment
                    {
                        Name = "Шуруповерт Makita",
                        Type = "Электроинструмент",
                        Description = "Аккумуляторный шуруповерт с двумя аккумуляторами в комплекте.",
                        PricePerDay = 600m,
                        Deposit = 2500m,
                        ImageUrl = "/images/shurupovert.jpg",
                        AvailableQuantity = 4
                    },
                    new Equipment
                    {
                        Name = "Компрессор воздушный",
                        Type = "Компрессоры",
                        Description = "Воздушный компрессор для покраски и пневмоинструмента.",
                        PricePerDay = 1200m,
                        Deposit = 4000m,
                        ImageUrl = "/images/kompressor.jpg",
                        AvailableQuantity = 2
                    }
                };

                context.Equipments.AddRange(equipments);
                await context.SaveChangesAsync();

                // 5. Добавляем тестовые отзывы (привязываем к первому пользователю)
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user != null)
                {
                    var reviews = new List<Review>
                    {
                        new Review
                        {
                            UserId = user.Id,
                            EquipmentId = equipments[0].Id, // Бетономешалка
                            Rating = 5,
                            Comment = "Отличная бетономешалка! Работала без нареканий весь день.",
                            CreatedAt = DateTime.Now.AddDays(-5)
                        },
                        new Review
                        {
                            UserId = user.Id,
                            EquipmentId = equipments[1].Id, // Перфоратор
                            Rating = 4,
                            Comment = "Хороший перфоратор, но немного тяжеловат.",
                            CreatedAt = DateTime.Now.AddDays(-3)
                        }
                    };

                    context.Reviews.AddRange(reviews);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}