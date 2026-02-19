using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TechRent.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Поставьте оценку")]
        [Range(1, 5, ErrorMessage = "Оценка от 1 до 5")]
        [Display(Name = "Оценка")]
        public int Rating { get; set; }

        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }

        [Display(Name = "Дата отзыва")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Навигационные свойства
        public IdentityUser? User { get; set; }
        public Equipment? Equipment { get; set; }
    }
}