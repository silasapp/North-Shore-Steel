using Microsoft.AspNetCore.Mvc;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Components
{
    public class CustomCustomerNavigationViewComponent : NopViewComponent
    {
        private readonly ICustomerModelFactory _customerModelFactory;

        public CustomCustomerNavigationViewComponent(ICustomerModelFactory customerModelFactory)
        {
            _customerModelFactory = customerModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool isABuyer, bool isOperations, int selectedTabId = 0)
        {
            var model = await _customerModelFactory.PrepareCustomerNavigationModelAsync(isABuyer, isOperations,selectedTabId);
            return View(model);
        }
    }
}
