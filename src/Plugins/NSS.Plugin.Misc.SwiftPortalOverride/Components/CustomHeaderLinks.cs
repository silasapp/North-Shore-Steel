using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Common;
using System.Threading.Tasks;

namespace Nop.Web.Components
{
    public class CustomHeaderLinksViewComponent : NopViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;
        public CustomHeaderLinksViewComponent(ICommonModelFactory commonModelFactory)
        {
            _commonModelFactory = commonModelFactory;
        }
        public async Task<IViewComponentResult> Invoke()
        {
            var model = await _commonModelFactory.PrepareHeaderLinksModelAsync();
            return View(model);
        }
    }
}
