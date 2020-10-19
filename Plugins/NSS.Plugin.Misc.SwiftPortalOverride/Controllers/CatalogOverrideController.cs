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
            CatalogModel model = new CatalogModel();
            var shapeIds = new List<int>();
            var specIds = new List<int>();
            var res = _catalogModelFactory.PrepareSwiftCatalogModel(shapeIds, specIds);
            model.Products = res.Products;
            model.PagingFilteringContext.SpecificationFilter.NotFilteredItems = res.PagingFilteringContext.SpecificationFilter.NotFilteredItems;


            model.GroupedSpecificationAttributeName = model.PagingFilteringContext.SpecificationFilter.NotFilteredItems.GroupBy(sf => sf.SpecificationAttributeName);

            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/CustomCatalogIndex.cshtml", model);
        }

        // [HttpPost]
        //public async Task<PartialViewResult> FilteredProductsResult(List<int> SpecIds, List<int> ShapeIds)
        //{
        //    //var shapeIds = ShapeIds;
        //    //var specIds = SpecIds;
        //    //var x = _catalogModelFactory.PrepareSwiftCatalogModel(shapeIds, specIds);

        //    //x.Products = "Success Data";
        //    //return PartialView("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/_FilteredPartialView.cshtml", model);

        //}


        public IActionResult AddToCart()
        {
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/Cart.cshtml");
        }
    }
}