using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Predobro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public IActionResult Dashboard()
        {
            // Admin dashboard logic here
            return View();
        }
    }
}