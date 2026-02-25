using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TechRent.Models.Entities
{
    public class Favorite
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int EquipmentId { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Навигационные свойства
        public IdentityUser User { get; set; }
        public Equipment Equipment { get; set; }
    }
}