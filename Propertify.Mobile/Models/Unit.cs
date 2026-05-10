namespace Propertify.Mobile.Models
{
    public class Unit
    {
        public int Id { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public decimal RentAmount { get; set; }
        public double Area { get; set; }
        public bool IsOccupied { get; set; } = false;
        public string Status { get; set; } = "Vacant";
        public string? ElectricityMeter { get; set; }
        public string? WaterMeter { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int Kitchens { get; set; }
        public int LivingRooms { get; set; }
        public int Majlis { get; set; }
        public string? UnitImages { get; set; }
        public string? VideoPath { get; set; }
        public int PropertyId { get; set; }

        public string? ThumbnailUrl =>
            !string.IsNullOrEmpty(UnitImages) ? UnitImages.Split(',')[0] : null;
    }
}
