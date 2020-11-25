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

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class CatalogOverrideController : BasePublicController
    {
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;

        #region Constructor
        public CatalogOverrideController(ICatalogModelFactory catalogModelFactory, IGenericAttributeService genericAttributeService, IWorkContext workContext, IWebHelper webHelper, IStoreContext storeContext)
        {
            _catalogModelFactory = catalogModelFactory;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _webHelper = webHelper;
            _storeContext = storeContext;
        }
        #endregion

        [HttpsRequirement]
        public IActionResult Index()
        {
            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var shapeIds = new List<int>();
            var specIds = new List<int>();

            CatalogModel = _catalogModelFactory.PrepareSwiftCatalogModel(shapeIds, specIds);

            var shapes = CatalogModel.PagingFilteringContext.ShapeFilter.FilterItems.Select(x => x.Shape).OrderBy(s => s.Order ).ToList();

            List<ShapeData> shapeData = new List<ShapeData>();
            for (var i = 0; i < shapes.Count; i++)
            {
                var childShapes = shapes[i].SubCategories.ToList();
                var shape = new ShapeData
                {
                    id = shapes[i].Id,
                    pid = shapes[i].ParentId,
                    name = shapes[i].Name,
                    hasChild = childShapes != null && childShapes.Count > 0
                };
                shapeData.Add(shape);

                if (childShapes != null && childShapes.Count > 0)
                    for (int j = 0; j < childShapes.Count; j++)
                    {
                        shape = new ShapeData
                        {
                            id = childShapes[j].Id,
                            pid = childShapes[j].ParentId,
                            name = childShapes[j].Name
                        };
                        shapeData.Add(shape);
                    }
            }

            ViewBag.dataSource = JavaScriptConvert.ToString(shapeData);
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/CustomCatalogIndex.cshtml", CatalogModel);
        }

        public class ShapeData
        {
            public int id { get; set; }
            public int? pid { get; set; }
            public string name { get; set; }
            public bool hasChild { get; set; }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public PartialViewResult FilteredProductsResult([FromBody] FilterParams filterParams)
        {
            var shapeIds = filterParams?.ShapeIds;
            var specIds = filterParams?.SpecIds;
            var searchKeyword = filterParams?.SearchKeyword;
            if (shapeIds == null)
                shapeIds = new List<int>();
            if (specIds == null)
                specIds = new List<int>();

            CatalogModel = _catalogModelFactory.PrepareSwiftCatalogModel(shapeIds, specIds, searchKeyword);
            CatalogModel.FilterParams = filterParams;

            return PartialView("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/_FilteredPartialView.cshtml", CatalogModel);

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