using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class CartOverrideController : ShoppingCartController
    {
        #region Constructor
        public CartOverrideController(CaptchaSettings captchaSettings, CustomerSettings customerSettings, ICacheKeyService cacheKeyService, ICheckoutAttributeParser checkoutAttributeParser, ICheckoutAttributeService checkoutAttributeService, ICurrencyService currencyService, ICustomerActivityService customerActivityService, ICustomerService customerService, IDiscountService discountService, IDownloadService downloadService, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, ILocalizationService localizationService, INopFileProvider fileProvider, INotificationService notificationService, IPermissionService permissionService, IPictureService pictureService, IPriceFormatter priceFormatter, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IProductService productService, IShippingService shippingService, IShoppingCartModelFactory shoppingCartModelFactory, IShoppingCartService shoppingCartService, IStaticCacheManager staticCacheManager, IStoreContext storeContext, ITaxService taxService, IUrlRecordService urlRecordService, IWebHelper webHelper, IWorkContext workContext, IWorkflowMessageService workflowMessageService, MediaSettings mediaSettings, OrderSettings orderSettings, ShoppingCartSettings shoppingCartSettings) : base(captchaSettings, customerSettings, cacheKeyService, checkoutAttributeParser, checkoutAttributeService, currencyService, customerActivityService, customerService, discountService, downloadService, genericAttributeService, giftCardService, localizationService, fileProvider, notificationService, permissionService, pictureService, priceFormatter, productAttributeParser, productAttributeService, productService, shippingService, shoppingCartModelFactory, shoppingCartService, staticCacheManager, storeContext, taxService, urlRecordService, webHelper, workContext, workflowMessageService, mediaSettings, orderSettings, shoppingCartSettings)
        {
        }


        #endregion

        public IActionResult Index()
        {
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomCatalog/Cart.cshtml");
        }

    }
}