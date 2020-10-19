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
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class CatalogOverrideController:Controller
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
            var shapeIds = new List<int> { 1 };
            var specIds = new List<int> ();
            //var x = _catalogModelFactory.PrepareSwiftCatalogModel(shapeIds, specIds);


            CatalogPagingFilteringModel.SpecificationFilterModel model = new CatalogPagingFilteringModel.SpecificationFilterModel();
            model.NotFilteredItems.Add(new CatalogPagingFilteringModel.SpecificationFilterItem
            {
                SpecificationAttributeOptionId = 1,
                SpecificationAttributeName = "METALS",
                SpecificationAttributeOptionName = "Alloy"
            });
            model.NotFilteredItems.Add(new CatalogPagingFilteringModel.SpecificationFilterItem
            {
                SpecificationAttributeOptionId = 2,
                SpecificationAttributeName = "METALS",
                SpecificationAttributeOptionName = "Aluminium"
            });
            model.NotFilteredItems.Add(new CatalogPagingFilteringModel.SpecificationFilterItem
            {
                SpecificationAttributeOptionId = 3,
                SpecificationAttributeName = "METALS",
                SpecificationAttributeOptionName = "Stainless Steel"
            });
            model.NotFilteredItems.Add(new CatalogPagingFilteringModel.SpecificationFilterItem
            {
                SpecificationAttributeOptionId = 4,
                SpecificationAttributeName = "GRADES",
                SpecificationAttributeOptionName = "A36"
            });
            model.NotFilteredItems.Add(new CatalogPagingFilteringModel.SpecificationFilterItem
            {
                SpecificationAttributeOptionId = 5,
                SpecificationAttributeName = "GRADES",
                SpecificationAttributeOptionName = "A514"
            });


            model.GroupedSpecificationAttributeName = model.NotFilteredItems.GroupBy(sf => sf.SpecificationAttributeName);

            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/CustomCatalogIndex.cshtml", model);
        }

        [HttpPost]
        //public async Task<PartialViewResult> FilteredProductsResult(List<int> SpecIds, List<int> ShapeIds)
        //{
        //    //var shapeIds = ShapeIds;
        //    //var specIds = SpecIds;
        //    //var x = _catalogModelFactory.PrepareSwiftCatalogModel(shapeIds, specIds);

        //    //x.Products = "Success Data";
        //    //return PartialView("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/_FilteredPartialView.cshtml", model);

        //}
    }
}
