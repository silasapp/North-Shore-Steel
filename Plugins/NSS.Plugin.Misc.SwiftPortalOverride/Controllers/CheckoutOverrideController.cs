using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.ShoppingCart;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Http.Extensions;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using Nop.Services.Discounts;

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
        private readonly ICountryModelFactory _countryModelFactory;
        private readonly PayPalServiceManager _payPalServiceManager;
        private readonly SwiftCoreSettings _settings;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDiscountService _discountService;

        #endregion

        #region Ctor
        public CheckoutOverrideController(IDiscountService discountService, ICheckoutAttributeParser checkoutAttributeParser, IStateProvinceService stateProvinceService,SwiftCoreSettings swiftCoreSettings, PayPalServiceManager payPalServiceManager, IShapeService shapeService, NSSApiProvider nSSApiProvider, AddressSettings addressSettings,
            IShoppingCartModelFactory shoppingCartModelFactory, CustomerSettings customerSettings,
            IAddressAttributeParser addressAttributeParser, IAddressService addressService,
            ICheckoutModelFactory checkoutModelFactory, ICountryService countryService, ICustomerService customerService,
            IGenericAttributeService genericAttributeService, ILocalizationService localizationService, ILogger logger,
            IOrderProcessingService orderProcessingService, IOrderService orderService, IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService, IProductService productService, IShippingService shippingService,
            IShoppingCartService shoppingCartService, IStoreContext storeContext, IWebHelper webHelper,
            IWorkContext workContext, OrderSettings orderSettings, PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings, ICountryModelFactory countryModelFactory) : base(addressSettings, customerSettings, addressAttributeParser, addressService, checkoutModelFactory, countryService, customerService, genericAttributeService, localizationService, logger, orderProcessingService, orderService, paymentPluginManager, paymentService, productService, shippingService, shoppingCartService, storeContext, webHelper, workContext, orderSettings, paymentSettings, rewardPointsSettings, shippingSettings)
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
            _countryModelFactory = countryModelFactory;
            _payPalServiceManager = payPalServiceManager;
            _settings = swiftCoreSettings;
            _stateProvinceService = stateProvinceService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _discountService = discountService;
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
            model.OrderTotals = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false);

            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
            {
                filterByCountryId = _customerService.GetCustomerBillingAddress(_workContext.CurrentCustomer)?.CountryId ?? 0;
            }

            var usaCountryId = "1";
            model.StateProvinces = _countryModelFactory.GetStatesByCountryId(usaCountryId, false);

            // account credit
            // TODO add can credit in db
            //nss get credit amount
            var creditResult = _nSSApiProvider.GetCompanyCreditBalance(12345, useMock: true);
            model.AccountCreditModel = new AccountCreditModel { CanCredit = true, CreditAmount = creditResult.CreditAmount ?? (decimal)0.00 };

            (model.PaypalScript, _) = _payPalServiceManager.GetScript(_settings);

            //model
            model.PaymentMethodModel = _checkoutModelFactory.PreparePaymentMethodModel(cart, filterByCountryId);
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CheckoutOverride/Checkout.cshtml", model);
        }

        public override IActionResult Completed(int? orderId)
        {
            //validation
            if (_customerService.IsGuest(_workContext.CurrentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            Nop.Core.Domain.Orders.Order order = null;
            if (orderId.HasValue)
            {
                //load order by identifier (if provided)
                order = _orderService.GetOrderById(orderId.Value);
            }
            if (order == null)
            {
                order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                    .FirstOrDefault();
            }
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
            {
                return RedirectToRoute("Homepage");
            }

            //disable "order completed" page?
            //if (_orderSettings.DisableOrderCompletedPage)
            //{
            //    return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            //}

            var model = _checkoutModelFactory.PrepareCheckoutCompletedModel(order);
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CheckoutOverride/Completed.cshtml", model);
        }

        [IgnoreAntiforgeryToken]
        public JsonResult SwiftSaveShipping(CheckoutShippingAddressModel model, IFormCollection form)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(_localizationService.GetResource("Checkout.Disabled"));

                var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_customerService.IsGuest(_workContext.CurrentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!_shoppingCartService.ShoppingCartRequiresShipping(cart))
                    throw new Exception("Shipping is not required");

                //pickup point
                if (_shippingSettings.AllowPickupInStore && !_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
                {
                    var pickupInStore = ParsePickupInStore(form);
                    if (pickupInStore)
                    {
                        var pickupOption = ParsePickupOption(form);
                        SavePickupOption(pickupOption);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, null, _storeContext.CurrentStore.Id);
                }

                int.TryParse(form["shipping_address_id"], out var shippingAddressId);

                if (shippingAddressId > 0)
                {
                    //existing address
                    var address = _customerService.GetCustomerAddress(_workContext.CurrentCustomer.Id, shippingAddressId)
                        ?? throw new Exception(_localizationService.GetResource("Checkout.Address.NotFound"));

                    _workContext.CurrentCustomer.ShippingAddressId = address.Id;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                else
                {
                    //new address
                    var newAddress = model.ShippingNewAddress;

                    //custom address attributes
                    var customAttributes = _addressAttributeParser.ParseCustomAddressAttributes(form);
                    var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = _checkoutModelFactory.PrepareShippingAddressModel(cart,
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        shippingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            data = shippingAddressModel
                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress(_customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        _addressService.InsertAddress(address);

                        _customerService.InsertCustomerAddress(_workContext.CurrentCustomer, address);
                    }

                    _workContext.CurrentCustomer.ShippingAddressId = address.Id;

                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }

                model = _checkoutModelFactory.PrepareShippingAddressModel(cart, prePopulateNewAddressWithCustomerFields: true);

                return Json(new
                {
                    data = model
                });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual IActionResult SwiftSavePaymentMethod([FromBody] PaymentMethodRequest filterParams)
        {
            try
            {
                string paymentmethod = filterParams.PaymentMethod;
                CheckoutPaymentMethodModel model = filterParams.Model;
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(_localizationService.GetResource("Checkout.Disabled"));

                var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_customerService.IsGuest(_workContext.CurrentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                //payment method 
                if (string.IsNullOrEmpty(paymentmethod))
                    throw new Exception("Selected payment method can't be parsed");

                //reward points
                if (_rewardPointsSettings.Enabled)
                {
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, model.UseRewardPoints,
                        _storeContext.CurrentStore.Id);
                }

                //Check whether payment workflow is required
                var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart);
                if (!isPaymentWorkflowRequired)
                {
                    //payment is not required
                    _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, null, _storeContext.CurrentStore.Id);

                    var confirmOrderModel = _checkoutModelFactory.PrepareConfirmOrderModel(cart);
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

                var paymentMethodInst = _paymentPluginManager
                    .LoadPluginBySystemName(paymentmethod, _workContext.CurrentCustomer, _storeContext.CurrentStore.Id);
                if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                    throw new Exception("Selected payment method can't be parsed");

                //save
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentmethod, _storeContext.CurrentStore.Id);

                return OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual JsonResult CreatePayPalOrder([FromBody] ErpCheckoutModel model)
        {
            try
            {
                var result = Json(new { });
                //prepare order GUID
                var paymentRequest = new ProcessPaymentRequest();
                _paymentService.GenerateOrderGuid(paymentRequest);

                //try to create an order
                var (order, errorMessage) = _payPalServiceManager.CreateOrder(_settings, paymentRequest.OrderGuid, model);
                if (order != null)
                {
                    //save order details for future using
                    paymentRequest.CustomValues.Add(PaypalDefaults.PayPalOrderIdKey, order.Id);

                    result = Json(new { success = 1, orderId = order.Id });
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    result = Json(new { error = 1, message = errorMessage });
                }

                HttpContext.Session.Set("OrderPaymentInfo", paymentRequest);

                return result;
            }
            catch (Exception exc)
            {

                return Json(new { error = 1, message = exc.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual JsonResult PlaceOrder([FromBody] ErpCheckoutModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "Checkout Model is null");

            if (model.HasError)
                return Json(new { error = 1, message = "Something went wrong while placing order" });


            //validation
            if (_orderSettings.CheckoutDisabled)
                throw new Exception(_localizationService.GetResource("Checkout.Disabled"));

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (_customerService.IsGuest(_workContext.CurrentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            try
            {
                var saveShippingAddress = model.ShippingAddress.SaveToAddressBook;
                var savebillingAddress = model.BillingAddress.SaveToAddressBook;

                // save shipping address if asked to
                SaveShippingAddress(model.ShippingAddress);

                // save billing address if asked to
                if(!model.ShippingAddress.IsPickupInStore)
                    SaveBillingAddress(model.BillingAddress);

                // payment
                var result = ProcessPayment(model.PaymentMethodModel.CheckoutPaymentMethodType);

                return result;

                //return result;
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }


        private void SaveBillingAddress(ErpCheckoutBillingAddress model)
        {
            var SaveToAddressBook = model.SaveToAddressBook;
            var billingAddressId = model.BillingAddressId;

            if (model.ShipToSameAddress && _workContext.CurrentCustomer.ShippingAddressId.GetValueOrDefault() > 0)
            {
                //existing address
                var address = _customerService.GetCustomerAddress(_workContext.CurrentCustomer.Id, _workContext.CurrentCustomer.ShippingAddressId.GetValueOrDefault())
                    ?? throw new Exception(_localizationService.GetResource("Checkout.Address.NotFound"));

                _workContext.CurrentCustomer.BillingAddressId = address.Id;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }
            else
            {
                if (billingAddressId > 0)
                {
                    //existing address
                    var address = _customerService.GetCustomerAddress(_workContext.CurrentCustomer.Id, billingAddressId)
                        ?? throw new Exception(_localizationService.GetResource("Checkout.Address.NotFound"));

                    _workContext.CurrentCustomer.BillingAddressId = address.Id;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                else
                {
                    //new address
                    var customer = _workContext.CurrentCustomer;
                    //new address
                    var newAddress = model.BillingNewAddress;

                    // populate fields
                    newAddress.Email = customer.Email;
                    newAddress.FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                    newAddress.LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);
                    newAddress.PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);

                    string customAttributes = null;

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress(_customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        //address is not found. let's create a new one
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        //some validation
                        if (address.CountryId == 0)
                            address.CountryId = null;

                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;

                        _addressService.InsertAddress(address);

                        _customerService.InsertCustomerAddress(_workContext.CurrentCustomer, address);
                    }

                    _workContext.CurrentCustomer.BillingAddressId = address.Id;

                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
            }

        }

        private void SaveShippingAddress(ErpCheckoutShippingAddress model)
        {
            var pickUpInStore = model.IsPickupInStore;

            if (pickUpInStore)
            {
                var pickupPointsResponse = _shippingService.GetPickupPoints(_workContext.CurrentCustomer.BillingAddressId ?? 0,
                    _workContext.CurrentCustomer, storeId: _storeContext.CurrentStore.Id);

                if (pickupPointsResponse.Success)
                {
                    var pickupOption = pickupPointsResponse.PickupPoints.FirstOrDefault(x => x.Id == model.PickupPoint.Id);

                    SavePickupOption(pickupOption);
                }

            }
            else
            {
                var SaveToAddressBook = model.SaveToAddressBook;
                var addressId = model.ShippingAddressId;

                if (addressId > 0)
                {
                    // existing
                    //existing address
                    var address = _customerService.GetCustomerAddress(_workContext.CurrentCustomer.Id, addressId)
                        ?? throw new Exception(_localizationService.GetResource("Checkout.Address.NotFound"));

                    _workContext.CurrentCustomer.ShippingAddressId = address.Id;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                else
                {
                    var customer = _workContext.CurrentCustomer;
                    //new address
                    var newAddress = model.ShippingNewAddress;

                    // populate fields
                    newAddress.Email = customer.Email;
                    newAddress.FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                    newAddress.LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);
                    newAddress.PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);

                    //custom address attributes
                    string customAttributes = null;
                    // REMOVED

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress(_customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        _addressService.InsertAddress(address);

                        _customerService.InsertCustomerAddress(_workContext.CurrentCustomer, address);
                    }

                    _workContext.CurrentCustomer.ShippingAddressId = address.Id;

                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }

            }
        }


        private JsonResult ProcessPayment(int paymentMethodtype)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (!IsMinimumOrderPlacementIntervalValid(_workContext.CurrentCustomer))
                throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

            //place order
            var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
            if (processPaymentRequest == null)
                processPaymentRequest = new ProcessPaymentRequest();

            _paymentService.GenerateOrderGuid(processPaymentRequest);
            processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
            processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
            processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
            HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);

            // place order based on payment selected

            var checkoutPaymentMethod = (CheckoutPaymentMethodType)paymentMethodtype;
            string paymentMethod = "";
            var result = Json(new { error = 1, message = "payment method selected was not found" });

            switch (checkoutPaymentMethod)
            {
                case CheckoutPaymentMethodType.CreditCard:
                    paymentMethod = "CREDITCARD";
                    break;
                case CheckoutPaymentMethodType.Paypal:
                    paymentMethod = "PAYPAL";
                    break;
                case CheckoutPaymentMethodType.LineOfCredit:
                    paymentMethod = "CREDIT";
                    break;
                default:
                    break;
            }

            result = ProcessPaymentType(processPaymentRequest, paymentMethod);

            return result;
        }

        private JsonResult ProcessPaymentType(ProcessPaymentRequest processPaymentRequest, string paymentMethod)
        {
            //nss get credit amount
            var creditResult = _nSSApiProvider.GetCompanyCreditBalance(12345, useMock: true);

            //add custom values
            processPaymentRequest.CustomValues.Add(PaypalDefaults.PaymentMethodTypeKey, paymentMethod);
            if(paymentMethod == "CREDIT")
                processPaymentRequest.CustomValues.Add(PaypalDefaults.CreditBalanceKey, creditResult.CreditAmount ?? (decimal)0.00);

            var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);

            if (placeOrderResult.Success)
            {
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = placeOrderResult.PlacedOrder
                };

                // call nss place Order
                NSSPlaceOrderRequest(placeOrderResult.PlacedOrder, paymentMethod);

                if (paymentMethod == "CREDIT")
                {
                    placeOrderResult.PlacedOrder.PaymentStatus = PaymentStatus.Paid;
                    _orderService.UpdateOrder(placeOrderResult.PlacedOrder);

                    _orderProcessingService.CheckOrderStatus(placeOrderResult.PlacedOrder);
                }

                //success
                return Json(new { success = 1, orderId = placeOrderResult.PlacedOrder.Id });
            }

            // error
            return Json(new { error = 1, message = "Order was not placed. Line of credit payment error" });
        }

        private void NSSPlaceOrderRequest(Nop.Core.Domain.Orders.Order order, string paymentMethod)
        {
            try
            {
                var shippingAddress = _addressService.GetAddressById(order.ShippingAddressId ?? 0);
                var pickupAddress = _addressService.GetAddressById(order.PickupAddressId ?? 0);

                var chkAttr = _checkoutAttributeParser.ParseCheckoutAttributes(order.CheckoutAttributesXml);
                var poAttr = chkAttr.FirstOrDefault(x => x.Name == SwiftCore.Helpers.Constants.CheckoutPONoAttribute);
                var poValues = poAttr != null ? _checkoutAttributeParser.ParseValues(order.CheckoutAttributesXml, poAttr.Id) : new List<string>();

                // discounts
                var discounts = new List<Discount>();
                var discounUsagetList = _discountService.GetAllDiscountUsageHistory(customerId: _workContext.CurrentCustomer.Id, orderId: order.Id);
                foreach (var item in discounUsagetList)
                {
                    var discount = _discountService.GetDiscountById(item.Id);
                    if (discount != null)
                        discounts.Add(new Discount { Amount = discount.DiscountAmount, Code = discount.CouponCode, Description = discount.Name });
                }

                // order items
                var orderItemList = _orderService.GetOrderItems(order.Id);
                var orderItems = new List<DTOs.Requests.OrderItem>();
                foreach (var item in orderItemList)
                {
                    var attrs = _genericAttributeService.GetAttributesForEntity(item.ProductId, nameof(Product));

                    var json = JsonConvert.SerializeObject(new Dictionary<string, object>(attrs.Select(x => new KeyValuePair<string, object>(x.Key, x.Value))));

                    //var model = JsonConvert.DeserializeObject<ErpProductModel>(json);


                    orderItems.Add(new DTOs.Requests.OrderItem
                    {
                        Description = attrs.FirstOrDefault(x => x.Key == "itemName")?.Value,
                        ItemId = (int.TryParse(attrs.FirstOrDefault(x => x.Key == "itemId")?.Value, out var itemId) ? itemId: 0),
                        Quantity = item.Quantity,
                        TotalPrice = item.PriceExclTax,
                        UnitPrice = item.UnitPriceExclTax,
                        TotalWeight = (decimal.TryParse(attrs.FirstOrDefault(x => x.Key == "weight")?.Value, out var weight) ? weight * item.Quantity :  (decimal)0.00),
                        // product attr
                        Notes = "",
                        SawOptions = "",
                        SawTolerance = "",
                        Uom = ""
                    });
                }

                var request = new NSSCreateOrderRequest()
                {
                    OrderId = order.Id,
                    OrderTotal = order.OrderTotal,
                    PaymentMethodReferenceNo = order.AuthorizationTransactionId,
                    PaymentMethodType = paymentMethod,
                    ContactEmail = _workContext.CurrentCustomer.Email,
                    ContactFirstName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.FirstNameAttribute, _storeContext.CurrentStore.Id),
                    ContactLastName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.LastNameAttribute, _storeContext.CurrentStore.Id),
                    ContactPhone = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.PhoneAttribute, _storeContext.CurrentStore.Id),

                    ShippingAddressLine1 = shippingAddress?.Address1,
                    ShippingAddressLine2 = shippingAddress?.Address2,
                    ShippingAddressState = _stateProvinceService.GetStateProvinceById(shippingAddress?.StateProvinceId ?? 0)?.Abbreviation,
                    ShippingAddressCity = shippingAddress?.City,
                    ShippingAddressPostalCode = shippingAddress?.ZipPostalCode,

                    PickupInStore = order.PickupInStore,
                    PickupLocationId = pickupAddress?.City?.ToLower() == "houston" ? 1 : (pickupAddress?.City?.ToLower() == "beaumont" ? 2 : 0),
                    UserId = _genericAttributeService.GetAttribute<int>(_workContext.CurrentCustomer, SwiftCore.Helpers.Constants.WintrixKeyAttribute, _storeContext.CurrentStore.Id),

                    PoNo = poValues.FirstOrDefault(),

                    DeliveryDate = "",
                    ShippingTotal = order.OrderShippingExclTax,

                    TaxTotal = order.OrderTax,

                    // discounts
                    DiscountTotal = order.OrderDiscount,
                    Discounts = discounts.ToArray(),

                    // order items
                    LineItemTotal = order.OrderSubtotalExclTax,
                    OrderItems = orderItems.ToArray()
                };

                var resp = _nSSApiProvider.CreateNSSOrder(12345, request, useMock: true);

                if (resp.NSSOrderNo > 0)
                    _genericAttributeService.SaveAttribute<long>(order, "ErpOrderNo", resp.NSSOrderNo, _storeContext.CurrentStore.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                // silent;
            }

        }

        private JsonResult ProcessCreditCardPayment(ProcessPaymentRequest processPaymentRequest, string paymentMethod)
        {
            return Json(new { error = 1, message = "Debit/Credit card payment not supported yet." });
        }

        private JsonResult ProcePaypalPayment(ProcessPaymentRequest processPaymentRequest, string paymentMethod)
        {
            return Json(new { error = 1, message = "Paypal payment not supported yet." });
        }

        private void ErpConfirmOrder()
        {

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

        public static string GetJson(object data)
        {
            var contractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None
            });
        }
    }

    public class PaymentMethodRequest
    {
        public string PaymentMethod { get; set; }
        public CheckoutPaymentMethodModel Model { get; set; }
    }

    public class ErpMockOrder
    {
        public int OrderId { get; set; }
    }
}