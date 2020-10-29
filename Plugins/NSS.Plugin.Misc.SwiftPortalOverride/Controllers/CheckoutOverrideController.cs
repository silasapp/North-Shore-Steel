using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Models.ShoppingCart;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using System.Collections.Generic;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class CheckoutOverrideController : CheckoutController
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly NSSApiProvider _nSSApiProvider;
        private readonly IShapeService _shapeService;

        #endregion

        #region Ctor
        public CheckoutOverrideController(IShapeService shapeService, NSSApiProvider nSSApiProvider, AddressSettings addressSettings, IShoppingCartModelFactory shoppingCartModelFactory, CustomerSettings customerSettings, IAddressAttributeParser addressAttributeParser, IAddressService addressService, ICheckoutModelFactory checkoutModelFactory, ICountryService countryService, ICustomerService customerService, IGenericAttributeService genericAttributeService, ILocalizationService localizationService, ILogger logger, IOrderProcessingService orderProcessingService, IOrderService orderService, IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IProductService productService, IShippingService shippingService, IShoppingCartService shoppingCartService, IStoreContext storeContext, IWebHelper webHelper, IWorkContext workContext, OrderSettings orderSettings, PaymentSettings paymentSettings, RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings) : base(addressSettings, customerSettings, addressAttributeParser, addressService, checkoutModelFactory, countryService, customerService, genericAttributeService, localizationService, logger, orderProcessingService, orderService, paymentPluginManager, paymentService, productService, shippingService, shoppingCartService, storeContext, webHelper, workContext, orderSettings, paymentSettings, rewardPointsSettings, shippingSettings)
        {
            _addressSettings = addressSettings;
            _customerSettings = customerSettings;
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _checkoutModelFactory = checkoutModelFactory;
            _countryService = countryService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _productService = productService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _nSSApiProvider = nSSApiProvider;
            _shapeService = shapeService;
        }
        #endregion


        #region Methods
        public override IActionResult Index()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
            var downloadableProductsRequireRegistration =
                _customerSettings.RequireRegistrationForDownloadableProducts && _productService.HasAnyDownloadableProduct(cartProductIds);

            if (_customerService.IsGuest(_workContext.CurrentCustomer) && (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration))
                return Challenge();

            //if we have only "button" payment methods available (displayed onthe shopping cart page, not during checkout),
            //then we should allow standard checkout
            //all payment methods (do not filter by country here as it could be not specified yet)
            var paymentMethods = _paymentPluginManager
                .LoadActivePlugins(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id)
                .Where(pm => !pm.HidePaymentMethod(cart)).ToList();
            //payment methods displayed during checkout (not with "Button" type)
            var nonButtonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
                .ToList();
            //"button" payment methods(*displayed on the shopping cart page)
            var buttonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            if (!nonButtonPaymentMethods.Any() && buttonPaymentMethods.Any())
                return RedirectToRoute("ShoppingCart");

            //reset checkout data
            _customerService.ResetCheckoutData(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

            //validation (cart)
            var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                NopCustomerDefaults.CheckoutAttributes, _storeContext.CurrentStore.Id);
            var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            if (scWarnings.Any())
                return RedirectToRoute("ShoppingCart");
            //validation (each shopping cart item)
            foreach (var sci in cart)
            {
                var product = _productService.GetProductById(sci.ProductId);

                var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                    sci.ShoppingCartType,
                    product,
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc,
                    sci.RentalEndDateUtc,
                    sci.Quantity,
                    false,
                    sci.Id);
                if (sciWarnings.Any())
                    return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            return RedirectToRoute("CheckoutBillingAddress");
        }

        public override IActionResult OnePageCheckout()
        {
            var shoppingCartModel = new ShoppingCartModel();
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);


            CheckoutCompleteOverrideModel model = new CheckoutCompleteOverrideModel();

            model.BillingAddressModel = _checkoutModelFactory.PrepareOnePageCheckoutModel(cart);
            model.ShippingAddressModel = _checkoutModelFactory.PrepareShippingAddressModel(cart, prePopulateNewAddressWithCustomerFields: true);
            model.ShippingMethodModel = _checkoutModelFactory.PrepareShippingMethodModel(cart, _customerService.GetCustomerShippingAddress(_workContext.CurrentCustomer));
            model.ConfirmModel = _checkoutModelFactory.PrepareConfirmOrderModel(cart);
            model.ShoppingCartModel = _shoppingCartModelFactory.PrepareShoppingCartModel(shoppingCartModel, cart);
            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
            {
                filterByCountryId = _customerService.GetCustomerBillingAddress(_workContext.CurrentCustomer)?.CountryId ?? 0;
            }

            //model
            model.PaymentMethodModel = _checkoutModelFactory.PreparePaymentMethodModel(cart, filterByCountryId);
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CheckoutOverride/Checkout.cshtml", model);
        }

        //[HttpPost]
        //[IgnoreAntiforgeryToken]
        //public IActionResult GetShippingRate(NSSCalculateShippingRequest requestParam)
        //{
        //    var request = new NSSCalculateShippingRequest();
        //    var orderItems = new List<Item>();
        //    request.OrderWeight = decimal.Zero;

        //    var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

        //    if (_workContext.CurrentCustomer.ShippingAddressId != null)
        //    {
        //        var address = _addressService.GetAddressById(_workContext.CurrentCustomer.ShippingAddressId.Value);
        //        _workContext.CurrentCustomer.

        //        foreach (var item in cart)
        //            {
        //                var attr = _genericAttributeService.GetAttributesForEntity(item.ProductId, nameof(Product));

        //                bool isNum = decimal.TryParse(attr.FirstOrDefault(x => x.Key == "weight")?.Value, out decimal weight);
        //                isNum = int.TryParse(attr.FirstOrDefault(x => x.Key == "shapeId")?.Value, out int shapeId);
        //                isNum = int.TryParse(attr.FirstOrDefault(x => x.Key == "itemId")?.Value, out int itemId);
        //                isNum = decimal.TryParse(attr.FirstOrDefault(x => x.Key == "length")?.Value, out decimal length);

        //                request.OrderWeight += (weight * item.Quantity);

        //                if (length > request.MaxLength)
        //                    request.MaxLength = length;

        //                var shape = _shapeService.GetShapeById(shapeId);

        //                orderItems.Add(new Item
        //                {
        //                    ItemId = itemId,
        //                    ShapeId = shapeId,
        //                    ShapeName = shape?.Name
        //                });
        //            }

        //        request.Items = orderItems.ToArray();

        //        var response = _nSSApiProvider.GetShippingRate(requestParam, true);

        //        return Json(response);
        //    }

        //}

        #endregion
    }
}