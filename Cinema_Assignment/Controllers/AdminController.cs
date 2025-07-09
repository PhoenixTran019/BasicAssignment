using Microsoft.AspNetCore.Mvc;

namespace Cinema_Assignment.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Home()
        {
            return View();
        }
    }
}
