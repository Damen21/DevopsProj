using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Predobro.Controllers;
using Predobro.Data;
using Predobro.Models;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly FoodContext _context;

    public HomeController(ILogger<HomeController> logger, FoodContext context)
    {
        _logger = logger;
        _context = context;
    }

    // In your HomeController, modify the Index action to exclude items with quantity 0
    public async Task<IActionResult> Index()
    {
        var items = await _context.Items
            .Where(i => i.Quantity > 0)  // Only show items with quantity > 0
            .ToListAsync();
        return View(items);
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
