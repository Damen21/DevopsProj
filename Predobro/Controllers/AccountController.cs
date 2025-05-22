using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Predobro.Data;
using Predobro.Models;
using System.Threading.Tasks;
using System.Linq;
using Predobro.Controllers;

[Authorize]
public class AccountController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly FoodContext _context;

    public AccountController(UserManager<ApplicationUser> userManager, FoodContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewBag.User = user;
        
        IEnumerable<Order> orders;
        
        if (User.IsInRole("Customer"))
        {
            // Load customer's orders
            orders = await _context.Orders
                .Where(o => o.CustomerId == user.Id && o.IsSubmitted)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .OrderByDescending(o => o.SubmittedAt)
                .ToListAsync();
        }
        else if (User.IsInRole("Store"))
        {
            // Load orders containing this store's items
            orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.Customer)
                .Where(o => o.OrderItems.Any(oi => oi.Item.StoreId == user.Id))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
                
            ViewBag.CurrentUserId = user.Id;
        }
        else
        {
            orders = new List<Order>();
        }
        
        return View(orders);
    }
}