using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TechRent.Models.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // внешний ключ к IdentityUser

        [Required]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        [Display(Name = "Дата начала")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Дата окончания обязательна")]
        [Display(Name = "Дата окончания")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Общая стоимость")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Удержанный залог")]
        public decimal DepositPaid { get; set; }

        [Display(Name = "Штраф")]
        public decimal Fine { get; set; }

        [Required]
        [Display(Name = "Статус")]
        public string Status { get; set; } = "Подтверждено"; // Подтверждено, Завершено, Отменено

        // Навигационные свойства
        public IdentityUser? User { get; set; }
        public Equipment? Equipment { get; set; }
        public Delivery? Delivery { get; set; }
    }
}