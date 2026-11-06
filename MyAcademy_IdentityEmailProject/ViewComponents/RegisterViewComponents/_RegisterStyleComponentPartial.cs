using Microsoft.AspNetCore.Mvc;

namespace MyAcademy_IdentityEmailProject.ViewComponents.RegisterViewComponents
{
    public class _RegisterStyleComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
