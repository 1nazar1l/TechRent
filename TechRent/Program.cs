using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechRent.Data;
using Microsoft.AspNetCore.Identity.UI;

namespace TechRent
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            // НАСТРОЙКА IDENTITY
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddRazorPages(); // ВАЖНО: добавляем поддержку Razor Pages для Identity

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication(); // обязательно для Identity
            app.UseAuthorization();

            // ВАЖНО: добавляем маршруты для Razor Pages Identity
            app.MapRazorPages(); // ЭТО НУЖНО ДОБАВИТЬ!
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Инициализация БД
            using (var scope = app.Services.CreateScope())
            {
                await DbInitializer.Initialize(scope.ServiceProvider);
            }

            app.Run();
        }
    }
}