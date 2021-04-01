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
using Nop.Core.Domain.Customers;

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

        public override IList<string> AddToCart(Nop.Core.Domain.Customers.Customer customer, 
            Product product, ShoppingCartType shoppingCartType, int storeId, string attributesXml = null, 
            decimal customerEnteredPrice = 0, DateTime? rentalStartDate = null, DateTime? rentalEndDate = null, 
            int quantity = 1, bool addRequiredProducts = true)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }

            if (shoppingCartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }

            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));
                return warnings;
            }

            //reset checkout info
            _customerService.ResetCheckoutData(customer, storeId);

            var cart = GetShoppingCart(customer, shoppingCartType, storeId);

            var shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                shoppingCartType, product, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                var newQuantity = quantity;
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                    storeId, attributesXml,
                    customerEnteredPrice, rentalStartDate, rentalEndDate,
                    newQuantity, addRequiredProducts, shoppingCartItem.Id));

                if (warnings.Any())
                    return warnings;

                //shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.Quantity = newQuantity;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                _sciRepository.Update(shoppingCartItem);

                //event notification
                _eventPublisher.EntityUpdated(shoppingCartItem);
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                    storeId, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate,
                    quantity, addRequiredProducts));

                if (warnings.Any())
                    return warnings;

                //maximum items validation
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                            return warnings;
                        }

                        break;
                    case ShoppingCartType.Wishlist:
                        if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                            return warnings;
                        }

                        break;
                    default:
                        break;
                }

                var now = DateTime.UtcNow;
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartType = shoppingCartType,
                    StoreId = storeId,
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    CustomerEnteredPrice = customerEnteredPrice,
                    Quantity = quantity,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate,
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    CustomerId = customer.Id
                };

                _sciRepository.Insert(shoppingCartItem);

                //updated "HasShoppingCartItems" property used for performance optimization
                customer.HasShoppingCartItems = !IsCustomerShoppingCartEmpty(customer);

                _customerService.UpdateCustomer(customer);

                //event notification
                _eventPublisher.EntityInserted(shoppingCartItem);
            }

            return warnings;
        }

        public override decimal GetUnitPrice(ShoppingCartItem shoppingCartItem, bool includeDiscounts, out decimal discountAmount, out List<Discount> appliedDiscounts)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            var customer = _customerService.GetCustomerById(shoppingCartItem.CustomerId);
            var product = _productService.GetProductById(shoppingCartItem.ProductId);

            // override product price based on selected/default unit price
            var(uom, pricePerFt, pricePerCWT, pricePerPiece) = GetPriceUnits(product, shoppingCartItem);
            product.Price = GetERPUnitPrice(uom, pricePerFt, pricePerCWT, pricePerPiece);

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

        public override decimal GetSubTotal(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts,
            out int? maximumDiscountQty)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            decimal subTotal;
            maximumDiscountQty = null;

            //unit price
            var unitPrice = GetUnitPrice(shoppingCartItem, includeDiscounts,
                out discountAmount, out appliedDiscounts);

            // custom get erp units
            var product = _productService.GetProductById(shoppingCartItem.ProductId);
            var (uom, _, _, _) = GetPriceUnits(product, shoppingCartItem);

            //discount
            if (appliedDiscounts.Any())
            {
                //we can properly use "MaximumDiscountedQuantity" property only for one discount (not cumulative ones)
                Discount oneAndOnlyDiscount = null;
                if (appliedDiscounts.Count == 1)
                    oneAndOnlyDiscount = appliedDiscounts.First();

                if ((oneAndOnlyDiscount?.MaximumDiscountedQuantity.HasValue ?? false) &&
                    shoppingCartItem.Quantity > oneAndOnlyDiscount.MaximumDiscountedQuantity.Value)
                {
                    maximumDiscountQty = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                    //we cannot apply discount for all shopping cart items
                    var discountedQuantity = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                    var discountedSubTotal = GetERPSubTotal(product, unitPrice, uom, discountedQuantity);
                    discountAmount *= discountedQuantity;

                    var notDiscountedQuantity = shoppingCartItem.Quantity - discountedQuantity;
                    var notDiscountedUnitPrice = GetUnitPrice(shoppingCartItem, false);
                    var notDiscountedSubTotal = notDiscountedUnitPrice * notDiscountedQuantity;

                    subTotal = discountedSubTotal + notDiscountedSubTotal;
                }
                else
                {
                    //discount is applied to all items (quantity)
                    //calculate discount amount for all items
                    discountAmount *= shoppingCartItem.Quantity;

                    subTotal = GetERPSubTotal(product, unitPrice, uom, shoppingCartItem.Quantity);
                }
            }
            else
            {
                subTotal = GetERPSubTotal(product, unitPrice, uom, shoppingCartItem.Quantity);
            }

            return subTotal;
        }

        private decimal GetERPUnitPrice(string uom, decimal pricePerFt, decimal pricePerCWT, decimal pricePerPiece)
        {
            decimal unitPrice = uom == Constants.UnitPerPieceField ? pricePerPiece : uom == Constants.UnitPerWeightField ? pricePerCWT : uom == Constants.UnitPerFtField ? pricePerFt : decimal.Zero;

            return unitPrice;
        }

        private decimal GetERPSubTotal(Product product, decimal unitPrice, string uom, int qty)
        {
            decimal subtotal;

            if (uom == Constants.UnitPerWeightField)
            {
                var totalWeight = Math.Round(product.Weight * qty, 0) / 100;
                subtotal = unitPrice * totalWeight;
            }
            else if (uom == Constants.UnitPerFtField)
            {
                var totalLength = Math.Round(product.Length * qty, 0);
                subtotal = unitPrice * totalLength;
            }   
            else
                subtotal = unitPrice * qty;

            return Math.Round(subtotal, 2);
        }

        private (string, decimal, decimal, decimal) GetPriceUnits(Product product, ShoppingCartItem item)
        {
            var attr = _genericAttributeService.GetAttributesForEntity(product.Id, nameof(Product));
            decimal.TryParse(attr.FirstOrDefault(x => x.Key == "pricePerFt")?.Value, out var pricePerFt);
            decimal.TryParse(attr.FirstOrDefault(x => x.Key == "pricePerCWT")?.Value, out var pricePerCWT);
            decimal.TryParse(attr.FirstOrDefault(x => x.Key == "pricePerPiece")?.Value, out var pricePerPiece);

            var attribute = _productAttributeService.GetAllProductAttributes()?.FirstOrDefault(x => x.Name == Constants.PurchaseUnitAttribute);
            var mappings = _productAttributeParser.ParseProductAttributeMappings(item.AttributesXml);
            var mapping = mappings.FirstOrDefault(x => x.ProductAttributeId == attribute?.Id);

            var uom = Constants.UnitPerPieceField;

            if(mapping != null)
                uom = _productAttributeParser.ParseProductAttributeValues(item.AttributesXml, mapping?.Id ?? 0)?.FirstOrDefault()?.Name;

            uom = uom == Constants.UnitPerPieceField && pricePerPiece == decimal.Zero ? pricePerCWT != decimal.Zero ? Constants.UnitPerWeightField : pricePerFt != decimal.Zero ? Constants.UnitPerFtField  : Constants.UnitPerPieceField : uom;

            return (uom, pricePerFt, pricePerCWT, pricePerPiece);
        }
    }
}
