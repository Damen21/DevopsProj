using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Predobro.Data;
using System.Security.Claims;

namespace Predobro.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            
            if (User.Identity.IsAuthenticated && User.IsInRole("Customer"))
            {
                // Get cart count
                var dbContext = context.HttpContext.RequestServices.GetService<FoodContext>();
                var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (dbContext != null && userId != null)
                {
                    var cartItemCount = dbContext.Orders
                        .Where(o => o.CustomerId == userId && !o.IsSubmitted)
                        .SelectMany(o => o.OrderItems)
                        .Sum(oi => oi.Quantity);
                        
                    ViewBag.CartItemCount = cartItemCount;
                }
            }
        }
    }
}