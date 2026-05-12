using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SCRANKRealEstate.Models;

namespace SCRANKRealEstate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Sample data - in production, this would come from a database
            var featuredProperties = GetFeaturedProperties();
            return View(featuredProperties);
        }

        public IActionResult Properties()
        {
            var properties = GetAllProperties();
            return View(properties);
        }

        public IActionResult PropertyDetails(int id)
        {
            var property = GetAllProperties().FirstOrDefault(p => p.Id == id);
            if (property == null)
            {
                return NotFound();
            }
            return View(property);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string phone, string message)
        {
            // In production, save to database or send email
            _logger.LogInformation($"Contact form submitted by {name} ({email})");
            TempData["Message"] = "Thank you for contacting us! We'll get back to you soon.";
            return RedirectToAction("Contact");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Sample data methods - replace with database calls in production
        private List<Property> GetFeaturedProperties()
        {
            return GetAllProperties().Where(p => p.IsFeatured).Take(6).ToList();
        }

        private List<Property> GetAllProperties()
        {
            return new List<Property>
            {
                new Property
                {
                    Id = 1,
                    Address = "123 Mountain View Drive",
                    City = "Boise",
                    State = "Idaho",
                    ZipCode = "83702",
                    Price = 475000,
                    Bedrooms = 4,
                    Bathrooms = 2.5m,
                    SquareFeet = 2400,
                    PropertyType = "Single Family",
                    Description = "Beautiful 4-bedroom home with stunning mountain views. Updated kitchen, hardwood floors, and spacious backyard perfect for entertaining.",
                    ImageUrl = "/images/properties/house1.jpg",
                    ListedDate = DateTime.Now.AddDays(-15),
                    IsFeatured = true,
                    Status = "Available"
                },
                new Property
                {
                    Id = 2,
                    Address = "456 River Street",
                    City = "Coeur d'Alene",
                    State = "Idaho",
                    ZipCode = "83814",
                    Price = 625000,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    SquareFeet = 1950,
                    PropertyType = "Condo",
                    Description = "Luxury waterfront condo with panoramic lake views. Modern finishes, open floor plan, and resort-style amenities.",
                    ImageUrl = "/images/properties/house2.jpg",
                    ListedDate = DateTime.Now.AddDays(-8),
                    IsFeatured = true,
                    Status = "Available"
                },
                new Property
                {
                    Id = 3,
                    Address = "789 Pine Ridge Lane",
                    City = "Idaho Falls",
                    State = "Idaho",
                    ZipCode = "83401",
                    Price = 385000,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    SquareFeet = 1800,
                    PropertyType = "Townhouse",
                    Description = "Charming townhouse in quiet neighborhood. Perfect starter home with low maintenance and great schools nearby.",
                    ImageUrl = "/images/properties/house3.jpg",
                    ListedDate = DateTime.Now.AddDays(-3),
                    IsFeatured = true,
                    Status = "Available"
                },
                new Property
                {
                    Id = 4,
                    Address = "321 Eagle Summit Road",
                    City = "Sun Valley",
                    State = "Idaho",
                    ZipCode = "83353",
                    Price = 1250000,
                    Bedrooms = 5,
                    Bathrooms = 4,
                    SquareFeet = 4200,
                    PropertyType = "Single Family",
                    Description = "Luxury mountain estate with ski-in/ski-out access. Gourmet kitchen, wine cellar, and home theater.",
                    ImageUrl = "/images/properties/house4.jpg",
                    ListedDate = DateTime.Now.AddDays(-20),
                    IsFeatured = true,
                    Status = "Available"
                },
                new Property
                {
                    Id = 5,
                    Address = "555 Downtown Boulevard",
                    City = "Boise",
                    State = "Idaho",
                    ZipCode = "83702",
                    Price = 295000,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    SquareFeet = 1100,
                    PropertyType = "Condo",
                    Description = "Modern urban condo in the heart of downtown. Walk to restaurants, shops, and entertainment.",
                    ImageUrl = "/images/properties/house5.jpg",
                    ListedDate = DateTime.Now.AddDays(-5),
                    IsFeatured = true,
                    Status = "Pending"
                },
                new Property
                {
                    Id = 6,
                    Address = "888 Harvest Circle",
                    City = "Meridian",
                    State = "Idaho",
                    ZipCode = "83642",
                    Price = 525000,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    SquareFeet = 2800,
                    PropertyType = "Single Family",
                    Description = "Spacious family home with bonus room and 3-car garage. Large lot with mature landscaping.",
                    ImageUrl = "/images/properties/house6.jpg",
                    ListedDate = DateTime.Now.AddDays(-12),
                    IsFeatured = true,
                    Status = "Available"
                }
            };
        }
    }
}
