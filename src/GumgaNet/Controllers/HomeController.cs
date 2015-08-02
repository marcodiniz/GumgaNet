using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace MovieAngular.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var user = (ClaimsIdentity) User.Identity;
            ViewBag.Name = user.Name;
            ViewBag.CanEdit = user.FindFirst("CanEdit") != null ? "true" : "false";
            return View();
        }
    }
}