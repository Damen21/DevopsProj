using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Predobro.Controllers;
using Predobro.Data;
using Predobro.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

[Authorize(Roles = "Customer")]
public class OrderController : BaseController
{
    private readonly FoodContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(FoodContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // List completed orders for the current user
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var orders = await _context.Orders
            .Where(o => o.CustomerId == user.Id && o.IsSubmitted)  // Only submitted orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
            .OrderByDescending(o => o.SubmittedAt)  // Sort by submission date
            .ToListAsync();
        return View(orders);
    }

    // Show details for a specific order
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == user.Id);

        if (order == null)
            return NotFound();

        return View(order);
    }

    // Add item to cart
    [HttpPost]
    public async Task<IActionResult> AddToCart(int itemId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        // Get the item and check quantity
        var item = await _context.Items.FindAsync(itemId);
        if (item == null)
            return NotFound();
            
        if (item.Quantity <= 0)
            return RedirectToAction("Index", "Home");
        
        // Find or create user's cart (unsubmitted order)
        var cart = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.CustomerId == user.Id && !o.IsSubmitted);

        // If no cart exists, create a new one
        if (cart == null)
        {
            cart = new Order
            {
                CustomerId = user.Id,
                CreatedAt = DateTime.Now,
                IsSubmitted = false,
                OrderItems = new List<OrderItem>()
            };
            _context.Orders.Add(cart);
            await _context.SaveChangesAsync(); // Save to get cart.Id
        }

        // Check if this item is already in the cart
        var existingCartItem = cart.OrderItems
            .FirstOrDefault(oi => oi.ItemId == itemId);

        if (existingCartItem != null)
        {
            // Item already in cart - just increment quantity if stock allows
            if (item.Quantity >= 1) {
                existingCartItem.Quantity += 1;
                item.Quantity -= 1;
            } else {
                TempData["Error"] = "Not enough stock available";
            }
        }
        else
        {
            // Add new item to cart
            var orderItem = new OrderItem
            {
                OrderId = cart.Id,
                ItemId = itemId,
                Quantity = 1
            };
            _context.OrderItems.Add(orderItem);
            
            // Reduce item quantity by 1
            item.Quantity -= 1;
        }

        await _context.SaveChangesAsync();
        
        // Show a confirmation message
        TempData["Message"] = "Item added to your cart!";
        
        // Redirect to previous page or home
        return RedirectToAction("Index", "Home");
    }

    // Show the user's cart 
    public async Task<IActionResult> Cart()
    {
        var user = await _userManager.GetUserAsync(User);
        var cart = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)
            .FirstOrDefaultAsync(o => o.CustomerId == user.Id && !o.IsSubmitted);

        return View(cart);
    }

    // Remove item from cart
    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int orderItemId)
    {
        var user = await _userManager.GetUserAsync(User);
        var orderItem = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Item)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && 
                                      oi.Order.CustomerId == user.Id && 
                                      !oi.Order.IsSubmitted);

        if (orderItem == null)
            return NotFound();

        // Return quantity to item stock
        orderItem.Item.Quantity += orderItem.Quantity;
        
        // Remove the order item
        _context.OrderItems.Remove(orderItem);
        
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Cart));
    }

    // Update item quantity in cart
    [HttpPost]
    public async Task<IActionResult> UpdateCartItem(int orderItemId, int quantity)
    {
        if (quantity <= 0)
            return RedirectToAction(nameof(RemoveFromCart), new { orderItemId });
            
        var user = await _userManager.GetUserAsync(User);
        var orderItem = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Item)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && 
                                      oi.Order.CustomerId == user.Id && 
                                      !oi.Order.IsSubmitted);

        if (orderItem == null)
            return NotFound();

        // Calculate difference in quantity
        int diff = quantity - orderItem.Quantity;
        
        // Check if item has enough stock for an increase
        if (diff > 0 && orderItem.Item.Quantity < diff)
        {
            // Not enough stock
            TempData["Error"] = "Not enough stock available";
            return RedirectToAction(nameof(Cart));
        }
        
        // Update item stock
        orderItem.Item.Quantity -= diff;
        
        // Update order item quantity
        orderItem.Quantity = quantity;
        
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Cart));
    }

    // Convert cart to order (checkout)
    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        var user = await _userManager.GetUserAsync(User);
        var cart = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.CustomerId == user.Id && !o.IsSubmitted);

        if (cart == null || !cart.OrderItems.Any())
        {
            TempData["Error"] = "Your cart is empty!";
            return RedirectToAction(nameof(Cart));
        }
            
        // Mark cart as submitted (converting it to an order)
        cart.IsSubmitted = true;
        cart.SubmittedAt = DateTime.Now;
        
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(OrderComplete), new { id = cart.Id });
    }

    // Order complete page
    public async Task<IActionResult> OrderComplete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)
            .FirstOrDefaultAsync(o => o.Id == id && 
                                     o.CustomerId == user.Id && 
                                     o.IsSubmitted);
            
        if (order == null)
            return NotFound();
            
        return View(order);
    }
    
    // Delete an order (only completed orders)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
            .FirstOrDefaultAsync(o => o.Id == id && 
                                     o.CustomerId == user.Id && 
                                     o.IsSubmitted);  // Only allow deleting submitted orders

        if (order == null)
            return NotFound();
            
        // Restore quantities for each item in this order
        foreach (var orderItem in order.OrderItems)
        {
            // If the item still exists, restore its quantity
            if (orderItem.Item != null)
            {
                orderItem.Item.Quantity += orderItem.Quantity;
            }
        }
        
        // Remove order and its items
        _context.OrderItems.RemoveRange(order.OrderItems);
        _context.Orders.Remove(order);
        
        await _context.SaveChangesAsync();
        
        // Redirect back to orders page
        return RedirectToAction(nameof(Index));
    }
}