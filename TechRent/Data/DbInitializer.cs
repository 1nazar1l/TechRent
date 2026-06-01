using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            // ===== 1. ОЧИСТКА ВСЕХ ДАННЫХ =====
            Console.WriteLine("Очистка базы данных...");

            // Удаляем все отзывы
            context.Reviews.RemoveRange(context.Reviews);

            // Удаляем все бронирования
            context.Bookings.RemoveRange(context.Bookings);

            // Удаляем все избранное
            context.Favorites.RemoveRange(context.Favorites);

            // Удаляем все оборудование
            context.Equipments.RemoveRange(context.Equipments);

            // Удаляем все категории
            context.Categories.RemoveRange(context.Categories);

            // Сохраняем изменения
            await context.SaveChangesAsync();
            Console.WriteLine("Очистка завершена.");

            // ===== 2. СОЗДАНИЕ РОЛЕЙ =====
            string[] roles = new[] { "Admin", "User", "Supplier" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"Роль '{role}' создана.");
                }
            }

            // ===== 3. СОЗДАНИЕ ТЕСТОВЫХ ПОЛЬЗОВАТЕЛЕЙ =====

            // Администратор
            string adminEmail = "admin@rentoffice.com";
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
                    Console.WriteLine($"Администратор '{adminEmail}' создан.");
                }
            }

            // Обычный пользователь
            string userEmail = "user@rentoffice.com";
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
                    Console.WriteLine($"Пользователь '{userEmail}' создан.");
                }
            }

            // Поставщик
            string supplierEmail = "supplier@rentoffice.com";
            string supplierPassword = "Supplier123!";

            if (await userManager.FindByEmailAsync(supplierEmail) == null)
            {
                var supplier = new IdentityUser
                {
                    UserName = supplierEmail,
                    Email = supplierEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(supplier, supplierPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(supplier, "Supplier");
                    Console.WriteLine($"Поставщик '{supplierEmail}' создан.");
                }
            }

            // ===== 4. ДОБАВЛЕНИЕ КАТЕГОРИЙ (ОФИСНОЕ ОБОРУДОВАНИЕ) =====
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Принтеры и МФУ",
                    Description = "Лазерные и струйные принтеры, многофункциональные устройства для печати, сканирования и копирования",
                    DisplayOrder = 1
                },
                new Category
                {
                    Name = "Компьютеры",
                    Description = "Ноутбуки, моноблоки, системные блоки для любых задач",
                    DisplayOrder = 2
                },
                new Category
                {
                    Name = "Видеоконференции",
                    Description = "Камеры, микрофоны, спикерфоны для проведения онлайн-встреч",
                    DisplayOrder = 3
                },
                new Category
                {
                    Name = "Офисная мебель",
                    Description = "Эргономичные кресла, столы, переговорные зоны",
                    DisplayOrder = 4
                },
                new Category
                {
                    Name = "Кухонное оборудование",
                    Description = "Кофемашины, чайники, микроволновые печи для офиса",
                    DisplayOrder = 5
                },
                new Category
                {
                    Name = "Проекторы и экраны",
                    Description = "Проекторы для презентаций, интерактивные доски, экраны",
                    DisplayOrder = 6
                },
                new Category
                {
                    Name = "Серверное оборудование",
                    Description = "Серверы, сетевое оборудование, системы хранения данных",
                    DisplayOrder = 7
                },
                new Category
                {
                    Name = "Расходные материалы",
                    Description = "Картриджи, бумага, канцелярские принадлежности",
                    DisplayOrder = 8
                }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
            Console.WriteLine($"Добавлено {categories.Count} категорий.");

            // ===== 5. ПОЛУЧАЕМ ID КАТЕГОРИЙ =====
            var printersCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Принтеры и МФУ");
            var computersCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Компьютеры");
            var videoCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Видеоконференции");
            var furnitureCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Офисная мебель");
            var kitchenCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Кухонное оборудование");
            var projectorsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Проекторы и экраны");

            // ===== 6. ДОБАВЛЕНИЕ ОБОРУДОВАНИЯ (ОФИСНОЕ) =====
            var equipments = new List<Equipment>
            {
                // Принтеры и МФУ
                new Equipment
                {
                    Name = "МФУ Canon i-SENSYS MF445dw",
                    CategoryId = printersCategory?.Id ?? 1,
                    Description = "Лазерное МФУ формата A4 с печатью, сканированием и копированием. Поддержка Wi-Fi и двусторонней печати.",
                    PricePerDay = 45,
                    Deposit = 200,
                    ImageUrl = "https://images.unsplash.com/photo-1581091226033-d5c48150dbaa?w=400&auto=format",
                    AvailableQuantity = 8
                },
                new Equipment
                {
                    Name = "Принтер HP LaserJet Pro",
                    CategoryId = printersCategory?.Id ?? 1,
                    Description = "Монохромный лазерный принтер с быстрой печатью до 40 страниц в минуту.",
                    PricePerDay = 35,
                    Deposit = 150,
                    ImageUrl = "https://images.unsplash.com/photo-1563206767-5b18f218e8d0?w=400&auto=format",
                    AvailableQuantity = 12
                },
        
                // Компьютеры
                new Equipment
                {
                    Name = "Ноутбук Apple MacBook Pro 14",
                    CategoryId = computersCategory?.Id ?? 2,
                    Description = "Ноутбук с процессором M3, 16GB RAM, 512GB SSD. Идеален для работы и дизайна.",
                    PricePerDay = 120,
                    Deposit = 1500,
                    ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400&auto=format",
                    AvailableQuantity = 5
                },
                new Equipment
                {
                    Name = "Моноблок iMac 24",
                    CategoryId = computersCategory?.Id ?? 2,
                    Description = "24-дюймовый моноблок с дисплеем Retina, процессор M3.",
                    PricePerDay = 150,
                    Deposit = 1800,
                    ImageUrl = "https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?w=400&auto=format",
                    AvailableQuantity = 3
                },
                new Equipment
                {
                    Name = "Ноутбук Dell XPS 15",
                    CategoryId = computersCategory?.Id ?? 2,
                    Description = "Мощный ноутбук для бизнеса, 32GB RAM, 1TB SSD, Intel Core i7.",
                    PricePerDay = 100,
                    Deposit = 1200,
                    ImageUrl = "https://images.unsplash.com/photo-1593642702749-b7d2a804fbcf?w=400&auto=format",
                    AvailableQuantity = 6
                },
        
                // Видеоконференции
                new Equipment
                {
                    Name = "Камера Logitech Brio 4K",
                    CategoryId = videoCategory?.Id ?? 3,
                    Description = "Веб-камера 4K для конференций, с шумоподавлением и автофокусом.",
                    PricePerDay = 25,
                    Deposit = 100,
                    ImageUrl = "https://images.unsplash.com/photo-1587825140708-dfaf72ae4f04?w=400&auto=format",
                    AvailableQuantity = 15
                },
                new Equipment
                {
                    Name = "Комплект для конференций Logitech Rally",
                    CategoryId = videoCategory?.Id ?? 3,
                    Description = "Полный комплект для переговорных комнат: камера, микрофон, спикерфон.",
                    PricePerDay = 180,
                    Deposit = 1000,
                    ImageUrl = "https://images.unsplash.com/photo-1573164713988-8665fc963095?w=400&auto=format",
                    AvailableQuantity = 2
                },
        
                // Офисная мебель
                new Equipment
                {
                    Name = "Кресло офисное эргономичное",
                    CategoryId = furnitureCategory?.Id ?? 4,
                    Description = "Эргономичное кресло с поддержкой поясницы и регулировкой высоты.",
                    PricePerDay = 30,
                    Deposit = 200,
                    ImageUrl = "https://images.unsplash.com/photo-1580480055273-228ff5388ef8?w=400&auto=format",
                    AvailableQuantity = 10
                },
                new Equipment
                {
                    Name = "Стол для переговоров",
                    CategoryId = furnitureCategory?.Id ?? 4,
                    Description = "Стол для переговорной комнаты на 8 персон.",
                    PricePerDay = 80,
                    Deposit = 500,
                    ImageUrl = "https://images.unsplash.com/photo-1542471028-78b7f0cf65c1?w=400&auto=format",
                    AvailableQuantity = 4
                },
        
                // Кухонное оборудование
                new Equipment
                {
                    Name = "Кофемашина Jura E8",
                    CategoryId = kitchenCategory?.Id ?? 5,
                    Description = "Автоматическая кофемашина для офиса. Приготовление любых кофейных напитков.",
                    PricePerDay = 50,
                    Deposit = 400,
                    ImageUrl = "https://images.unsplash.com/photo-1578601127250-46f4e83c65e9?w=400&auto=format",
                    AvailableQuantity = 6
                },
                new Equipment
                {
                    Name = "Микроволновая печь Samsung",
                    CategoryId = kitchenCategory?.Id ?? 5,
                    Description = "Микроволновая печь с грилем, объем 23 литра.",
                    PricePerDay = 15,
                    Deposit = 80,
                    ImageUrl = "https://images.unsplash.com/photo-1575023782549-62ca0d244b39?w=400&auto=format",
                    AvailableQuantity = 8
                },
        
                // Проекторы
                new Equipment
                {
                    Name = "Проектор Epson EB-695Wi",
                    CategoryId = projectorsCategory?.Id ?? 6,
                    Description = "Интерактивный проектор для презентаций и обучения.",
                    PricePerDay = 90,
                    Deposit = 600,
                    ImageUrl = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=400&auto=format",
                    AvailableQuantity = 3
                }
            };

            context.Equipments.AddRange(equipments);
            await context.SaveChangesAsync();
            Console.WriteLine($"Добавлено {equipments.Count} единиц оборудования.");

            // ===== 7. ДОБАВЛЕНИЕ ОТЗЫВОВ =====
            var existingUser = await userManager.FindByEmailAsync(userEmail);
            if (existingUser != null)
            {
                var reviews = new List<Review>
                {
                    new Review
                    {
                        UserId = existingUser.Id,
                        EquipmentId = equipments[0].Id, // МФУ Canon
                        Rating = 5,
                        Comment = "Отличное устройство! Быстрая печать, удобное сканирование. Доставили вовремя, установили и настроили.",
                        CreatedAt = DateTime.Now.AddDays(-7)
                    },
                    new Review
                    {
                        UserId = existingUser.Id,
                        EquipmentId = equipments[2].Id, // MacBook Pro
                        Rating = 5,
                        Comment = "Отличный ноутбук для работы. Все приложения летают. Рекомендую!",
                        CreatedAt = DateTime.Now.AddDays(-5)
                    },
                    new Review
                    {
                        UserId = existingUser.Id,
                        EquipmentId = equipments[9].Id, // Кофемашина (индекс 9)
                        Rating = 4,
                        Comment = "Отличная кофемашина, офис теперь счастливее. Немного дороговата аренда, но качество оправдывает.",
                        CreatedAt = DateTime.Now.AddDays(-3)
                    },
                    new Review
                    {
                        UserId = existingUser.Id,
                        EquipmentId = equipments[5].Id, // Камера Logitech
                        Rating = 5,
                        Comment = "Качество видео отличное, звук чистый. Провели конференцию без проблем.",
                        CreatedAt = DateTime.Now.AddDays(-1)
                    }
                };

                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
                Console.WriteLine($"Добавлено {reviews.Count} отзывов.");
            }

            Console.WriteLine("База данных успешно инициализирована для RentOffice!");
        }
    }
}