using Microsoft.AspNetCore.Mvc;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using Nop.Web.Framework.Components;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Components
{
    public class CustomCustomerNavigationViewComponent : NopViewComponent
    {
        private readonly ICustomerModelFactory _customerModelFactory;

        public CustomCustomerNavigationViewComponent(ICustomerModelFactory customerModelFactory)
        {
            _customerModelFactory = customerModelFactory;
        }

        public IViewComponentResult Invoke(bool isABuyer, int selectedTabId = 0)
        {
            var model = _customerModelFactory.PrepareCustomerNavigationModel(isABuyer, selectedTabId);
            return View(model);
        }
    }
}
