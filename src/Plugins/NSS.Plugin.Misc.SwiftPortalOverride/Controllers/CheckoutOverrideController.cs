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
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Http.Extensions;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using Nop.Services.Discounts;
using Nop.Web.Models.Common;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore;
using System.Diagnostics;
using NSS.Plugin.Misc.SwiftCore.DTOs;
using System.Threading.Tasks;

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
        private readonly IApiService _apiService;

        #endregion

        #region Ctor
        public CheckoutOverrideController(
            IApiService apiService,
            ICompanyService companyService,
            Factories.ICheckoutModelFactory overrideCheckoutModelFactory,
            IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, ICustomerCompanyProductService customerCompanyProductService, ICustomerCompanyService customerCompanyService, IOrderTotalCalculationService orderTotalCalculationService, IDiscountService discountService, ICheckoutAttributeParser checkoutAttributeParser, IStateProvinceService stateProvinceService, SwiftCoreSettings swiftCoreSettings, PayPalServiceManager payPalServiceManager, IShapeService shapeService, AddressSettings addressSettings,
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
            _apiService = apiService;
        }
        #endregion


        #region Methods


        #region Page Actions

        public override async Task<IActionResult> OnePageCheckout()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey,(await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            if (!await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (!_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("Checkout");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            var shoppingCartModel = new ShoppingCartModel();

            CheckoutCompleteOverrideModel model = new CheckoutCompleteOverrideModel
            {
                BillingAddressModel = await _overrideCheckoutModelFactory.PrepareOnePageCheckoutModelAsync(cart),
                ShippingAddressModel = await _overrideCheckoutModelFactory.PrepareShippingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true),
                ShippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync())),
                ConfirmModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart),
                ShoppingCartModel = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(shoppingCartModel, cart),
                OrderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, false)
            };

            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
            {
                filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync()))?.CountryId ?? 0;
            }

            var usaCountryId = "1";
            model.StateProvinces = await _countryModelFactory.GetStatesByCountryIdAsync(usaCountryId, false);

            // account credit
            var creditModel = new AccountCreditModel();
            var (erpCompId, customerCompany) = await GetCustomerCompanyDetails();
            var companyInfo = await _apiService.GetCompanyInfoAsync(eRPCompanyId.ToString());

            if (customerCompany != null && companyInfo != null && companyInfo.HasCredit && customerCompany.CanCredit)
            {
                var creditResult = await _apiService.GetCompanyCreditBalanceAsync(erpCompId);
                creditModel = new AccountCreditModel { CanCredit = true, CreditAmount = creditResult?.CreditAmount ?? decimal.Zero };

                // save credit bal, cached purposes
                await _genericAttributeService.SaveAttributeAsync<decimal?>(await _workContext.GetCurrentCustomerAsync(), PaypalDefaults.CreditBalanceKey, creditModel.CreditAmount,(await _storeContext.GetCurrentStoreAsync()).Id);
            }

            model.AccountCreditModel = creditModel;

            (model.PaypalScript, _) = await _payPalServiceManager.GetScriptAsync(_settings);

            //model
            model.PaymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CheckoutOverride/Checkout.cshtml", model);
        }

        public override async Task<IActionResult> Completed(int? orderId)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            if (!await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            //validation
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            Nop.Core.Domain.Orders.Order order = null;
            if (orderId.HasValue)
            {
                //load order by identifier (if provided)
                order = await _orderService.GetOrderByIdAsync(orderId.Value);
            }
            if (order == null)
            {
                
                order = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
                    .FirstOrDefault();
            }
            if (order == null || order.Deleted ||( await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
            {
                return RedirectToRoute("Homepage");
            }

            var model = await _checkoutModelFactory.PrepareCheckoutCompletedModelAsync(order);
            //add erp order no
            model.CustomProperties.TryAdd(SwiftCore.Helpers.Constants.ErpOrderNoAttribute,await _genericAttributeService.GetAttributeAsync<long?>(order, SwiftCore.Helpers.Constants.ErpOrderNoAttribute));
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CheckoutOverride/Completed.cshtml", model);
        }

        public virtual async Task<IActionResult> Rejected()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            if (!await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            return View();
        }

        #endregion


        #region Ajax Actions

        [IgnoreAntiforgeryToken]
        public virtual async Task<JsonResult> CreatePayPalOrder([FromBody] ErpCheckoutModel model)
        {
            try
            {
                var result = Json(new { });
                //prepare order GUID
                var paymentRequest = new ProcessPaymentRequest();
                _paymentService.GenerateOrderGuid(paymentRequest);

                //try to create an order
                var (order, errorMessage) = await _payPalServiceManager.CreateOrderAsync(_settings, paymentRequest.OrderGuid, model);
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

                return result;
            }
            catch (Exception exc)
            {
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<JsonResult> PlaceOrder([FromBody] ErpCheckoutModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "Checkout Model is null");

            //validation
            if (_orderSettings.CheckoutDisabled)
                throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            try
            {
                // save shipping
                if (!model.ShippingAddress.IsPickupInStore)
                   ( await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = model.ShippingAddress.ShippingAddressId;

                // save billing
                if (model.BillingAddress.BillingAddressId > 0)
                {
                  ( await _workContext.GetCurrentCustomerAsync()).BillingAddressId = model.BillingAddress.BillingAddressId;
                }
                else if (!model.ShippingAddress.IsPickupInStore)
                {
                  (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = model.ShippingAddress.ShippingAddressId;
                }

               await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

                // payment
                var result = await ProcessPayment(model.PaymentMethodModel.CheckoutPaymentMethodType, model);

                return result;

                //return result;
            }
            catch (Exception exc)
            {
               await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }


        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> SaveNewAddress([FromBody] NewAddress nAddress)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey,(await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));
            bool isExist = true;
            var customer = await _workContext.GetCurrentCustomerAsync();
            //new address
            AddressModel newAddress = new AddressModel
            {

                // populate fields
                Email = customer.Email,
                FirstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                LastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute),
                PhoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute),
                Address1 = nAddress.Address1,
                Address2 = nAddress.Address2,
                City = nAddress.City,
                StateProvinceId = nAddress.StateProvinceId,
                ZipPostalCode = nAddress.ZipPostalCode,
                CountryId = 1
            };

            //custom address attributes
            string customAttributes = null;

            //try to find an address with the same values (don't duplicate records)
            var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id)).ToList(),
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

               await  _addressService.InsertAddressAsync(address);
                await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(), address);

                var company = await _companyService.GetCompanyEntityByErpEntityIdAsync(eRPCompanyId);
                var companyAddress = string.Format(SwiftPortalOverrideDefaults.CompanyAddressKey, address.Id);
               await _genericAttributeService.SaveAttributeAsync<int>(company, companyAddress, address.Id);
            }


            var _ = nAddress.AddressType == "billing" ? (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = address.Id : (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = address.Id;

            await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

            var contractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            var savedAddress = JsonConvert.SerializeObject(address, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None
            });

            return Json(new { isExist, savedAddress });
        }

        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> GetShippingRate([FromBody] ShippingCostRequest address)
        {
            try
            {
                ERPCalculateShippingResponse response = await GetShippingCost(address);

                if (!response.Allowed)
                    response.ShippingCost = decimal.Zero;

                var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                //if pickup
                if (address.IsPickup)
                {
                    var pickupPoints = (await _shippingService.GetPickupPointsAsync((await _workContext.GetCurrentCustomerAsync()).BillingAddressId ?? 0, await _workContext.GetCurrentCustomerAsync(), storeId:(await _storeContext.GetCurrentStoreAsync()).Id)).PickupPoints.ToList();
                    var pickupPoint = string.IsNullOrEmpty(address.PickupPointId) ? pickupPoints.FirstOrDefault() : pickupPoints.FirstOrDefault(x => x.Id == address.PickupPointId);

                   await SavePickupOptionAsync(pickupPoint);
                }
                else
                {
                    // update shipping address
                    if (address.ExistingAddressId.HasValue && address.ExistingAddressId.Value > 0)
                    {
                       (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = address.ExistingAddressId;
                       await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                    }

                    //Get shipping
                    var shippingOptions = (await _shippingService.GetShippingOptionsAsync(shoppingCart, await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()),
                       await _workContext.GetCurrentCustomerAsync(), "Shipping.NSS",(await _storeContext.GetCurrentStoreAsync()).Id)).ShippingOptions.ToList();

                    var shippingOption = shippingOptions.FirstOrDefault();

                    //save
                    if (shippingOption != null)
                       await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption,(await _storeContext.GetCurrentStoreAsync()).Id);

                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                }

                var orderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(shoppingCart, false);

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

        #endregion


        #region Util Methods

        private async Task<ERPCalculateShippingRequest> BuildShippingCostRequest(ErpCheckoutModel model)
        {
            var shippingAddress = await _addressService.GetAddressByIdAsync(model.ShippingAddress.ShippingAddressId);
            var shippingAddressNew = model.ShippingAddress.ShippingNewAddress;

            var shipStateProvince = shippingAddress != null ? await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) : await _stateProvinceService.GetStateProvinceByIdAsync(shippingAddressNew.StateProvinceId ?? 0);

            var shippingAddress1 = shippingAddress != null ? shippingAddress.Address1 : shippingAddressNew.Address1;
            var shippingAddress2 = shippingAddress != null ? shippingAddress.Address2 : shippingAddressNew.Address2;
            var shippingCity = shippingAddress != null ? shippingAddress.City : shippingAddressNew.City;
            var shippingCountryId = shippingAddress != null ? shippingAddress.CountryId : shippingAddressNew.CountryId;
            var shippingZipPostalCode = shippingAddress != null ? shippingAddress.ZipPostalCode : shippingAddressNew.ZipPostalCode;
            var shippingPhoneNumber = shippingAddress != null ? shippingAddress.PhoneNumber : await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.PhoneAttribute);

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

        private async Task<ERPCalculateShippingResponse> GetShippingCost(ShippingCostRequest address = null, ERPCalculateShippingRequest requestOverride = null)
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
                    State = (await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0))?.Abbreviation,
                    City = address.City,
                    PostalCode = address.ZipPostalCode,
                    PickupLocationId = address.IsPickup ? (address?.City?.ToLower() == "houston" ? 2 : (address?.City?.ToLower() == "beaumont" ? 1 : 0)) : 0
                };

            var orderItems = new List<Item>();
            request.OrderWeight = 0;

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart,(await _storeContext.GetCurrentStoreAsync()).Id);

            foreach (var item in cart)
            {
                var attr =await _genericAttributeService.GetAttributesForEntityAsync(item.ProductId, nameof(Product));

                bool isNum = decimal.TryParse(attr.FirstOrDefault(x => x.Key == "weight")?.Value, out decimal weight);
                isNum = int.TryParse(attr.FirstOrDefault(x => x.Key == "shapeId")?.Value, out int shapeId);
                isNum = int.TryParse(attr.FirstOrDefault(x => x.Key == "itemId")?.Value, out int itemId);
                isNum = decimal.TryParse(attr.FirstOrDefault(x => x.Key == "lengthFt")?.Value, out decimal length);

                request.OrderWeight += (int)Math.Round(weight * item.Quantity, 0);

                if (length > request.MaxLength)
                    request.MaxLength = (int)Math.Round(length);

                var shape = await _shapeService.GetShapeByIdAsync(shapeId);

                orderItems.Add(new Item
                {
                    ItemId = itemId,
                    ShapeId = shapeId,
                    ShapeName = shape?.Name
                });
            }

            request.Items = JsonConvert.SerializeObject(orderItems.ToArray());

            var (_, response) = await _apiService.GetShippingRateAsync(request);
            return response;
        }

        private async Task<JsonResult> ProcessPayment(int paymentMethodtype, ErpCheckoutModel model)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (!await IsMinimumOrderPlacementIntervalValidAsync(await _workContext.GetCurrentCustomerAsync()))
                throw new Exception(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

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
            processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
            processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            processPaymentRequest.PaymentMethodSystemName = paymentMethod;
            HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);

            var result = await ProcessPaymentType(processPaymentRequest, paymentMethod);

            return result;
        }

        private async Task<JsonResult> ProcessPaymentType(ProcessPaymentRequest processPaymentRequest, string paymentMethod)
        {
            //add custom values
            processPaymentRequest.CustomValues.Add(PaypalDefaults.PaymentMethodTypeKey, paymentMethod);
            if (paymentMethod == "CREDIT")
            {
                //nss get credit amount
                var (erpCompId, _) = await GetCustomerCompanyDetails();
                // get credit balance from cache
                var creditAmount = await _genericAttributeService.GetAttributeAsync<decimal>(await _workContext.GetCurrentCustomerAsync(), PaypalDefaults.CreditBalanceKey, (await _storeContext.GetCurrentStoreAsync()).Id, decimal.Zero);
                processPaymentRequest.CustomValues.Add(PaypalDefaults.CreditBalanceKey, creditAmount);
            }

            var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);

            if (placeOrderResult.Success)
            {
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = placeOrderResult.PlacedOrder
                };

                // reset credit balance
                await _genericAttributeService.SaveAttributeAsync<decimal?>(await _workContext.GetCurrentCustomerAsync(), PaypalDefaults.CreditBalanceKey, null, (await _storeContext.GetCurrentStoreAsync()).Id);

                // call nss place Order
                processPaymentRequest.CustomValues.TryGetValue(PaypalDefaults.ShippingDeliveryDateKey, out var deliveryDate);

                try
                {
                  await  NSSPlaceOrderRequest(placeOrderResult.PlacedOrder, paymentMethod, deliveryDate?.ToString());
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());

                    return Json(new { error = 3, message = "Order was not placed successfully" });
                }


                if (paymentMethod == "CREDIT")
                {
                    placeOrderResult.PlacedOrder.PaymentStatus = PaymentStatus.Paid;
                    await _orderService.UpdateOrderAsync(placeOrderResult.PlacedOrder);

                    await _orderProcessingService.CheckOrderStatusAsync(placeOrderResult.PlacedOrder);
                }

                //success
                return Json(new { success = 1, orderId = placeOrderResult.PlacedOrder.Id });
            }

            return Json(new { error = 1, message = "Order was not placed successfully" });
        }

        private async Task NSSPlaceOrderRequest(Nop.Core.Domain.Orders.Order order, string paymentMethod, string deliveryDate)
        {
            var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);
            var pickupAddress = await _addressService.GetAddressByIdAsync(order.PickupAddressId ?? 0);

            var chkAttr = await _checkoutAttributeParser.ParseCheckoutAttributesAsync(order.CheckoutAttributesXml);
            var poAttr = chkAttr.FirstOrDefault(x => x.Name == SwiftCore.Helpers.Constants.CheckoutPONoAttribute);
            var poValues = poAttr != null ? _checkoutAttributeParser.ParseValues(order.CheckoutAttributesXml, poAttr.Id) : new List<string>();

            var (erpCompId, customerCompany) = await GetCustomerCompanyDetails();

            // discounts
            var discounts = new List<Discount>();
            var discounUsagetList = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);
            foreach (var item in discounUsagetList)
            {
                var discount = await _discountService.GetDiscountByIdAsync(item.DiscountId);
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
            var orderItemList = await _orderService.GetOrderItemsAsync(order.Id);
            var orderItems = new List<SwiftCore.DTOs.OrderItem>();
            foreach (var item in orderItemList)
            {
                var genAttrs = await _genericAttributeService.GetAttributesForEntityAsync(item.ProductId, nameof(Product));

                var mappings = await _productAttributeParser.ParseProductAttributeMappingsAsync(item.AttributesXml);
                var attrs = await _productAttributeService.GetAllProductAttributesAsync();

                string uom = null, notes = null, sawoptions = null, sawTolerance = null, workOrderInstructions = null;

                // build prod attr
                foreach (var mapping in mappings)
                {
                    if (mapping != null)
                    {
                        var workOrderAttr = attrs.FirstOrDefault(x => x.Name == Constants.WorkOrderInstructionsAttribute);
                        var noteAttr = attrs.FirstOrDefault(x => x.Name == Constants.NotesAttribute);
                        var sawOptionAttr = attrs.FirstOrDefault(x => x.Name == Constants.CutOptionsAttribute);
                        var sawToleranceAttr = attrs.FirstOrDefault(x => x.Name == Constants.LengthToleranceCutAttribute);
                        var uomAttr = attrs.FirstOrDefault(x => x.Name == Constants.PurchaseUnitAttribute);

                        if (mapping.ProductAttributeId == noteAttr?.Id)
                        {
                            notes = _productAttributeParser.ParseValues(item.AttributesXml, mapping.Id)?.FirstOrDefault();
                        }
                        else if (mapping.ProductAttributeId == sawOptionAttr?.Id)
                        {
                            sawoptions = (await _productAttributeParser.ParseProductAttributeValuesAsync(item.AttributesXml, mapping.Id))?.FirstOrDefault()?.Name;
                        }
                        else if (mapping.ProductAttributeId == workOrderAttr?.Id)
                        {
                            workOrderInstructions = _productAttributeParser.ParseValues(item.AttributesXml, mapping.Id)?.FirstOrDefault();
                        }
                        else if (mapping.ProductAttributeId == uomAttr?.Id)
                        {
                            uom = (await _productAttributeParser.ParseProductAttributeValuesAsync(item.AttributesXml, mapping.Id))?.FirstOrDefault()?.Name ?? Constants.UnitPerPieceField;
                        }
                    }
                }

                orderItems.Add(new SwiftCore.DTOs.OrderItem
                {
                    Description = genAttrs.FirstOrDefault(x => x.Key == "itemName")?.Value,
                    ItemId = (int.TryParse(genAttrs.FirstOrDefault(x => x.Key == "itemId")?.Value, out var itemId) ? itemId : 0),
                    CustomerPartNo = customerCompany != null ? (_customerCompanyProductService.GetCustomerCompanyProductById(customerCompany.Id, item.ProductId)?.CustomerPartNo ?? string.Empty) : string.Empty,
                    Quantity = item.Quantity,
                    TotalPrice = Math.Round(item.PriceExclTax, 2),
                    UnitPrice = Math.Round(item.UnitPriceExclTax, 2),
                    TotalWeight = (decimal.TryParse(genAttrs.FirstOrDefault(x => x.Key == "weight")?.Value, out var weight) ? (int)Math.Round(weight * item.Quantity) : 0),
                    // product attr
                    Notes = notes,
                    SawOptions = (sawoptions?.ToLower() == "other" || sawoptions?.ToLower() == "none") ? null : sawoptions,
                    SawTolerance = sawoptions?.ToLower() == "none" ? null : workOrderInstructions,
                    Uom = uom
                });
            }

            var request = new ERPCreateOrderRequest()
            {
                ContactEmail = (await _workContext.GetCurrentCustomerAsync()).Email,
                ContactFirstName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.FirstNameAttribute),
                ContactLastName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.LastNameAttribute),
                ContactPhone = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.PhoneAttribute),
                UserId = await _genericAttributeService.GetAttributeAsync<int>(await _workContext.GetCurrentCustomerAsync(), SwiftCore.Helpers.Constants.ErpKeyAttribute),

                ShippingAddressLine1 = shippingAddress?.Address1,
                ShippingAddressLine2 = shippingAddress?.Address2,
                ShippingAddressState = (await _stateProvinceService.GetStateProvinceByIdAsync(shippingAddress?.StateProvinceId ?? 0))?.Abbreviation,
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

            // api call
            var resp = await _apiService.CreateNSSOrderAsync(erpCompId, request);

            if (resp.NSSOrderNo > 0)
                await _genericAttributeService.SaveAttributeAsync<long>(order, SwiftCore.Helpers.Constants.ErpOrderNoAttribute, resp.NSSOrderNo);

        }

        private async Task<(int companyId, CustomerCompany customerCompany)> GetCustomerCompanyDetails()
        {
            CustomerCompany customerCompany = null;

            string erpCompIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey,( await _workContext.GetCurrentCustomerAsync()).Id);
            int erpCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), erpCompIdCookieKey));

            if (erpCompanyId > 0)
                customerCompany = await _customerCompanyService.GetCustomerCompanyByErpCompIdAsync((await _workContext.GetCurrentCustomerAsync()).Id, erpCompanyId);

            return (erpCompanyId, customerCompany);
        }

        #endregion


        #endregion
    }
}