using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Blogs;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.News;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Themes;
using Nop.Services.Topics;
using Nop.Web.Factories;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework.UI;
using Nop.Web.Models.Common;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public class CustomCommonModelFactory : CommonModelFactory
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ForumSettings _forumSettings;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;

        #endregion

        #region Ctor

        public CustomCommonModelFactory(BlogSettings blogSettings, CaptchaSettings captchaSettings, CatalogSettings catalogSettings, CommonSettings commonSettings, CustomerSettings customerSettings, DisplayDefaultFooterItemSettings displayDefaultFooterItemSettings, ForumSettings forumSettings, IActionContextAccessor actionContextAccessor, IBlogService blogService, ICacheKeyService cacheKeyService, ICategoryService categoryService, ICurrencyService currencyService, ICustomerService customerService, IForumService forumService, IGenericAttributeService genericAttributeService, IHttpContextAccessor httpContextAccessor, ILanguageService languageService, ILocalizationService localizationService, IManufacturerService manufacturerService, INewsService newsService, INopFileProvider fileProvider, IPageHeadBuilder pageHeadBuilder, IPermissionService permissionService, IPictureService pictureService, IProductService productService, IProductTagService productTagService, IShoppingCartService shoppingCartService, ISitemapGenerator sitemapGenerator, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IThemeContext themeContext, IThemeProvider themeProvider, ITopicService topicService, IUrlHelperFactory urlHelperFactory, IUrlRecordService urlRecordService, IWebHelper webHelper, IWorkContext workContext, LocalizationSettings localizationSettings, MediaSettings mediaSettings, NewsSettings newsSettings, SitemapSettings sitemapSettings, SitemapXmlSettings sitemapXmlSettings, StoreInformationSettings storeInformationSettings, VendorSettings vendorSettings) : base(blogSettings, captchaSettings, catalogSettings, commonSettings, customerSettings, displayDefaultFooterItemSettings, forumSettings, actionContextAccessor, blogService, cacheKeyService, categoryService, currencyService, customerService, forumService, genericAttributeService, httpContextAccessor, languageService, localizationService, manufacturerService, newsService, fileProvider, pageHeadBuilder, permissionService, pictureService, productService, productTagService, shoppingCartService, sitemapGenerator, staticCacheManager, storeContext, themeContext, themeProvider, topicService, urlHelperFactory, urlRecordService, webHelper, workContext, localizationSettings, mediaSettings, newsSettings, sitemapSettings, sitemapXmlSettings, storeInformationSettings, vendorSettings)
        {
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _permissionService = permissionService;
            _forumSettings = forumSettings;
            _customerService = customerService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
        }

        #endregion


        public override HeaderLinksModel PrepareHeaderLinksModel()
        {
            var customer = _workContext.CurrentCustomer;

            var unreadMessageCount = GetUnreadPrivateMessages();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = string.Format(_localizationService.GetResource("PrivateMessages.TotalUnread"), unreadMessageCount);

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
                    !_genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.NotifiedAboutNewPrivateMessagesAttribute, _storeContext.CurrentStore.Id))
                {
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.NotifiedAboutNewPrivateMessagesAttribute, true, _storeContext.CurrentStore.Id);
                    alertMessage = string.Format(_localizationService.GetResource("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
                }
            }

            var model = new HeaderLinksModel
            {
                IsAuthenticated = _customerService.IsRegistered(customer),
                CustomerName = _customerService.IsRegistered(customer) ? _customerService.FormatUsername(customer) : string.Empty,
                ShoppingCartEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                WishlistEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                AllowPrivateMessages = _customerService.IsRegistered(customer) && _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage,
            };
            //performance optimization (use "HasShoppingCartItems" property)
            if (customer.HasShoppingCartItems)
            {
                model.ShoppingCartItems = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id)
                    .Count;

                model.WishlistItems = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id)
                    .Count;
            }

            return model;
        }
    }
}
