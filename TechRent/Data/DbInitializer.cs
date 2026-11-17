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

            // Применяем миграции, чтобы на чистой БД сначала создалась схема
            await context.Database.MigrateAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ===== 1. СОЗДАНИЕ РОЛЕЙ =====
            string[] roles = new[] { "Admin", "User", "Supplier" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"Роль '{role}' создана.");
                }
            }

            // ===== 2. СОЗДАНИЕ ТЕСТОВЫХ ПОЛЬЗОВАТЕЛЕЙ =====

            // Администратор (логин и пароль не меняем)
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

            var supplierUser = await userManager.FindByEmailAsync(supplierEmail);
            var supplierId = supplierUser?.Id;

            // ===== 3. ДОБАВЛЕНИЕ КАТЕГОРИЙ (если их нет) =====
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "Велосипеды",
                        Description = "Горные, городские и электровелосипеды для поездок и тренировок",
                        DisplayOrder = 1
                    },
                    new Category
                    {
                        Name = "Тренажёры и фитнес",
                        Description = "Беговые дорожки, гребные и эллиптические тренажёры для дома и зала",
                        DisplayOrder = 2
                    },
                    new Category
                    {
                        Name = "Туризм и кемпинг",
                        Description = "Палатки, спальники, рюкзаки и всё необходимое для похода",
                        DisplayOrder = 3
                    },
                    new Category
                    {
                        Name = "Зимний спорт",
                        Description = "Горные лыжи, сноуборды и защитная экипировка для сезона",
                        DisplayOrder = 4
                    },
                    new Category
                    {
                        Name = "Водный спорт",
                        Description = "Сапборды, каяки и аксессуары для отдыха на воде",
                        DisplayOrder = 5
                    },
                    new Category
                    {
                        Name = "Командные виды спорта",
                        Description = "Инвентарь для футбола, волейбола и активных игр на свежем воздухе",
                        DisplayOrder = 6
                    }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
                Console.WriteLine($"Добавлено {categories.Count} категорий.");
            }

            // ===== 4. ДОБАВЛЕНИЕ СПОРТИВНОГО ИНВЕНТАРЯ (если его нет) =====
            if (!context.Equipments.Any())
            {
                var bikesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Велосипеды");
                var fitnessCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Тренажёры и фитнес");
                var campingCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Туризм и кемпинг");
                var winterCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Зимний спорт");
                var waterCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Водный спорт");
                var teamCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Командные виды спорта");

                var equipments = new List<Equipment>
                {
                    // Велосипеды
                    new Equipment
                    {
                        Name = "Горный велосипед Trek Marlin 7",
                        CategoryId = bikesCategory?.Id ?? 1,
                        Description = "Горный велосипед с алюминиевой рамой, 21 скоростью и гидравлическими тормозами. Подходит для города и лёгкого бездорожья.",
                        PricePerDay = 35,
                        Deposit = 300,
                        AvailableQuantity = 6,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Городской велосипед City Cruiser 28",
                        CategoryId = bikesCategory?.Id ?? 1,
                        Description = "Комфортный городской велосипед с корзиной и багажником. Идеален для прогулок по городу.",
                        PricePerDay = 25,
                        Deposit = 200,
                        AvailableQuantity = 8,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Электровелосипед Himo C26",
                        CategoryId = bikesCategory?.Id ?? 1,
                        Description = "Электровелосипед с запасом хода до 100 км, съёмной батареей и семискоростной трансмиссией.",
                        PricePerDay = 55,
                        Deposit = 500,
                        AvailableQuantity = 4,
                        SupplierId = supplierId
                    },

                    // Тренажёры и фитнес
                    new Equipment
                    {
                        Name = "Беговая дорожка NordicTrack T 6.5 S",
                        CategoryId = fitnessCategory?.Id ?? 2,
                        Description = "Складная беговая дорожка с электроприводом, 12 программами тренировок и амортизацией бегового полотна.",
                        PricePerDay = 40,
                        Deposit = 600,
                        AvailableQuantity = 3,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Гребной тренажёр Concept2 RowErg",
                        CategoryId = fitnessCategory?.Id ?? 2,
                        Description = "Профессиональный гребной тренажёр с воздушным сопротивлением и точным измерением нагрузки.",
                        PricePerDay = 30,
                        Deposit = 400,
                        AvailableQuantity = 5,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Эллиптический тренажёр Sole E35",
                        CategoryId = fitnessCategory?.Id ?? 2,
                        Description = "Эллиптический тренажёр с плавным ходом, регулируемым сопротивлением и встроенными программами.",
                        PricePerDay = 45,
                        Deposit = 550,
                        AvailableQuantity = 2,
                        SupplierId = supplierId
                    },

                    // Туризм и кемпинг
                    new Equipment
                    {
                        Name = "Палатка Naturehike Cloud Up 4",
                        CategoryId = campingCategory?.Id ?? 3,
                        Description = "Четырёхместная лёгкая палатка с усиленным тентом и быстрой сборкой. Для походов и кемпинга.",
                        PricePerDay = 20,
                        Deposit = 150,
                        AvailableQuantity = 10,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Спальный мешок Deuter Orbit -5",
                        CategoryId = campingCategory?.Id ?? 3,
                        Description = "Тёплый спальный мешок для температур до −5 °C. Компактно складывается в чехол.",
                        PricePerDay = 12,
                        Deposit = 80,
                        AvailableQuantity = 15,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Туристический рюкзак Osprey 60 л",
                        CategoryId = campingCategory?.Id ?? 3,
                        Description = "Анатомический рюкзак на 60 литров с дождевым чехлом и системой вентиляции спины.",
                        PricePerDay = 10,
                        Deposit = 70,
                        AvailableQuantity = 12,
                        SupplierId = supplierId
                    },

                    // Зимний спорт
                    new Equipment
                    {
                        Name = "Горные лыжи Atomic Redster + ботинки",
                        CategoryId = winterCategory?.Id ?? 4,
                        Description = "Комплект горных лыж с креплениями и ботинками. Подготовлены и заточены под карвинг.",
                        PricePerDay = 50,
                        Deposit = 500,
                        AvailableQuantity = 6,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Сноуборд Burton Custom",
                        CategoryId = winterCategory?.Id ?? 4,
                        Description = "Универсальный сноуборд с креплениями для трасс любого уровня сложности.",
                        PricePerDay = 45,
                        Deposit = 450,
                        AvailableQuantity = 5,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Сапборд Aqua Marina 10'6\"",
                        CategoryId = waterCategory?.Id ?? 5,
                        Description = "Надувной SUP-борд с веслом, насосом и гермомешком. Устойчив на воде и легко перевозится.",
                        PricePerDay = 40,
                        Deposit = 350,
                        AvailableQuantity = 7,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Каяк двухместный Intex Explorer K2",
                        CategoryId = waterCategory?.Id ?? 5,
                        Description = "Надувной двухместный каяк с вёслами и ремонтным набором. Для спокойной воды и прогулок.",
                        PricePerDay = 35,
                        Deposit = 300,
                        AvailableQuantity = 4,
                        SupplierId = supplierId
                    },

                    // Командные виды спорта
                    new Equipment
                    {
                        Name = "Волейбольный набор (сетка + мяч)",
                        CategoryId = teamCategory?.Id ?? 6,
                        Description = "Комплект для пляжного и паркового волейбола: сетка с креплениями и мяч.",
                        PricePerDay = 15,
                        Deposit = 100,
                        AvailableQuantity = 8,
                        SupplierId = supplierId
                    },
                    new Equipment
                    {
                        Name = "Футбольные ворота складные",
                        CategoryId = teamCategory?.Id ?? 6,
                        Description = "Пара складных ворот из алюминия с сеткой и креплениями. Быстро собираются на поле.",
                        PricePerDay = 18,
                        Deposit = 120,
                        AvailableQuantity = 6,
                        SupplierId = supplierId
                    }
                };

                context.Equipments.AddRange(equipments);
                await context.SaveChangesAsync();
                Console.WriteLine($"Добавлено {equipments.Count} единиц спортивного инвентаря.");
            }

            // ===== 5. ПУНКТ ВЫДАЧИ ПОСТАВЩИКА (если его нет) =====
            if (supplierId != null && !context.Offices.Any(o => o.SupplierId == supplierId))
            {
                context.Offices.Add(new Office
                {
                    Name = "Пункт выдачи RentSport",
                    Address = "г. Минск, пр-т Победителей, 84",
                    Latitude = 53.9384,
                    Longitude = 27.4849,
                    SupplierId = supplierId
                });
                await context.SaveChangesAsync();
                Console.WriteLine("Добавлен пункт выдачи поставщика.");
            }

            // ===== 6. ДОБАВЛЕНИЕ ОТЗЫВОВ (если их нет) =====
            if (!context.Reviews.Any())
            {
                var existingUser = await userManager.FindByEmailAsync("user@rentoffice.com");
                var equipments = await context.Equipments.ToListAsync();

                if (existingUser != null && equipments.Count >= 10)
                {
                    var reviews = new List<Review>
                    {
                        new Review
                        {
                            UserId = existingUser.Id,
                            EquipmentId = equipments[0].Id, // Горный велосипед Trek Marlin 7
                            Rating = 5,
                            Comment = "Брал велосипед на выходные — всё в отличном состоянии, передачи переключаются плавно. Доставили вовремя.",
                            CreatedAt = DateTime.Now.AddDays(-7)
                        },
                        new Review
                        {
                            UserId = existingUser.Id,
                            EquipmentId = equipments[2].Id, // Электровелосипед Himo C26
                            Rating = 5,
                            Comment = "Электровелосипед просто огонь! Батареи хватило на весь день, очень удобно для поездок по городу.",
                            CreatedAt = DateTime.Now.AddDays(-5)
                        },
                        new Review
                        {
                            UserId = existingUser.Id,
                            EquipmentId = equipments[9].Id, // Горные лыжи Atomic Redster
                            Rating = 4,
                            Comment = "Лыжи подготовлены хорошо, ботинки подошли по размеру. Немного поцарапаны, но на катание не влияет.",
                            CreatedAt = DateTime.Now.AddDays(-3)
                        },
                        new Review
                        {
                            UserId = existingUser.Id,
                            EquipmentId = equipments[5].Id, // Эллиптический тренажёр Sole E35
                            Rating = 5,
                            Comment = "Тренажёр тихий и плавный, собрали за пару минут. Отличная альтернатива спортзалу дома.",
                            CreatedAt = DateTime.Now.AddDays(-1)
                        }
                    };

                    context.Reviews.AddRange(reviews);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Добавлено {reviews.Count} отзывов.");
                }
            }

            Console.WriteLine("База данных успешно инициализирована для RentSport!");
        }
    }
}
