using System.ComponentModel.DataAnnotations;

namespace TechRent.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название категории обязательно")]
        [Display(Name = "Название категории")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Порядок отображения")]
        public int DisplayOrder { get; set; } = 0;

        // Навигационное свойство - список оборудования в этой категории
        public ICollection<Equipment>? Equipments { get; set; }
    }
}