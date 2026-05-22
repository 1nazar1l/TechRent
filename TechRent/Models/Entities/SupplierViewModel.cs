namespace TechRent.Models.Entities
{
    public class SupplierViewModel
    {
        public int TotalEquipment { get; set; }
        public int ActiveRentals { get; set; }
        public decimal TotalEarnings { get; set; }
        public double AverageRating { get; set; }
        public List<SupplierEquipmentItem> Equipment { get; set; } = new List<SupplierEquipmentItem>();
        public List<Category> Categories { get; set; } = new List<Category>();
    }

    public class SupplierEquipmentItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public Category Category { get; set; }
        public int PricePerDay { get; set; }
        public int Deposit { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsAvailable => AvailableQuantity > 0;
        public int TotalRentals { get; set; }
    }
}