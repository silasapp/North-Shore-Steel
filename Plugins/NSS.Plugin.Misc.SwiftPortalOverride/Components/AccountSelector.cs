using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class AccountSelectorViewComponent : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public AccountSelectorViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/Shared/Components/AccountSelector/Default.cshtml");
        }
    }
}
