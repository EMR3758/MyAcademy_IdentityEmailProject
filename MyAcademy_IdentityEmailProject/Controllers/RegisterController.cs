using Microsoft.AspNetCore.Mvc;

namespace MyAcademy_IdentityEmailProject.Controllers
{
    public class RegisterController : Controller
    {

        public IActionResult SignUp()
        {
            return View();
        }
    }
}
