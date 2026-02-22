using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechRent.Models.Entities;

namespace TechRent.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Добавляем наши таблицы
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Category> Categories { get; set; } // Добавить эту строку

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // обязательно для Identity

            // Настройка связей между таблицами

            // Booking -> User (один ко многим)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict); // запрещаем каскадное удаление

            // Booking -> Equipment (один ко многим)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Equipment)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking -> Delivery (один к одному)
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Booking)
                .WithOne(b => b.Delivery)
                .HasForeignKey<Delivery>(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review -> User
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Equipment
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Equipment)
                .WithMany(e => e.Reviews)
                .HasForeignKey(r => r.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка связи между Equipment и Category
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Equipments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Запрещаем удаление категории, если есть оборудование
        }
    }
}