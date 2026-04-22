using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace TechRent.Models.Entities
{
    public class Office
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название офиса")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Широта")]
        [NotMapped]
        public string LatitudeString
        {
            get => Latitude.ToString(CultureInfo.InvariantCulture);
            set => Latitude = double.TryParse(value?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0;
        }

        [Display(Name = "Долгота")]
        [NotMapped]
        public string LongitudeString
        {
            get => Longitude.ToString(CultureInfo.InvariantCulture);
            set => Longitude = double.TryParse(value?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Display(Name = "Адрес")]
        public string? Address { get; set; }
    }
}
