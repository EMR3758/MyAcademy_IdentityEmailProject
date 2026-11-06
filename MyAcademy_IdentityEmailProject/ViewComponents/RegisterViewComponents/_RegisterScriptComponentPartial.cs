using Microsoft.AspNetCore.Mvc;

namespace MyAcademy_IdentityEmailProject.ViewComponents.RegisterViewComponents
{
    public class _RegisterScriptComponentPartial: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
