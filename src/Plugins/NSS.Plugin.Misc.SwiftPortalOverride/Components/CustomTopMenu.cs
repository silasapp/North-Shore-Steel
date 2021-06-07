using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Components
{
    public class CustomTopMenuViewComponent : NopViewComponent
    {
        public CustomTopMenuViewComponent()
        {

        }
        public IViewComponentResult Invoke()
        {
            var model = new TopMenuModel();
            return View(model);
        }
    }
}
