using System.ComponentModel.DataAnnotations;

namespace TechRent.Models.Entities
{
    public class Delivery
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Адрес обязателен")]
        [Display(Name = "Адрес доставки")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Стоимость доставки")]
        public decimal Cost { get; set; }

        [Required]
        [Display(Name = "Дата доставки")]
        [DataType(DataType.Date)]
        public DateTime DeliveryDate { get; set; }

        [Required]
        [Display(Name = "Статус доставки")]
        public string Status { get; set; } = "Ожидает";

        // Навигационное свойство
        public Booking? Booking { get; set; }
    }
}