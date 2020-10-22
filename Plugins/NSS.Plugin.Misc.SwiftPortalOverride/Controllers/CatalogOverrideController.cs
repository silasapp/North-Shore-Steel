using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
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
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class CatalogOverrideController : Controller
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        #region Constructor
        public CatalogOverrideController(ICatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }
        #endregion

        public IActionResult Index()
        {
            var shapeIds = new List<int>();
            var specIds = new List<int>();

            CatalogModel = _catalogModelFactory.PrepareSwiftCatalogModel(shapeIds, specIds);

            var shapes = CatalogModel.PagingFilteringContext.ShapeFilter.Shapes;

            List<ShapeData> shapeData = new List<ShapeData>();
            for (var i = 0; i < shapes.Count; i++)
            {
                shapeData.Add(new ShapeData
                {
                    id = shapes[i].Id,
                    pid = shapes[i].ParentId,
                    name = shapes[i].Name
                });  
            }

            ViewBag.dataSource = shapeData;
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/CustomCatalogIndex.cshtml", CatalogModel);
        }

        public class ShapeData
        {
            public int id { get; set; }
            public int? pid { get; set; }
            public string name { get; set; }
        }

        [HttpPost]
        public PartialViewResult FilteredProductsResult([FromBody] FilterParams filterParams)
        {
            var shapeIds = filterParams.ShapeIds;
            var specIds = filterParams.SpecIds;
            if (shapeIds == null)
                shapeIds = new List<int>();
            if (specIds == null)
                specIds = new List<int>();

            if (shapeIds.Count > 0 || specIds.Count > 0)
                CatalogModel = _catalogModelFactory.PrepareSwiftCatalogModel(shapeIds, specIds);

            return PartialView("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/_FilteredPartialView.cshtml", CatalogModel);

        }

        public class FilterParams
        {
            public List<int> SpecIds { get; set; }
            public List<int> ShapeIds { get; set; }
        }

        public CatalogModel CatalogModel { get; set; }
    }
}