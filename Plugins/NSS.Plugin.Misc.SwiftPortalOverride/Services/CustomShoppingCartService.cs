using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSS.Plugin.Misc.SwiftCore.Helpers;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    public partial class CustomShoppingCartService : ShoppingCartService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly IShippingService _shippingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        public CustomShoppingCartService(CatalogSettings catalogSettings, IAclService aclService, IActionContextAccessor actionContextAccessor, ICacheKeyService cacheKeyService, ICheckoutAttributeParser checkoutAttributeParser, ICheckoutAttributeService checkoutAttributeService, ICurrencyService currencyService, ICustomerService customerService, IDateRangeService dateRangeService, IDateTimeHelper dateTimeHelper, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, ILocalizationService localizationService, IPermissionService permissionService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IProductService productService, IRepository<ShoppingCartItem> sciRepository, IShippingService shippingService, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IStoreMappingService storeMappingService, IUrlHelperFactory urlHelperFactory, IUrlRecordService urlRecordService, IWorkContext workContext, OrderSettings orderSettings, ShoppingCartSettings shoppingCartSettings) : base(catalogSettings, aclService, actionContextAccessor, cacheKeyService, checkoutAttributeParser, checkoutAttributeService, currencyService, customerService, dateRangeService, dateTimeHelper, eventPublisher, genericAttributeService, localizationService, permissionService, priceCalculationService, priceFormatter, productAttributeParser, productAttributeService, productService, sciRepository, shippingService, staticCacheManager, storeContext, storeMappingService, urlHelperFactory, urlRecordService, workContext, orderSettings, shoppingCartSettings)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _actionContextAccessor = actionContextAccessor;
            _cacheKeyService = cacheKeyService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateRangeService = dateRangeService;
            _dateTimeHelper = dateTimeHelper;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _sciRepository = sciRepository;
            _shippingService = shippingService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _shoppingCartSettings = shoppingCartSettings;
        }
        #endregion

        public override decimal GetUnitPrice(ShoppingCartItem shoppingCartItem, bool includeDiscounts, out decimal discountAmount, out List<Discount> appliedDiscounts)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            var customer = _customerService.GetCustomerById(shoppingCartItem.CustomerId);
            var product = _productService.GetProductById(shoppingCartItem.ProductId);

            // override product price based on selected/default unit price
            var price = GetPrice(product);
            if (price != null)
                product.Price = price.GetValueOrDefault();

            return GetUnitPrice(product,
                customer,
                shoppingCartItem.ShoppingCartType,
                shoppingCartItem.Quantity,
                shoppingCartItem.AttributesXml,
                shoppingCartItem.CustomerEnteredPrice,
                shoppingCartItem.RentalStartDateUtc,
                shoppingCartItem.RentalEndDateUtc,
                includeDiscounts,
                out discountAmount,
                out appliedDiscounts);
        }

        private decimal? GetPrice(Product product)
        {
            var attr = _genericAttributeService.GetAttributesForEntity(product.Id, nameof(Product));
            var pricePerFt = attr.FirstOrDefault(x => x.Key == "pricePerFt")?.Value;
            var pricePerCWT = attr.FirstOrDefault(x => x.Key == "pricePerCWT")?.Value;
            var pricePerPiece = attr.FirstOrDefault(x => x.Key == "pricePerPiece")?.Value;
            var length = attr.FirstOrDefault(x => x.Key == "length")?.Value;
            var weight = attr.FirstOrDefault(x => x.Key == "weight")?.Value;

            string price = !string.IsNullOrEmpty(pricePerPiece) ? pricePerPiece : !string.IsNullOrEmpty(pricePerCWT) ? pricePerCWT : !string.IsNullOrEmpty(pricePerFt) ? pricePerFt : "0.00";

            string defaultUnit = !string.IsNullOrEmpty(pricePerPiece) ? Constants.UnitPerPieceField : !string.IsNullOrEmpty(pricePerCWT) ? Constants.UnitPerWeightField : !string.IsNullOrEmpty(pricePerFt) ? Constants.UnitPerFtField : "0.00";

            var isnum = decimal.TryParse(price, out decimal priceDecimal);
            decimal weightDecimal = (decimal)0.00;
            decimal lengthDecimal = (decimal)0.00;
            isnum = isnum && decimal.TryParse(length, out lengthDecimal);
            isnum = isnum && decimal.TryParse(weight, out weightDecimal);
            decimal? unitPrice = null;

            if (isnum)
            {
                if (defaultUnit == Constants.UnitPerWeightField)
                    unitPrice = (weightDecimal / 100) * priceDecimal;
                else if (defaultUnit == Constants.UnitPerFtField)
                    unitPrice = lengthDecimal * unitPrice;
                else
                    unitPrice = priceDecimal;
            }

            return unitPrice;
        }
    }
}
