using Microsoft.AspNetCore.Mvc;

namespace MyAcademy_IdentityEmailProject.Controllers
{
    public class MessageLayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
