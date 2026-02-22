using System.ComponentModel.DataAnnotations;

namespace TechRent.Models.Entities
{
    public class Equipment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        // Внешний ключ для категории
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        // Навигационное свойство для категории
        [Display(Name = "Категория")]
        public Category? Category { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Display(Name = "Цена за день")]
        [Range(1, int.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        public int PricePerDay { get; set; }

        [Required(ErrorMessage = "Залог обязателен")]
        [Display(Name = "Залог")]
        [Range(0, int.MaxValue)]
        public int Deposit { get; set; }

        [Display(Name = "Изображение (URL)")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Количество обязательно")]
        [Display(Name = "Доступное количество")]
        [Range(0, int.MaxValue)]
        public int AvailableQuantity { get; set; }

        // Вычисляемое поле (не сохраняется в БД)
        public bool IsAvailable => AvailableQuantity > 0;

        // Навигационные свойства
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}