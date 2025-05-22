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
                    .ThenInclude(i => i.Store)
                .OrderByDescending(o => o.SubmittedAt)
                .ToListAsync();
        }
        else if (User.IsInRole("Store"))
        {
            // Load orders containing this store's items (exclude orders where all this store's items are completed)
            orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.Customer)
                .Where(o => o.OrderItems.Any(oi => oi.Item.StoreId == user.Id) && 
                           o.IsSubmitted && 
                           o.OrderItems.Any(oi => oi.Item.StoreId == user.Id && oi.Status != OrderItemStatus.Completed))
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

    // Update to handle individual order item status
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderItemStatus(int orderItemId, OrderItemStatus status)
    {
        var user = await _userManager.GetUserAsync(User);
        
        // Only allow stores to update order status
        if (!User.IsInRole("Store"))
            return Forbid();
        
        // Find the order item and verify it belongs to this store
        var orderItem = await _context.OrderItems
            .Include(oi => oi.Item)
            .Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && 
                                oi.Item.StoreId == user.Id &&
                                oi.Order.IsSubmitted);
        
        if (orderItem == null)
            return NotFound();
        
        // Update the status
        orderItem.Status = status;
        await _context.SaveChangesAsync();
        
        TempData["Message"] = $"Item '{orderItem.Item.Name}' in Order #{orderItem.OrderId} status updated to {status}";
        return RedirectToAction(nameof(Index));
    }
}