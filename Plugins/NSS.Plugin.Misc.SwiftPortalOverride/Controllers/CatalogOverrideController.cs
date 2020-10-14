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
using System;
using System.Collections.Generic;
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
            return Ok();

        }
    }
}
