using Microsoft.AspNetCore.Mvc;
using SCRANKRealEstate.Models;
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace SCRANKRealEstate.Controllers
{
    public class HomeController : Controller
    {
        public static class Global
        {
            public static string gstrConnectionString = "Server=Spennys_PC_25\\SQLEXPRESS;Database=SCRANK;Integrated Security=True;Trust Server Certificate=True";
        }

        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var featuredProperties = GetFeaturedProperties();
            return View(featuredProperties);
        }

        public IActionResult Properties()
        {
            var allProperties = GetAllProperties();
            
            // Get top 3 featured properties ordered by FeatureNumber
            var featuredProperties = allProperties
                .Where(p => p.IsFeatured)
                .OrderBy(p => p.FeatureNumber)
                .Take(3)
                .ToList();
            
            // Get all non-featured properties ordered by Id (ListingKey)
            var nonFeaturedProperties = allProperties
                .Where(p => !p.IsFeatured)
                .OrderBy(p => p.Id)
                .ToList();
            
            // Get remaining featured properties (after top 3) ordered by Id
            var remainingFeaturedProperties = allProperties
                .Where(p => p.IsFeatured)
                .OrderBy(p => p.FeatureNumber)
                .Skip(3)
                .OrderBy(p => p.Id)
                .ToList();
            
            // Combine: top 3 featured, then all non-featured, then remaining featured
            var sortedProperties = featuredProperties
                .Concat(nonFeaturedProperties)
                .Concat(remainingFeaturedProperties)
                .ToList();
            
            return View(sortedProperties);
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

        private List<Property> GetFeaturedProperties()
        {
            return GetAllProperties()
                .Where(p => p.IsFeatured)
                .OrderBy(p => p.FeatureNumber)
                .Take(3)
                .ToList();
        }

        public static void GetListings(DataTable pdtSchools)
        {
            string strqueryStatement = "Select * from tblRealEstateListingDetails";
            CallQuery(strqueryStatement, pdtSchools, Global.gstrConnectionString);
        }

        public static void CallQuery(string pstrQuery, DataTable pdtTable, string pstrConnectionstring)
        {
            using (SqlConnection _con = new SqlConnection(pstrConnectionstring))
            {
                using (SqlCommand _cmd = new SqlCommand(pstrQuery, _con))
                {
                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);
                    _con.Open();
                    _dap.Fill(pdtTable);
                    _con.Close();
                }
            }
        }

        // Helper method to get all images for a listing
        private List<string> GetPropertyImages(int listingId)
        {
            var imageUrls = new List<string>();
            var listingFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", $"Listing_{listingId}");

            if (Directory.Exists(listingFolder))
            {
                var imageFiles = Directory.GetFiles(listingFolder, "*.jpg")
                    .Concat(Directory.GetFiles(listingFolder, "*.jpeg"))
                    .Concat(Directory.GetFiles(listingFolder, "*.png"))
                    .OrderBy(f => f)
                    .ToList();

                foreach (var imageFile in imageFiles)
                {
                    var fileName = Path.GetFileName(imageFile);
                    imageUrls.Add($"/images/Listing_{listingId}/{fileName}");
                }
            }

            // If no images found, add default image
            if (!imageUrls.Any())
            {
                imageUrls.Add("/images/default-house.jpg");
            }

            return imageUrls;
        }

        private List<Property> GetAllProperties()
        {
            var properties = new List<Property>();
            DataTable dtProperties = new DataTable();
            GetListings(dtProperties);

            foreach (DataRow row in dtProperties.Rows)
            {
                int listingId = Convert.ToInt32(row["flngListingKey"]);
                
                // Get all images for this listing
                var imageUrls = GetPropertyImages(listingId);

                properties.Add(new Property
                {
                    Id = listingId,
                    Address = row["fstrAddress"].ToString(),
                    City = row["fstrCity"].ToString(),
                    State = row["fstrState"].ToString(),
                    ZipCode = row["fintZipCode"].ToString(),
                    Price = Convert.ToDecimal(row["flngPrice"]),
                    Bedrooms = Convert.ToInt32(row["fintBeds"]),
                    Bathrooms = Convert.ToDecimal(row["flngBaths"]),
                    SquareFeet = Convert.ToInt32(row["fintSquareFeet"]),
                    Acres = Convert.ToDecimal(row["flngAcres"]),
                    PropertyType = row["fstrType"].ToString(),
                    Description = row["fstrDescription"]?.ToString() ?? string.Empty,
                    ImageUrl = imageUrls.FirstOrDefault() ?? "/images/default-house.jpg", // First image for thumbnails
                    ImageUrls = imageUrls, // All images for gallery
                    ListedDate = Convert.ToDateTime(row["fdtmListingDate"]),
                    IsFeatured = row["fblnIsFeatured"] != DBNull.Value && Convert.ToBoolean(row["fblnIsFeatured"]),
                    FeatureNumber = row["fintFeatureNumber"] != DBNull.Value ? Convert.ToInt32(row["fintFeatureNumber"]) : 999, 
                    MLSNumber = row["fintMLSNumber"] != DBNull.Value ? Convert.ToInt32(row["fintMLSNumber"]) : 999, 
                    Status = row["fstrStatus"].ToString()
                });
            }

            return properties;
        }

        public IActionResult MortgageCalculator()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }
    }
}
