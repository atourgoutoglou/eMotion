using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMotion.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}