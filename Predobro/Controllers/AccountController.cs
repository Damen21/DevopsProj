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
        var orders = await _context.Orders
            .Where(o => o.CustomerId == user.Id)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.User = user;
        return View(orders);
    }
}