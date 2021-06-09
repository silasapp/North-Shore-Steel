using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes; 
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using Syncfusion.EJ2.Linq;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
using static NSS.Plugin.Misc.SwiftPortalOverride.Models.CatalogModel;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.Services;
using System.Diagnostics;
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;
using System.Text.RegularExpressions;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class CatalogOverrideController : BasePublicController
    {
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly IShapeService _shapeService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;

        #region Constructor
        public CatalogOverrideController(IShoppingCartService shoppingCartService, ILocalizationService localizationService, IShapeService shapeService, ICustomerCompanyService customerCompanyService, ICatalogModelFactory catalogModelFactory, IGenericAttributeService genericAttributeService, IWorkContext workContext, IWebHelper webHelper, IStoreContext storeContext)
        {
            _catalogModelFactory = catalogModelFactory;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _customerCompanyService = customerCompanyService;
            _shapeService = shapeService;
            _shoppingCartService = shoppingCartService;
            _localizationService = localizationService;
        }
        #endregion

        [HttpsRequirement]
        public async Task<IActionResult> Index()
        {
            // _workContext.CurrentCustomer changed to _workContext.GetCurrentCustomerAsync()
            // _workContext.CurrentStore changed to _workContext.GetCurrentStoreAsync()
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.GetCurrentCustomerAsync().Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            if (!(await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer)))
                return AccessDeniedView();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            CatalogModel = await _catalogModelFactory.PrepareSwiftCatalogModelAsync(new List<int>(), new List<int>(), isPageLoad: true);

            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/CustomCatalogIndex.cshtml", CatalogModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> FilteredProductsResult([FromBody] FilterParams filterParams)
        {
            Stopwatch filterTimer = new Stopwatch();
            filterTimer.Start();

            var shapeIds = filterParams?.ShapeIds;
            var specIds = filterParams?.SpecIds;
            var searchKeyword = filterParams?.SearchKeyword;
            if (shapeIds == null)
                shapeIds = new List<int>();
            if (specIds == null)
                specIds = new List<int>();

            var catalogTimer = new Stopwatch();
            catalogTimer.Start();
            CatalogModel = await _catalogModelFactory.PrepareSwiftCatalogModelAsync(shapeIds, specIds, searchKeyword);
            CatalogModel.FilterParams = filterParams;
            catalogTimer.Stop();
            Debug.Print("catalogTimer", catalogTimer.Elapsed.TotalMilliseconds);

            filterTimer.Stop();
            Debug.Print("filterTimer", filterTimer.Elapsed.TotalMilliseconds);
            CatalogModel.ActiveShapeAttributes = filterParams?.ActiveShapeAttributes?.ToList();

            return Json(
                new {
                    partialView = await RenderPartialViewToStringAsync("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/_FilteredPartialView.cshtml", CatalogModel),
                    shapes = JavaScriptConvert.ToString(CatalogModel.PagingFilteringContext.ShapeFilter.FilterItems),
                    specs = JavaScriptConvert.ToString(CatalogModel.PagingFilteringContext.SpecificationFilter.FilterItems),
                }
            );
        }

        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> RemoveFromFavorites([FromBody] int itemId)
        {

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = itemId == sci.ProductId;
                if (remove)
                {
                    await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var updatetopwishlistsectionhtml = string.Format(await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                          shoppingCarts.Count);
                    updatetopwishlistsectionhtml = Regex.Replace(updatetopwishlistsectionhtml, @"[()]+", "");

                    return Json(new
                    {
                        success = true,
                        message = "The product has been removed from your favorites.",
                        updatetopwishlistsectionhtml
                    });
                }
            }

            return Json(new
            {
                success = false,
                message = ""
            });
        }

        public class PartialViewWithData
        {
            public PartialViewResult PartialViewData { get; set; }
            public List<ShapeData> Shapes { get; set; }
        }

        public class FilterParams
        {
            public List<int> SpecIds { get; set; }
            public List<int> ShapeIds { get; set; }
            public string SearchKeyword { get; set; }
            public IList<ShapeAttribute> ActiveShapeAttributes { get; set; }
            public bool SawOption { get; set; }
        }

        public CatalogModel CatalogModel { get; set; }
    }

    public static class JavaScriptConvert
    {

        public static string ToString(object data)
        {
            var contractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None
            });
        }
    }
}

public class SawOptionsAttribute
{
    public int Id { get; set; }
    public string Name { get; set; }
    public object Values { get; set; }
}