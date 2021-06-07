using System.Threading.Tasks;
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
using System.Linq;

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

        public CustomCommonModelFactory(BlogSettings blogSettings, CaptchaSettings captchaSettings, CatalogSettings catalogSettings, CommonSettings commonSettings, CustomerSettings customerSettings, DisplayDefaultFooterItemSettings displayDefaultFooterItemSettings, ForumSettings forumSettings, IActionContextAccessor actionContextAccessor, IBlogService blogService, ICategoryService categoryService, ICurrencyService currencyService, ICustomerService customerService, IForumService forumService, IGenericAttributeService genericAttributeService, IHttpContextAccessor httpContextAccessor, ILanguageService languageService, ILocalizationService localizationService, IManufacturerService manufacturerService, INewsService newsService, INopFileProvider fileProvider, IPageHeadBuilder pageHeadBuilder, IPermissionService permissionService, IPictureService pictureService, IProductService productService, IProductTagService productTagService, IShoppingCartService shoppingCartService, ISitemapGenerator sitemapGenerator, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IThemeContext themeContext, IThemeProvider themeProvider, ITopicService topicService, IUrlHelperFactory urlHelperFactory, IUrlRecordService urlRecordService, IWebHelper webHelper, IWorkContext workContext, LocalizationSettings localizationSettings, MediaSettings mediaSettings, NewsSettings newsSettings, SitemapSettings sitemapSettings, SitemapXmlSettings sitemapXmlSettings, StoreInformationSettings storeInformationSettings, VendorSettings vendorSettings) :
            base(blogSettings, captchaSettings, catalogSettings, commonSettings, customerSettings, displayDefaultFooterItemSettings, forumSettings, actionContextAccessor,
                blogService, categoryService, currencyService, customerService, forumService, genericAttributeService, httpContextAccessor, languageService,
                localizationService, manufacturerService, newsService, fileProvider, pageHeadBuilder, permissionService, pictureService, productService, productTagService,
                shoppingCartService, sitemapGenerator, staticCacheManager, storeContext, themeContext, themeProvider, topicService, urlHelperFactory, urlRecordService,
                webHelper, workContext, localizationSettings, mediaSettings, newsSettings, sitemapSettings, sitemapXmlSettings, storeInformationSettings, vendorSettings)
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


        public override async Task<HeaderLinksModel> PrepareHeaderLinksModelAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var unreadMessageCount = await GetUnreadPrivateMessagesAsync();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = string.Format(await _localizationService.GetResourceAsync("PrivateMessages.TotalUnread"), unreadMessageCount);

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
                    !await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.NotifiedAboutNewPrivateMessagesAttribute,(await _storeContext.GetCurrentStoreAsync()).Id))
                {
                   await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.NotifiedAboutNewPrivateMessagesAttribute, true,(await _storeContext.GetCurrentStoreAsync()).Id);
                    alertMessage = string.Format(await _localizationService.GetResourceAsync("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
                }
            }

            var model = new HeaderLinksModel
            {
                IsAuthenticated = await _customerService.IsRegisteredAsync(customer),
                CustomerName =await _customerService.IsRegisteredAsync(customer) ? await _customerService.FormatUsernameAsync(customer) : string.Empty,
                ShoppingCartEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart),
                WishlistEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist),
                AllowPrivateMessages = await _customerService.IsRegisteredAsync(customer) && _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage,
            };
            //performance optimization (use "HasShoppingCartItems" property)
            if (customer.HasShoppingCartItems)
            {

                model.ShoppingCartItems = (await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id))
                    .Sum(item => item.Quantity);

                model.WishlistItems = (await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id))
                    .Sum(item => item.Quantity);

            }

            return model;
        }
    }
}
