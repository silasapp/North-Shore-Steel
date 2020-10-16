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
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class CatalogOverrideController : CatalogController
    {
        #region Constructor
        public CatalogOverrideController(CatalogSettings catalogSettings, IAclService aclService, ICatalogModelFactory catalogModelFactory, ICategoryService categoryService, ICustomerActivityService customerActivityService, IGenericAttributeService genericAttributeService, ILocalizationService localizationService, IManufacturerService manufacturerService, IPermissionService permissionService, IProductModelFactory productModelFactory, IProductService productService, IProductTagService productTagService, IStoreContext storeContext, IStoreMappingService storeMappingService, IVendorService vendorService, IWebHelper webHelper, IWorkContext workContext, MediaSettings mediaSettings, VendorSettings vendorSettings) : base(catalogSettings, aclService, catalogModelFactory, categoryService, customerActivityService, genericAttributeService, localizationService, manufacturerService, permissionService, productModelFactory, productService, productTagService, storeContext, storeMappingService, vendorService, webHelper, workContext, mediaSettings, vendorSettings)
        {
        }
        #endregion

        public IActionResult Index()
        {
            #region dataSource
            List<object> treedata = new List<object>();
            treedata.Add(new
            {
                id = 1,
                pid = 0,
                name = "Australia",
                hasChild = true
            });
            treedata.Add(new
            {
                id = 2,
                pid = 1,
                name = "New South Wales",

            });
            treedata.Add(new
            {
                id = 3,
                pid = 1,
                name = "Victoria"
            });
            treedata.Add(new
            {
                id = 4,
                name = "Brazil",
                pid = 0,
                hasChild = true
            });
            treedata.Add(new
            {
                id = 5,
                pid = 4,
                name = "Paraná"
            });
            #endregion

            FilterableProductsModel model = new FilterableProductsModel();
            model.SpecificationFilters = new List<SpecificationFilter>();
            #region specData
            model.SpecificationFilters.Add(new SpecificationFilter
            {
                Id = 1,
                SpecTitle = "METALS",
                Name = "Alloy",
                ProductCount = 8
            });
            model.SpecificationFilters.Add(new SpecificationFilter
            {
                Id = 2,
                SpecTitle = "METALS",
                Name = "Aluminium",
                ProductCount = 109
            });
            model.SpecificationFilters.Add(new SpecificationFilter
            {
                Id = 3,
                SpecTitle = "METALS",
                Name = "Stainless Steel",
                ProductCount = 138
            });
            model.SpecificationFilters.Add(new SpecificationFilter
            {
                Id = 4,
                SpecTitle = "GRADES",
                Name = "A36",
                ProductCount = 38
            });
            model.SpecificationFilters.Add(new SpecificationFilter
            {
                Id = 5,
                SpecTitle = "GRADES",
                Name = "A514",
                ProductCount = 33
            });
            #endregion
            IEnumerable<string> uniqueSpecTitles = model.SpecificationFilters.Select(x => x.SpecTitle).Distinct();
            model.UniqueSpecTitles = uniqueSpecTitles;
            return PartialView("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/CustomCatalogIndex.cshtml", model);
        }
    }
}
