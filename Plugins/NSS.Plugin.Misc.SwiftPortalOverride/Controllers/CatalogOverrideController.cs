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

        #region Constructor
        public CatalogOverrideController(IShapeService shapeService, ICustomerCompanyService customerCompanyService, ICatalogModelFactory catalogModelFactory, IGenericAttributeService genericAttributeService, IWorkContext workContext, IWebHelper webHelper, IStoreContext storeContext)
        {
            _catalogModelFactory = catalogModelFactory;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _customerCompanyService = customerCompanyService;
            _shapeService = shapeService;
        }
        #endregion

        [HttpsRequirement]
        public IActionResult Index()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            CatalogModel = _catalogModelFactory.PrepareSwiftCatalogModel(new List<int>(), new List<int>(), isPageLoad: true);

            //ViewBag.dataSource = JavaScriptConvert.ToString(shapeData);
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/CustomCatalogIndex.cshtml", CatalogModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public JsonResult FilteredProductsResult([FromBody] FilterParams filterParams)
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
            CatalogModel = _catalogModelFactory.PrepareSwiftCatalogModel(shapeIds, specIds, searchKeyword);
            CatalogModel.FilterParams = filterParams;
            catalogTimer.Stop();
            Debug.Print("catalogTimer", catalogTimer.Elapsed.TotalMilliseconds);

            var shapeTimer = new Stopwatch();
            shapeTimer.Start();
            //var shapes = CatalogModel.PagingFilteringContext.ShapeFilter.FilterItems.OrderBy(s => s.Shape.Order).ToList();

            List<ShapeData> shapeData = new List<ShapeData>();
            var shapes = _shapeService.GetShapes().OrderBy(s => s.Order);

            foreach (var shape in shapes)
            {
                var childShapes = shape?.SubCategories?.ToList();
                var data = new ShapeData
                {
                    Id = shape.Id,
                    ParentId = shape.ParentId,
                    Name = $"{shape.Name}",
                    DisplayName = $"{shape.Name}",
                    Count = 0,
                    HasChild = shapes.Any(x => x.ParentId == shape.Id),
                };
                shapeData.Add(data);
            }

            shapeTimer.Stop();
            Debug.Print("catalogTimer", shapeTimer.Elapsed.TotalMilliseconds);

            filterTimer.Stop();
            Debug.Print("filterTimer", filterTimer.Elapsed.TotalMilliseconds);

            return Json(
                new {
                    partialView = RenderPartialViewToString("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/_FilteredPartialView.cshtml", CatalogModel),
                    shapes = JavaScriptConvert.ToString(shapeData),
                    specs = JavaScriptConvert.ToString(CatalogModel.PagingFilteringContext.SpecificationFilter.FilterItems.GroupBy(sf => sf.SpecificationAttributeName).Select(x => x.OrderBy(y => y.SpecificationAttributeOptionName))),
                }
            );
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
        }

        public CatalogModel CatalogModel { get; set; }
    }

    public static class JavaScriptConvert
    {
        //public static IHtmlString SerializeObject(object value)
        //{
        //    using (var stringWriter = new StringWriter())
        //    using (var jsonWriter = new JsonTextWriter(stringWriter))
        //    {
        //        var serializer = new JsonSerializer
        //        {
        //            // Let's use camelCasing as is common practice in JavaScript
        //            ContractResolver = new CamelCasePropertyNamesContractResolver()
        //        };

        //        // We don't want quotes around object names
        //        jsonWriter.QuoteName = false;
        //        serializer.Serialize(jsonWriter, value);

        //        return new HtmlString(stringWriter.ToString());
        //    }
        //}

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