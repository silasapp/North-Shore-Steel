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
using Nop.Web.Models.Common;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;

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
        private readonly ERPApiProvider _nSSApiProvider;
        private readonly IShapeService _shapeService;
        private readonly ICountryModelFactory _countryModelFactory;
        private readonly PayPalServiceManager _payPalServiceManager;
        private readonly SwiftCoreSettings _settings;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDiscountService _discountService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly ICustomerCompanyProductService _customerCompanyProductService;

        #endregion

        #region Ctor
        public CheckoutOverrideController(ICustomerCompanyProductService customerCompanyProductService, ICustomerCompanyService customerCompanyService, IOrderTotalCalculationService orderTotalCalculationService, IDiscountService discountService, ICheckoutAttributeParser checkoutAttributeParser, IStateProvinceService stateProvinceService,SwiftCoreSettings swiftCoreSettings, PayPalServiceManager payPalServiceManager, IShapeService shapeService, ERPApiProvider nSSApiProvider, AddressSettings addressSettings,
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
            _orderTotalCalculationService = orderTotalCalculationService;
            _customerCompanyService = customerCompanyService;
            _customerCompanyProductService = customerCompanyProductService;
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
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (!_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("Checkout");

            if (_customerService.IsGuest(_workContext.CurrentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            var shoppingCartModel = new ShoppingCartModel();

            CheckoutCompleteOverrideModel model = new CheckoutCompleteOverrideModel
            {
                BillingAddressModel = _checkoutModelFactory.PrepareOnePageCheckoutModel(cart),
                ShippingAddressModel = _checkoutModelFactory.PrepareShippingAddressModel(cart, prePopulateNewAddressWithCustomerFields: true),
                ShippingMethodModel = _checkoutModelFactory.PrepareShippingMethodModel(cart, _customerService.GetCustomerShippingAddress(_workContext.CurrentCustomer)),
                ConfirmModel = _checkoutModelFactory.PrepareConfirmOrderModel(cart),
                ShoppingCartModel = _shoppingCartModelFactory.PrepareShoppingCartModel(shoppingCartModel, cart),
                OrderTotals = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false)
            };

            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
            {
                filterByCountryId = _customerService.GetCustomerBillingAddress(_workContext.CurrentCustomer)?.CountryId ?? 0;
            }

            var usaCountryId = "1";
            model.StateProvinces = _countryModelFactory.GetStatesByCountryId(usaCountryId, false);

            // account credit
            var creditModel = new AccountCreditModel();
            var (erpCompId, customerCompany) = GetCustomerCompanyDetails();

            if (customerCompany != null && customerCompany.CanCredit)
            {
                var creditResult = _nSSApiProvider.GetCompanyCreditBalance(erpCompId, useMock: false);
                creditModel = new AccountCreditModel { CanCredit = true, CreditAmount = creditResult.CreditAmount ?? (decimal)0.00 };
            }

            model.AccountCreditModel = creditModel;

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
            //add erp order no
            model.CustomProperties.TryAdd(SwiftCore.Helpers.Constants.ErpOrderNoAttribute, _genericAttributeService.GetAttribute<long?>(order, SwiftCore.Helpers.Constants.ErpOrderNoAttribute));
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CheckoutOverride/Completed.cshtml", model);
        }

        public virtual IActionResult Rejected()
        {            
            return View();
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

                // get shipping cost
                // shipping
                ERPCalculateShippingRequest request = BuildShippingCostRequest(model);

                var shipObj = GetShippingCost(requestOverride: request);

                //try to create an order
                var (order, errorMessage) = _payPalServiceManager.CreateOrder(_settings, paymentRequest.OrderGuid, model, shipObj.ShippingCost);
                if (order != null)
                {
                    //save order details for future using
                    paymentRequest.CustomValues.Add(PaypalDefaults.PayPalOrderIdKey, order.Id);
                    paymentRequest.CustomValues.Add(PaypalDefaults.ShippingCostKey, shipObj.ShippingCost);
                    paymentRequest.CustomValues.Add(PaypalDefaults.ShippingDeliveryDateKey, shipObj.DeliveryDate);

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
                var result = ProcessPayment(model.PaymentMethodModel.CheckoutPaymentMethodType, model);

                return result;

                //return result;
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public JsonResult GetShippingRate([FromBody] ShippingCostRequest address)
        {
            ERPCalculateShippingResponse response = GetShippingCost(address);

            var shoppingCart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(shoppingCart, out var orderDiscountAmount, out var orderAppliedDiscounts, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount);
            orderTotal += response.ShippingCost;

            return Json(new
            {
                shippingCalculatorResponse = response,
                orderTotal,
                orderDiscountAmount,
                orderAppliedDiscounts,
                appliedGiftCards,
                redeemedRewardPoints,
                redeemedRewardPointsAmount
            });
        }

        private ERPCalculateShippingRequest BuildShippingCostRequest(ErpCheckoutModel model)
        {
            var shippingAddress = _addressService.GetAddressById(model.ShippingAddress.ShippingAddressId);
            var shippingAddressNew = model.ShippingAddress.ShippingNewAddress;

            var shipStateProvince = shippingAddress != null ? _stateProvinceService.GetStateProvinceByAddress(shippingAddress) : _stateProvinceService.GetStateProvinceById(shippingAddressNew.StateProvinceId ?? 0);

            var shippingAddress1 = shippingAddress != null ? shippingAddress.Address1 : shippingAddressNew.Address1;
            var shippingAddress2 = shippingAddress != null ? shippingAddress.Address2 : shippingAddressNew.Address2;
            var shippingCity = shippingAddress != null ? shippingAddress.City : shippingAddressNew.City;
            var shippingCountryId = shippingAddress != null ? shippingAddress.CountryId : shippingAddressNew.CountryId;
            var shippingZipPostalCode = shippingAddress != null ? shippingAddress.ZipPostalCode : shippingAddressNew.ZipPostalCode;
            var shippingPhoneNumber = shippingAddress != null ? shippingAddress.PhoneNumber : _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.PhoneAttribute);

            var request = new ERPCalculateShippingRequest
            {
                DeliveryMethod = model.ShippingAddress.IsPickupInStore ? "Pickup" : "Shipping",
                DestinationAddressLine1 = shippingAddress1,
                DestinationAddressLine2 = shippingAddress2,
                State = shipStateProvince?.Abbreviation,
                City = shippingCity,
                PostalCode = shippingZipPostalCode,
                PickupLocationId = model.ShippingAddress.IsPickupInStore ? (model.ShippingAddress.PickupPoint?.City?.ToLower() == "houston" ? 1 : (model.ShippingAddress.PickupPoint?.City?.ToLower() == "beaumont" ? 2 : 0)) : 0
            };
            return request;
        }

        private ERPCalculateShippingResponse GetShippingCost(ShippingCostRequest address = null, ERPCalculateShippingRequest requestOverride = null)
        {
            if (address == null && requestOverride == null)
                throw new ArgumentNullException(nameof(address));

            ERPCalculateShippingRequest request;

            if (requestOverride != null)
                request = requestOverride;
            else
                request = new ERPCalculateShippingRequest
                {
                    DeliveryMethod = address.IsPickup ? "pickup" : "shipping",
                    DestinationAddressLine1 = address.Address1,
                    DestinationAddressLine2 = address.Address2,
                    State = _stateProvinceService.GetStateProvinceById(address.StateProvinceId ?? 0)?.Abbreviation,
                    City = address.City,
                    PostalCode = address.ZipPostalCode,
                    PickupLocationId = address.IsPickup ? (address?.City?.ToLower() == "houston" ? 1 : (address?.City?.ToLower() == "beaumont" ? 2 : 0)) : 0
                };

            var orderItems = new List<Item>();
            request.OrderWeight = decimal.Zero;

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            foreach (var item in cart)
            {
                var attr = _genericAttributeService.GetAttributesForEntity(item.ProductId, nameof(Product));

                bool isNum = decimal.TryParse(attr.FirstOrDefault(x => x.Key == "weight")?.Value, out decimal weight);
                isNum = int.TryParse(attr.FirstOrDefault(x => x.Key == "shapeId")?.Value, out int shapeId);
                isNum = int.TryParse(attr.FirstOrDefault(x => x.Key == "itemId")?.Value, out int itemId);
                isNum = decimal.TryParse(attr.FirstOrDefault(x => x.Key == "length")?.Value, out decimal length);

                request.OrderWeight += (weight * item.Quantity);

                if (length > request.MaxLength)
                    request.MaxLength = length;

                var shape = _shapeService.GetShapeById(shapeId);

                orderItems.Add(new Item
                {
                    ItemId = itemId,
                    ShapeId = shapeId,
                    ShapeName = shape?.Name
                });
            }

            request.Items = JsonConvert.SerializeObject(orderItems.ToArray());

            var response = _nSSApiProvider.GetShippingRate(request, useMock: false);
            return response;
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

        private JsonResult ProcessPayment(int paymentMethodtype, ErpCheckoutModel model)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (!IsMinimumOrderPlacementIntervalValid(_workContext.CurrentCustomer))
                throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

            //place order
            var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
            if (processPaymentRequest == null)
            {
                processPaymentRequest = new ProcessPaymentRequest();

                // get shipping cost
                var request = BuildShippingCostRequest(model);
                var shipObj = GetShippingCost(requestOverride: request);

                processPaymentRequest.CustomValues.Add(PaypalDefaults.ShippingCostKey, shipObj.ShippingCost);
                processPaymentRequest.CustomValues.Add(PaypalDefaults.ShippingDeliveryDateKey, shipObj.DeliveryDate);
            }  

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

            _paymentService.GenerateOrderGuid(processPaymentRequest);
            processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
            processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
            processPaymentRequest.PaymentMethodSystemName = paymentMethod;
            HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);

            result = ProcessPaymentType(processPaymentRequest, paymentMethod);

            return result;
        }

        private JsonResult ProcessPaymentType(ProcessPaymentRequest processPaymentRequest, string paymentMethod)
        {
            //add custom values
            processPaymentRequest.CustomValues.Add(PaypalDefaults.PaymentMethodTypeKey, paymentMethod);
            if(paymentMethod == "CREDIT")
            {
                //nss get credit amount
                var (erpCompId, _) = GetCustomerCompanyDetails();
                var creditResult = _nSSApiProvider.GetCompanyCreditBalance(erpCompId, useMock: false);
                processPaymentRequest.CustomValues.Add(PaypalDefaults.CreditBalanceKey, creditResult.CreditAmount ?? (decimal)0.00);
            }

            var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);

            if (placeOrderResult.Success)
            {
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = placeOrderResult.PlacedOrder
                };

                // call nss place Order
                processPaymentRequest.CustomValues.TryGetValue(PaypalDefaults.ShippingDeliveryDateKey, out var deliveryDate);
                NSSPlaceOrderRequest(placeOrderResult.PlacedOrder, paymentMethod, deliveryDate?.ToString());

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

        private void NSSPlaceOrderRequest(Nop.Core.Domain.Orders.Order order, string paymentMethod, string deliveryDate)
        {
            try
            {
                var shippingAddress = _addressService.GetAddressById(order.ShippingAddressId ?? 0);
                var pickupAddress = _addressService.GetAddressById(order.PickupAddressId ?? 0);

                var chkAttr = _checkoutAttributeParser.ParseCheckoutAttributes(order.CheckoutAttributesXml);
                var poAttr = chkAttr.FirstOrDefault(x => x.Name == SwiftCore.Helpers.Constants.CheckoutPONoAttribute);
                var poValues = poAttr != null ? _checkoutAttributeParser.ParseValues(order.CheckoutAttributesXml, poAttr.Id) : new List<string>();

                var (erpCompId, customerCompany) = GetCustomerCompanyDetails();

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

                    orderItems.Add(new DTOs.Requests.OrderItem
                    {
                        Description = attrs.FirstOrDefault(x => x.Key == "itemName")?.Value,
                        ItemId = (int.TryParse(attrs.FirstOrDefault(x => x.Key == "itemId")?.Value, out var itemId) ? itemId : 0),
                        CustomerPartNo = customerCompany != null ? (_customerCompanyProductService.GetCustomerCompanyProductById(customerCompany.Id, item.ProductId)?.CustomerPartNo ?? null) : null,
                        Quantity = item.Quantity,
                        TotalPrice = item.PriceExclTax,
                        UnitPrice = item.UnitPriceExclTax,
                        TotalWeight = (decimal.TryParse(attrs.FirstOrDefault(x => x.Key == "weight")?.Value, out var weight) ? weight * item.Quantity : (decimal)0.00),
                        // product attr
                        Notes = "",
                        SawOptions = "",
                        SawTolerance = "",
                        Uom = "EA"
                    });
                }

                var request = new ERPCreateOrderRequest()
                {
                    OrderId = order.Id,
                    OrderTotal = order.OrderTotal,
                    PaymentMethodReferenceNo = order.AuthorizationTransactionId,
                    PaymentMethodType = paymentMethod,
                    ContactEmail = _workContext.CurrentCustomer.Email,
                    ContactFirstName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.FirstNameAttribute),
                    ContactLastName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.LastNameAttribute),
                    ContactPhone = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.PhoneAttribute),

                    ShippingAddressLine1 = shippingAddress?.Address1,
                    ShippingAddressLine2 = shippingAddress?.Address2,
                    ShippingAddressState = _stateProvinceService.GetStateProvinceById(shippingAddress?.StateProvinceId ?? 0)?.Abbreviation,
                    ShippingAddressCity = shippingAddress?.City,
                    ShippingAddressPostalCode = shippingAddress?.ZipPostalCode,

                    PickupInStore = order.PickupInStore,
                    PickupLocationId = pickupAddress?.City?.ToLower() == "houston" ? 1 : (pickupAddress?.City?.ToLower() == "beaumont" ? 2 : 0),
                    UserId = _genericAttributeService.GetAttribute<int>(_workContext.CurrentCustomer, SwiftCore.Helpers.Constants.ErpKeyAttribute),

                    PoNo = poValues.FirstOrDefault(),

                    DeliveryDate = deliveryDate ?? string.Empty,
                    ShippingTotal = order.OrderShippingExclTax,

                    TaxTotal = order.OrderTax,

                    // discounts
                    DiscountTotal = order.OrderDiscount,
                    Discounts = JsonConvert.SerializeObject(discounts.ToArray()),

                    // order items
                    LineItemTotal = order.OrderSubtotalExclTax,
                    OrderItems = JsonConvert.SerializeObject(orderItems.ToArray())
                };

                // api call
                var resp = _nSSApiProvider.CreateNSSOrder(erpCompId, request, useMock: false);

                if (resp.NSSOrderNo > 0)
                    _genericAttributeService.SaveAttribute<long>(order, SwiftCore.Helpers.Constants.ErpOrderNoAttribute, resp.NSSOrderNo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                // silent;
            }

        }

        private (int companyId, CustomerCompany customerCompany) GetCustomerCompanyDetails()
        {
            string erpCompIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int.TryParse(Request.Cookies[erpCompIdCookieKey], out int ERPCompanyId);

            var customerCompany = new CustomerCompany();

            if (ERPCompanyId > 0)
                customerCompany = _customerCompanyService.GetCustomerCompanyByErpCompId(_workContext.CurrentCustomer.Id, ERPCompanyId);

            return (ERPCompanyId, customerCompany);
        }


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