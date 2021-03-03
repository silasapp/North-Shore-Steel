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
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore;
using System.Diagnostics;

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
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly Factories.ICheckoutModelFactory _overrideCheckoutModelFactory;
        private readonly ICompanyService _companyService;

        #endregion

        #region Ctor
        public CheckoutOverrideController(
            ICompanyService companyService,
            Factories.ICheckoutModelFactory overrideCheckoutModelFactory,
            IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, ICustomerCompanyProductService customerCompanyProductService, ICustomerCompanyService customerCompanyService, IOrderTotalCalculationService orderTotalCalculationService, IDiscountService discountService, ICheckoutAttributeParser checkoutAttributeParser, IStateProvinceService stateProvinceService, SwiftCoreSettings swiftCoreSettings, PayPalServiceManager payPalServiceManager, IShapeService shapeService, ERPApiProvider nSSApiProvider, AddressSettings addressSettings,
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
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _overrideCheckoutModelFactory = overrideCheckoutModelFactory;
            _companyService = companyService;
        }
        #endregion


        #region Methods
        public override IActionResult Index()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

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
                product.OrderMaximumQuantity = product.OrderMaximumQuantity > 0 ? product.OrderMaximumQuantity : 0;

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
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

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
                BillingAddressModel = _overrideCheckoutModelFactory.PrepareOnePageCheckoutModel(cart),
                ShippingAddressModel = _overrideCheckoutModelFactory.PrepareShippingAddressModel(cart, prePopulateNewAddressWithCustomerFields: true),
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
            var companyInfo = _nSSApiProvider.GetCompanyInfo(eRPCompanyId);

            if (customerCompany != null && companyInfo != null && companyInfo.HasCredit && customerCompany.CanCredit)
            {
                var creditResult = _nSSApiProvider.GetCompanyCreditBalance(erpCompId, useMock: false);
                creditModel = new AccountCreditModel { CanCredit = true, CreditAmount = creditResult?.CreditAmount ?? decimal.Zero };

                // save credit bal, cached purposes
                _genericAttributeService.SaveAttribute<decimal?>(_workContext.CurrentCustomer, PaypalDefaults.CreditBalanceKey, creditModel.CreditAmount, _storeContext.CurrentStore.Id);
            }

            model.AccountCreditModel = creditModel;

            (model.PaypalScript, _) = _payPalServiceManager.GetScript(_settings);

            //model
            model.PaymentMethodModel = _checkoutModelFactory.PreparePaymentMethodModel(cart, filterByCountryId);
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CheckoutOverride/Checkout.cshtml", model);
        }

        public override IActionResult Completed(int? orderId)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

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

            var model = _checkoutModelFactory.PrepareCheckoutCompletedModel(order);
            //add erp order no
            model.CustomProperties.TryAdd(SwiftCore.Helpers.Constants.ErpOrderNoAttribute, _genericAttributeService.GetAttribute<long?>(order, SwiftCore.Helpers.Constants.ErpOrderNoAttribute));
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CheckoutOverride/Completed.cshtml", model);
        }

        public virtual IActionResult Rejected()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            return View();
        }


        [IgnoreAntiforgeryToken]
        public virtual JsonResult CreatePayPalOrder([FromBody] ErpCheckoutModel model)
        {
            try
            {
                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"start create paypal order process time stamp - {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}");
                var sw1 = new Stopwatch();
                sw1.Start();

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
                    paymentRequest.CustomValues.Add(PaypalDefaults.ShippingDeliveryDateKey, model.DeliveryDate);

                    result = Json(new { success = 1, orderId = order.Id });
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    result = Json(new { error = 1, message = errorMessage });
                }

                HttpContext.Session.Set("OrderPaymentInfo", paymentRequest);
                sw1.Stop();

                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"emd create paypal order process time stamp - {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")} | time elapsed - {sw1.ElapsedMilliseconds} ms");

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
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"start place order process time stamp - {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}");
            var sw1 = new Stopwatch();
            sw1.Start();

            if (model == null)
                throw new ArgumentNullException(nameof(model), "Checkout Model is null");

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
                // save shipping
                if(!model.ShippingAddress.IsPickupInStore)
                    _workContext.CurrentCustomer.ShippingAddressId = model.ShippingAddress.ShippingAddressId;

                // save billing
                if (model.BillingAddress.BillingAddressId > 0)
                {
                    _workContext.CurrentCustomer.BillingAddressId = model.BillingAddress.BillingAddressId;
                }
                else if(!model.ShippingAddress.IsPickupInStore)
                {
                    _workContext.CurrentCustomer.BillingAddressId = model.ShippingAddress.ShippingAddressId;
                }

                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                // payment
                var result = ProcessPayment(model.PaymentMethodModel.CheckoutPaymentMethodType, model);

                sw1.Stop();

                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"place order after paypal took {sw1.ElapsedMilliseconds} ms");

                return result;

                //return result;
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }


        [IgnoreAntiforgeryToken]
        public JsonResult SaveNewAddress([FromBody] NewAddress nAddress)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));
            bool isExist = true;
            var customer = _workContext.CurrentCustomer;
            //new address
            AddressModel newAddress = new AddressModel();

            // populate fields
            newAddress.Email = customer.Email;
            newAddress.FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute);
            newAddress.LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);
            newAddress.PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
            newAddress.Address1 = nAddress.Address1;
            newAddress.Address2 = nAddress.Address2;
            newAddress.City = nAddress.City;
            newAddress.StateProvinceId = nAddress.StateProvinceId;
            newAddress.ZipPostalCode = nAddress.ZipPostalCode;
            newAddress.CountryId = 1;

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
                isExist = false;
                address = newAddress.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;

                _addressService.InsertAddress(address);
                _customerService.InsertCustomerAddress(_workContext.CurrentCustomer, address);

                var company = _companyService.GetCompanyEntityByErpEntityId(eRPCompanyId);
                var companyAddress = string.Format(SwiftPortalOverrideDefaults.CompanyAddressKey, address.Id);
                _genericAttributeService.SaveAttribute<int>(company, companyAddress, address.Id);
            }


            var _ = nAddress.AddressType == "billing" ? _workContext.CurrentCustomer.BillingAddressId = address.Id : _workContext.CurrentCustomer.ShippingAddressId = address.Id;

            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            var contractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            var savedAddress = JsonConvert.SerializeObject(address, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None
            });

            return Json(new { isExist, savedAddress });
        }

        [IgnoreAntiforgeryToken]
        public JsonResult GetShippingRate([FromBody] ShippingCostRequest address)
        {
            try
            {
                ERPCalculateShippingResponse response = GetShippingCost(address);

                if (!response.Allowed)
                    response.ShippingCost = decimal.Zero;

                var shoppingCart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                //if pickup
                if (address.IsPickup)
                {
                    var pickupPoints = _shippingService.GetPickupPoints(_workContext.CurrentCustomer.BillingAddressId ?? 0, _workContext.CurrentCustomer, storeId: _storeContext.CurrentStore.Id).PickupPoints.ToList();
                    var pickupPoint = string.IsNullOrEmpty(address.PickupPointId) ? pickupPoints.FirstOrDefault() : pickupPoints.FirstOrDefault(x => x.Id == address.PickupPointId);

                    SavePickupOption(pickupPoint);
                }
                else
                {
                    // update shipping address
                    if (address.ExistingAddressId.HasValue && address.ExistingAddressId.Value > 0)
                    {
                        _workContext.CurrentCustomer.ShippingAddressId = address.ExistingAddressId;
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                    }

                    //Get shipping
                    var shippingOptions = _shippingService.GetShippingOptions(shoppingCart, _customerService.GetCustomerShippingAddress(_workContext.CurrentCustomer),
                        _workContext.CurrentCustomer, "Shipping.NSS", _storeContext.CurrentStore.Id).ShippingOptions.ToList();

                    var shippingOption = shippingOptions.FirstOrDefault();

                    //save
                    if (shippingOption != null)
                        _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, _storeContext.CurrentStore.Id);

                    _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, null, _storeContext.CurrentStore.Id);
                }

                var orderTotals = _shoppingCartModelFactory.PrepareOrderTotalsModel(shoppingCart, false);

                return Json(new
                {
                    shippingCalculatorResponse = response,
                    orderTotal = orderTotals.OrderTotal,
                    shipping = orderTotals.Shipping,
                    tax = orderTotals.Tax,
                });
            }
            catch (Exception exc)
            {
                return Json(new { error = 2, message = exc.Message });
            }

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
                PickupLocationId = model.ShippingAddress.IsPickupInStore ? (model.ShippingAddress.PickupPoint?.City?.ToLower() == "houston" ? 2 : (model.ShippingAddress.PickupPoint?.City?.ToLower() == "beaumont" ? 1 : 0)) : 0
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
                    DeliveryMethod = address.IsPickup ? "Pickup" : "Shipping",
                    DestinationAddressLine1 = address.Address1,
                    DestinationAddressLine2 = address.Address2,
                    State = _stateProvinceService.GetStateProvinceById(address.StateProvinceId ?? 0)?.Abbreviation,
                    City = address.City,
                    PostalCode = address.ZipPostalCode,
                    PickupLocationId = address.IsPickup ? (address?.City?.ToLower() == "houston" ? 2 : (address?.City?.ToLower() == "beaumont" ? 1 : 0)) : 0
                };

            var orderItems = new List<Item>();
            request.OrderWeight = 0;

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            foreach (var item in cart)
            {
                var attr = _genericAttributeService.GetAttributesForEntity(item.ProductId, nameof(Product));

                bool isNum = decimal.TryParse(attr.FirstOrDefault(x => x.Key == "weight")?.Value, out decimal weight);
                isNum = int.TryParse(attr.FirstOrDefault(x => x.Key == "shapeId")?.Value, out int shapeId);
                isNum = int.TryParse(attr.FirstOrDefault(x => x.Key == "itemId")?.Value, out int itemId);
                isNum = decimal.TryParse(attr.FirstOrDefault(x => x.Key == "lengthFt")?.Value, out decimal length);

                request.OrderWeight += (int)Math.Round(weight * item.Quantity, 0);

                if (length > request.MaxLength)
                    request.MaxLength = (int)Math.Round(length);

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

                processPaymentRequest.CustomValues.Add(PaypalDefaults.ShippingDeliveryDateKey, model.DeliveryDate);
            }

            // place order based on payment selected

            var checkoutPaymentMethod = (CheckoutPaymentMethodType)paymentMethodtype;
            string paymentMethod = "";

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

            var result = ProcessPaymentType(processPaymentRequest, paymentMethod);

            return result;
        }

        private JsonResult ProcessPaymentType(ProcessPaymentRequest processPaymentRequest, string paymentMethod)
        {
            //add custom values
            processPaymentRequest.CustomValues.Add(PaypalDefaults.PaymentMethodTypeKey, paymentMethod);
            if (paymentMethod == "CREDIT")
            {
                //nss get credit amount
                var (erpCompId, _) = GetCustomerCompanyDetails();
                // get credit balance from cache
                var creditAmount = _genericAttributeService.GetAttribute<decimal>(_workContext.CurrentCustomer, PaypalDefaults.CreditBalanceKey, _storeContext.CurrentStore.Id, decimal.Zero);
                processPaymentRequest.CustomValues.Add(PaypalDefaults.CreditBalanceKey, creditAmount);
            }

            var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);

            if (placeOrderResult.Success)
            {
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = placeOrderResult.PlacedOrder
                };

                // reset credit balance
                _genericAttributeService.SaveAttribute<decimal?>(_workContext.CurrentCustomer, PaypalDefaults.CreditBalanceKey, null, _storeContext.CurrentStore.Id);

                // call nss place Order
                processPaymentRequest.CustomValues.TryGetValue(PaypalDefaults.ShippingDeliveryDateKey, out var deliveryDate);

                try
                {
                    var sw1 = new Stopwatch();
                    sw1.Start();
                    NSSPlaceOrderRequest(placeOrderResult.PlacedOrder, paymentMethod, deliveryDate?.ToString());
                    sw1.Stop();

                    _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"Total NSS place order time {sw1.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);

                    return Json(new { error = 3, message = "Order was not placed successfully" });
                }


                if (paymentMethod == "CREDIT")
                {
                    placeOrderResult.PlacedOrder.PaymentStatus = PaymentStatus.Paid;
                    _orderService.UpdateOrder(placeOrderResult.PlacedOrder);

                    _orderProcessingService.CheckOrderStatus(placeOrderResult.PlacedOrder);
                }

                //success
                return Json(new { success = 1, orderId = placeOrderResult.PlacedOrder.Id });
            }

            return Json(new { error = 1, message = "Order was not placed successfully" });
        }

        private void NSSPlaceOrderRequest(Nop.Core.Domain.Orders.Order order, string paymentMethod, string deliveryDate)
        {
            var sw1 = new Stopwatch();
            sw1.Start();

            var shippingAddress = _addressService.GetAddressById(order.ShippingAddressId ?? 0);
            var pickupAddress = _addressService.GetAddressById(order.PickupAddressId ?? 0);

            var chkAttr = _checkoutAttributeParser.ParseCheckoutAttributes(order.CheckoutAttributesXml);
            var poAttr = chkAttr.FirstOrDefault(x => x.Name == SwiftCore.Helpers.Constants.CheckoutPONoAttribute);
            var poValues = poAttr != null ? _checkoutAttributeParser.ParseValues(order.CheckoutAttributesXml, poAttr.Id) : new List<string>();

            var (erpCompId, customerCompany) = GetCustomerCompanyDetails();

            // discounts
            var discounts = new List<Discount>();
            var discounUsagetList = _discountService.GetAllDiscountUsageHistory(orderId: order.Id);
            foreach (var item in discounUsagetList)
            {
                var discount = _discountService.GetDiscountById(item.DiscountId);
                if (discount != null)
                {
                    decimal amount = decimal.Zero;
                    switch (discount.DiscountType)
                    {
                        case Nop.Core.Domain.Discounts.DiscountType.AssignedToOrderTotal:
                            amount = order.OrderSubtotalInclTax;
                            break;
                        case Nop.Core.Domain.Discounts.DiscountType.AssignedToShipping:
                            amount = order.OrderShippingExclTax;
                            break;
                        case Nop.Core.Domain.Discounts.DiscountType.AssignedToOrderSubTotal:
                            amount = order.OrderSubtotalExclTax;
                            break;
                        default:
                            amount = order.OrderTotal;
                            break;
                    }

                    discounts.Add(new Discount { Amount = Math.Round(_discountService.GetDiscountAmount(discount, amount), 2), Code = discount.CouponCode ?? string.Empty, Description = discount.Name ?? string.Empty });
                }
            }

            // order items
            var orderItemList = _orderService.GetOrderItems(order.Id);
            var orderItems = new List<DTOs.Requests.OrderItem>();
            foreach (var item in orderItemList)
            {
                var genAttrs = _genericAttributeService.GetAttributesForEntity(item.ProductId, nameof(Product));

                var mappings = _productAttributeParser.ParseProductAttributeMappings(item.AttributesXml);
                var attrs = _productAttributeService.GetAllProductAttributes();

                string uom = null, notes = null, sawoptions = null, sawTolerance = null;

                // build prod attr
                foreach (var mapping in mappings)
                {
                    if (mapping != null)
                    {
                        var noteAttr = attrs.FirstOrDefault(x => x.Name == Constants.WorkOrderInstructionsAttribute);
                        var sawOptionAttr = attrs.FirstOrDefault(x => x.Name == Constants.CutOptionsAttribute);
                        var sawToleranceAttr = attrs.FirstOrDefault(x => x.Name == Constants.LengthToleranceCutAttribute);
                        var uomAttr = attrs.FirstOrDefault(x => x.Name == Constants.PurchaseUnitAttribute);

                        if (mapping.ProductAttributeId == noteAttr?.Id)
                        {
                            notes = _productAttributeParser.ParseValues(item.AttributesXml, mapping.Id)?.FirstOrDefault();
                        }
                        else if (mapping.ProductAttributeId == sawOptionAttr?.Id)
                        {
                            sawoptions = _productAttributeParser.ParseProductAttributeValues(item.AttributesXml, mapping.Id)?.FirstOrDefault()?.Name;
                        }
                        else if (mapping.ProductAttributeId == sawToleranceAttr?.Id)
                        {
                            sawTolerance = _productAttributeParser.ParseValues(item.AttributesXml, mapping.Id)?.FirstOrDefault();
                        }
                        else if (mapping.ProductAttributeId == uomAttr?.Id)
                        {
                            uom = _productAttributeParser.ParseProductAttributeValues(item.AttributesXml, mapping.Id)?.FirstOrDefault()?.Name ?? SwiftCore.Helpers.Constants.UnitPerPieceField;
                        }
                    }
                }

                orderItems.Add(new DTOs.Requests.OrderItem
                {
                    Description = genAttrs.FirstOrDefault(x => x.Key == "itemName")?.Value,
                    ItemId = (int.TryParse(genAttrs.FirstOrDefault(x => x.Key == "itemId")?.Value, out var itemId) ? itemId : 0),
                    CustomerPartNo = customerCompany != null ? (_customerCompanyProductService.GetCustomerCompanyProductById(customerCompany.Id, item.ProductId)?.CustomerPartNo ?? string.Empty) : string.Empty,
                    Quantity = item.Quantity,
                    TotalPrice = Math.Round(item.PriceExclTax, 2),
                    UnitPrice = Math.Round(item.UnitPriceExclTax, 2),
                    TotalWeight = (decimal.TryParse(genAttrs.FirstOrDefault(x => x.Key == "weight")?.Value, out var weight) ? (int)Math.Round(weight * item.Quantity) : 0),
                    // product attr
                    Notes = notes ?? string.Empty,
                    SawOptions = sawoptions?? string.Empty,
                    SawTolerance = sawTolerance ?? string.Empty,
                    Uom = uom ?? string.Empty
                });
            }

            var request = new ERPCreateOrderRequest()
            {
                ContactEmail = _workContext.CurrentCustomer.Email,
                ContactFirstName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.FirstNameAttribute),
                ContactLastName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.LastNameAttribute),
                ContactPhone = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.PhoneAttribute),
                UserId = _genericAttributeService.GetAttribute<int>(_workContext.CurrentCustomer, SwiftCore.Helpers.Constants.ErpKeyAttribute),

                ShippingAddressLine1 = shippingAddress?.Address1,
                ShippingAddressLine2 = shippingAddress?.Address2,
                ShippingAddressState = _stateProvinceService.GetStateProvinceById(shippingAddress?.StateProvinceId ?? 0)?.Abbreviation,
                ShippingAddressCity = shippingAddress?.City,
                ShippingAddressPostalCode = shippingAddress?.ZipPostalCode,
                PickupInStore = order.PickupInStore,
                PickupLocationId = order.PickupInStore && pickupAddress?.City?.ToLower() == "houston" ? 2 : order.PickupInStore && pickupAddress?.City?.ToLower() == "beaumont" ? 1 : (order.PickupInStore ? 1 : 0),

                DeliveryDate = deliveryDate ?? string.Empty,
                ShippingTotal = Math.Round(order.OrderShippingExclTax, 2),

                TaxTotal = Math.Round(order.OrderTax, 2),

                // discounts
                DiscountTotal = Math.Round(order.OrderDiscount, 2),
                Discounts = JsonConvert.SerializeObject(discounts.ToArray()),

                // order items
                LineItemTotal = Math.Round(order.OrderSubtotalExclTax, 2),
                OrderItems = JsonConvert.SerializeObject(orderItems.ToArray()),

                OrderId = order.Id,
                OrderTotal = Math.Round(order.OrderTotal, 2),
                PaymentMethodReferenceNo = order.AuthorizationTransactionId,
                PaymentMethodType = paymentMethod,
                PoNo = poValues.FirstOrDefault()
            };

            sw1.Stop();

            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"build order request to call nss api time - {sw1.ElapsedMilliseconds} ms");

            var sw2 = new Stopwatch();
            sw2.Start();
            // api call
            var resp = _nSSApiProvider.CreateNSSOrder(erpCompId, request, useMock: false);
            sw2.Stop();
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"nss api time elapsed - {sw2.ElapsedMilliseconds} ms");

            if (resp.NSSOrderNo > 0)
                _genericAttributeService.SaveAttribute<long>(order, SwiftCore.Helpers.Constants.ErpOrderNoAttribute, resp.NSSOrderNo);

        }

        private (int companyId, CustomerCompany customerCompany) GetCustomerCompanyDetails()
        {
            CustomerCompany customerCompany = null;

            string erpCompIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int ERPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, erpCompIdCookieKey));

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