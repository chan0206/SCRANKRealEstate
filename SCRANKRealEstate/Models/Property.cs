namespace SCRANKRealEstate.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = "Idaho";
        public string ZipCode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Bedrooms { get; set; }
        public decimal Bathrooms { get; set; }
        public int SquareFeet { get; set; }
        public string PropertyType { get; set; } = string.Empty; // e.g., "Single Family", "Condo", "Townhouse"
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "/images/properties/default-house.jpg";
        public DateTime ListedDate { get; set; }
        public bool IsFeatured { get; set; }
        public string Status { get; set; } = "Available"; // Available, Pending, Sold
    }
}