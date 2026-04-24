using Microsoft.AspNetCore.Mvc;

namespace FinPlan.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}