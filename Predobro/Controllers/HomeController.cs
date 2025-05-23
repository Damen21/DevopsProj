using System.Diagnostics;
using System.Text.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Predobro.Controllers;
using Predobro.Data;
using Predobro.Models;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly FoodContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(ILogger<HomeController> logger, FoodContext context, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    // In your HomeController, modify the Index action to exclude items with quantity 0
    public async Task<IActionResult> Index()
    {
        var items = await _context.Items
            .Where(i => i.Quantity > 0)  // Only show items with quantity > 0
            .Include(i => i.Store) // Include the store (ApplicationUser) data
            .ToListAsync();
        
        // Get unique stores with their coordinates
        var storesWithItems = new List<object>();
        var processedStores = new HashSet<string>();
        
        foreach (var item in items)
        {
            if (!processedStores.Contains(item.Store.Id))
            {
                var coordinates = await GetCoordinatesForAddress(item.Store.Address);
                
                storesWithItems.Add(new {
                    Store = new {
                        Id = item.Store.Id,
                        FullName = item.Store.FullName,
                        Address = item.Store.Address
                    },
                    Items = items.Where(i => i.StoreId == item.Store.Id).Select(i => new {
                        Name = i.Name,
                        Price = (double)i.Price
                    }).ToList(),
                    Latitude = coordinates.Latitude,
                    Longitude = coordinates.Longitude
                });
                
                processedStores.Add(item.Store.Id);
            }
        }
        
        ViewBag.StoresWithCoordinates = storesWithItems;
        return View(items);
    }

    private async Task<(double Latitude, double Longitude)> GetCoordinatesForAddress(string address)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            // Use Nominatim (OpenStreetMap's geocoding service) - it's free
            var encodedAddress = Uri.EscapeDataString($"{address}, Ljubljana, Slovenia");
            var url = $"https://nominatim.openstreetmap.org/search?q={encodedAddress}&format=json&limit=1";
            
            // Add a user agent (required by Nominatim)
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Predobro-App/1.0");
            
            var response = await httpClient.GetStringAsync(url);
            var results = JsonSerializer.Deserialize<NominatimResult[]>(response);
            
            if (results != null && results.Length > 0)
            {
                return (double.Parse(results[0].lat), double.Parse(results[0].lon));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Geocoding failed for address '{address}': {ex.Message}");
        }
        
        // Fallback to Ljubljana center if geocoding fails
        return (46.0569, 14.5058);
    }

    public class NominatimResult
    {
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }
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
}
