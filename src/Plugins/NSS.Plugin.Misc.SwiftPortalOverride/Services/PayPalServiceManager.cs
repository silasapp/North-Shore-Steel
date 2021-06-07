using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tax;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftCore.Domain.PayPal;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Item = PayPalCheckoutSdk.Orders.Item;
using Order = PayPalCheckoutSdk.Orders.Order;
using NSS.Plugin.Misc.SwiftCore;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    public class PayPalServiceManager
    {
        #region Fields

        private readonly IAddressService _addresService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly CurrencySettings _currencySettings;
        private readonly IShapeService _shapeService;
        private readonly IApiService _apiService;

        #endregion

        #region Ctor

        public PayPalServiceManager(IAddressService addresService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICountryService countryService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            CurrencySettings currencySettings,
            IShapeService shapeService,
            IApiService apiService)
        {
            _addresService = addresService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _countryService = countryService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _currencySettings = currencySettings;
            _shapeService = shapeService;
            _apiService = apiService;
        }

        #endregion

        #region Utilities


        /// <summary>
        /// Handle function and get result
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="settings">Plugin settings</param>
        /// <param name="function">Function</param>
        /// <returns>Result; error message if exists</returns>
        private async Task<(TResult Result, string ErrorMessage)> HandleFunctionAsync<TResult>(SwiftCoreSettings settings, Func<Task<TResult>> function)
        {
            try
            {
                ////ensure that plugin is configured
                //if (!IsConfigured(settings))
                //    throw new NopException("Plugin not configured");

                //invoke function
                return (await function(), default);
            }
            catch (Exception exception)
            {
                //get a short error message
                var message = exception.Message;
                var detailedException = exception;
                do
                {
                    detailedException = detailedException.InnerException;
                } while (detailedException?.InnerException != null);
                if (detailedException is HttpException httpException)
                {
                    var details = JsonConvert.DeserializeObject<ExceptionDetails>(httpException.Message);
                    message = !string.IsNullOrEmpty(details.Message)
                        ? details.Message
                        : (!string.IsNullOrEmpty(details.Name) ? details.Name : message);
                }

                //log errors
                var logMessage = $"{PaypalDefaults.SystemName} error: {System.Environment.NewLine}{message}";
                await _logger.ErrorAsync(logMessage, exception, await _workContext.GetCurrentCustomerAsync());

                return (default, message);
            }
        }

        /// <summary>
        /// Handle request to checkout services and get result
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="settings">Plugin settings</param>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        private async Task<TResult> HandleCheckoutRequestAsync<TRequest, TResult>(SwiftCoreSettings settings, TRequest request)
            where TRequest : HttpRequest where TResult : class
        {
            //prepare common request params
            request.Headers.Add(HeaderNames.UserAgent, PaypalDefaults.UserAgent);
            request.Headers.Add("PayPal-Partner-Attribution-Id", PaypalDefaults.PartnerCode);
            request.Headers.Add("Prefer", "return=representation");

            //execute request
            var environment = settings.PayPalUseSandbox
                ? new SandboxEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPalEnvironment
                : new LiveEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPalEnvironment;
            var client = new PayPalHttpClient(environment);
            var response = await client.Execute(request)
                ?? throw new NopException("No response from the service.");

            //return the results if necessary
            if (typeof(TResult) == typeof(object))
                return default;

            var result = response.Result<TResult>()
                ?? throw new NopException("No response from the service.");

            return result;
        }

        /// <summary>
        /// Handle request to core services and get result
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="settings">Plugin settings</param>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        private async Task<TResult> HandleCoreRequestAsync<TRequest, TResult>(SwiftCoreSettings settings, TRequest request)
            where TRequest : BraintreeHttp.HttpRequest where TResult : class
        {
            //prepare common request params
            request.Headers.Add(HeaderNames.UserAgent, PaypalDefaults.UserAgent);
            request.Headers.Add("PayPal-Partner-Attribution-Id", PaypalDefaults.PartnerCode);
            request.Headers.Add("Prefer", "return=representation");

            //execute request
            var environment = settings.PayPalUseSandbox
                ? new PayPal.Core.SandboxEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPal.Core.PayPalEnvironment
                : new PayPal.Core.LiveEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPal.Core.PayPalEnvironment;
            var client = new PayPal.Core.PayPalHttpClient(environment);
            var response = await client.Execute(request)
                ?? throw new NopException("No response from the service.");

            //return the results if necessary
            if (typeof(TResult) == typeof(object))
                return default;

            var result = response.Result<TResult>()
                ?? throw new NopException("No response from the service.");

            return result;
        }

        #endregion


        #region Methods

        /// <summary>
        /// Prepare service script
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <returns>Script; error message if exists</returns>
        public async Task<(string Script, string ErrorMessage)> GetScriptAsync(SwiftCoreSettings settings)
        {
            return await HandleFunctionAsync(settings, async () =>
            {
                var parameters = new Dictionary<string, string>
                {
                    ["client-id"] = settings.PayPalClientId,
                    ["currency"] = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode?.ToUpper(),
                    ["intent"] = "authorize",
                    ["commit"] = false.ToString().ToLower(),
                    ["vault"] = false.ToString().ToLower(),
                    ["debug"] = false.ToString().ToLower(),
                    ["components"] = "hosted-fields,buttons", //default value
                    //["merchant-id"] = null, //not used
                    //["integration-date"] = null, //not used (YYYY-MM-DD format)
                    //["buyer-country"] = null, //available in the sandbox only
                    //["locale"] = null, //PayPal auto detects it
                };

                //if (!string.IsNullOrEmpty(settings.DisabledFunding))
                //    parameters["disable-funding"] = settings.DisabledFunding;
                //if (!string.IsNullOrEmpty(settings.DisabledCards))
                //    parameters["disable-card"] = settings.DisabledCards;
                var scriptUrl = QueryHelpers.AddQueryString(PaypalDefaults.ServiceScriptUrl, parameters);

                var (accessToken, _) = await GetAccessTokenAsync(settings);

                var clientToken = await GetClientTokenAsync(settings, accessToken?.Token);

                return $@"<script src=""{scriptUrl}"" data-partner-attribution-id=""{PaypalDefaults.PartnerCode}"" data-client-token=""{clientToken}""></script>";
            });
        }

        /// <summary>
        /// Create an order
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="orderGuid">Order GUID</param>
        /// <returns>Created order; error message if exists</returns>
        public async Task<(Order Order, string ErrorMessage)> CreateOrderAsync(SwiftCoreSettings settings, Guid orderGuid, ErpCheckoutModel model)
        {
            return await HandleFunctionAsync(settings, async () =>
            {
                var currency = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode;
                if (string.IsNullOrEmpty(currency))
                    throw new NopException("Primary store currency not set");

                var billingAddress = model.BillingAddress.ShipToSameAddress ? await _addresService.GetAddressByIdAsync(model.ShippingAddress.ShippingAddressId) : await _addresService.GetAddressByIdAsync(model.BillingAddress.BillingAddressId);
                var billingAddressNew = model.BillingAddress.ShipToSameAddress ? model.ShippingAddress.ShippingNewAddress : model.BillingAddress.BillingNewAddress;

                if (billingAddress == null && model.BillingAddress != null && model.BillingAddress.BillingNewAddress == null)
                    throw new NopException("Customer billing address not set");

                var shippingAddress = await _addresService.GetAddressByIdAsync(model.ShippingAddress.ShippingAddressId);
                var shippingAddressNew = model.ShippingAddress.ShippingNewAddress;

                var billStateProvince = billingAddress != null ? await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) : await _stateProvinceService.GetStateProvinceByIdAsync(billingAddressNew.StateProvinceId ?? 0);
                var shipStateProvince = shippingAddress != null ? await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) : await _stateProvinceService.GetStateProvinceByIdAsync(shippingAddressNew.StateProvinceId ?? 0);

                //prepare order details
                var orderDetails = new OrderRequest { CheckoutPaymentIntent = "AUTHORIZE" };

                //prepare some common properties
                orderDetails.ApplicationContext = new ApplicationContext
                {
                    BrandName = CommonHelper.EnsureMaximumLength(_storeContext.GetCurrentStore().Name, 127),
                    LandingPage = LandingPageType.Billing.ToString().ToUpper(),
                    UserAction = UserActionType.Continue.ToString().ToUpper(),
                    ShippingPreference = (!model.ShippingAddress.IsPickupInStore ? ShippingPreferenceType.Set_provided_address : ShippingPreferenceType.No_shipping)
                        .ToString().ToUpper()
                };

                var firstName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.FirstNameAttribute);
                var lastName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.LastNameAttribute);
                var customer = await _workContext.GetCurrentCustomerAsync();
                var email = customer.Email;

                // billing
                var billingAddress1 = billingAddress != null ? billingAddress.Address1 : billingAddressNew.Address1;
                var billingAddress2 = billingAddress != null ? billingAddress.Address2 : billingAddressNew.Address2;
                var billingCity = billingAddress != null ? billingAddress.City : billingAddressNew.City;
                var billingCountryId = billingAddress != null ? billingAddress.CountryId : billingAddressNew.CountryId;
                var billingZipPostalCode = billingAddress != null ? billingAddress.ZipPostalCode : billingAddressNew.ZipPostalCode;
                var billingPhoneNumber = billingAddress != null ? billingAddress.PhoneNumber : await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.PhoneAttribute);

                // shipping
                var shippingAddress1 = shippingAddress != null ? shippingAddress.Address1 : shippingAddressNew.Address1;
                var shippingAddress2 = shippingAddress != null ? shippingAddress.Address2 : shippingAddressNew.Address2;
                var shippingCity = shippingAddress != null ? shippingAddress.City : shippingAddressNew.City;
                var shippingCountryId = shippingAddress != null ? shippingAddress.CountryId : shippingAddressNew.CountryId;
                var shippingZipPostalCode = shippingAddress != null ? shippingAddress.ZipPostalCode : shippingAddressNew.ZipPostalCode;
                var shippingPhoneNumber = shippingAddress != null ? shippingAddress.PhoneNumber : await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.PhoneAttribute);

                //prepare customer billing details
                orderDetails.Payer = new Payer
                {
                    Name = new Name
                    {
                        GivenName = CommonHelper.EnsureMaximumLength(firstName, 140),
                        Surname = CommonHelper.EnsureMaximumLength(lastName, 140)
                    },
                    Email = CommonHelper.EnsureMaximumLength(email, 254),
                    AddressPortable = new AddressPortable
                    {
                        AddressLine1 = CommonHelper.EnsureMaximumLength(billingAddress1, 300),
                        AddressLine2 = CommonHelper.EnsureMaximumLength(billingAddress2, 300),
                        AdminArea2 = CommonHelper.EnsureMaximumLength(billingCity, 120),
                        AdminArea1 = CommonHelper.EnsureMaximumLength(billStateProvince?.Abbreviation, 300),
                        CountryCode = _countryService.GetCountryByIdAsync(billingCountryId ?? 0).Result?.TwoLetterIsoCode,
                        PostalCode = CommonHelper.EnsureMaximumLength(billingZipPostalCode, 60)
                    }
                };
                if (!string.IsNullOrEmpty(billingPhoneNumber))
                {
                    var cleanPhone = CommonHelper.EnsureMaximumLength(CommonHelper.EnsureNumericOnly(billingPhoneNumber), 14);
                    orderDetails.Payer.PhoneWithType = new PhoneWithType { PhoneNumber = new Phone { NationalNumber = cleanPhone } };
                }

                //prepare purchase unit details
                var shoppingCart = await _shoppingCartService
                    .GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                var taxTotal = Math.Round((await _orderTotalCalculationService.GetTaxTotalAsync(shoppingCart, false)).taxTotal, 2);
                ;
                var shippingTotal = Math.Round(await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(shoppingCart) ?? decimal.Zero, 2);
                var orderTotal = Math.Round((await _orderTotalCalculationService.GetShoppingCartTotalAsync(shoppingCart, usePaymentMethodAdditionalFee: false)).shoppingCartTotal ?? decimal.Zero, 2);

                var purchaseUnit = new PurchaseUnitRequest
                {
                    ReferenceId = CommonHelper.EnsureMaximumLength(orderGuid.ToString(), 256),
                    CustomId = CommonHelper.EnsureMaximumLength(orderGuid.ToString(), 127),
                    Description = CommonHelper.EnsureMaximumLength($"Purchase at '{_storeContext.GetCurrentStoreAsync().Result.Name}'", 127),
                    SoftDescriptor = CommonHelper.EnsureMaximumLength(_storeContext.GetCurrentStoreAsync().Result.Name, 22)
                };

                //prepare shipping address details
                if (!model.ShippingAddress.IsPickupInStore)
                {
                    purchaseUnit.ShippingDetail = new ShippingDetail
                    {
                        Name = new Name { FullName = CommonHelper.EnsureMaximumLength($"{firstName} {lastName}", 300) },
                        AddressPortable = new AddressPortable
                        {
                            AddressLine1 = CommonHelper.EnsureMaximumLength(shippingAddress1, 300),
                            AddressLine2 = CommonHelper.EnsureMaximumLength(shippingAddress2, 300),
                            AdminArea2 = CommonHelper.EnsureMaximumLength(shippingCity, 120),
                            AdminArea1 = CommonHelper.EnsureMaximumLength(shipStateProvince?.Abbreviation, 300),
                            CountryCode = _countryService.GetCountryByIdAsync(shippingCountryId ?? 0).Result?.TwoLetterIsoCode,
                            PostalCode = CommonHelper.EnsureMaximumLength(shippingZipPostalCode, 60)
                        }
                    };
                }

                //set order items
                var itemTotal = decimal.Zero;
                var items = await shoppingCart.SelectAwait(async item =>
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);

                    var subtotal = Math.Round((_shoppingCartService.GetSubTotalAsync(item, false)).Result.subTotal, 2);
                    var (productPriceExclTax, _) = await _taxService.GetProductPriceAsync(product,
                 (subtotal != decimal.Zero ? subtotal / item.Quantity : decimal.Zero), false, await _workContext.GetCurrentCustomerAsync());
                    var itemPrice = Math.Round(productPriceExclTax, 2);

                    itemTotal += subtotal;

                    return new Item
                    {
                        Name = CommonHelper.EnsureMaximumLength(product.Name, 127),
                        Description = CommonHelper.EnsureMaximumLength(product.ShortDescription, 127),

                        Sku = CommonHelper.EnsureMaximumLength(product.Sku, 127),
                        Quantity = item.Quantity.ToString(),
                        Category = (product.IsDownload ? ItemCategoryType.Digital_goods : ItemCategoryType.Physical_goods)
                            .ToString().ToUpper(),
                        UnitAmount = new PayPalCheckoutSdk.Orders.Money { CurrencyCode = currency, Value = itemPrice.ToString("0.00", CultureInfo.InvariantCulture) }
                    };
                }).ToListAsync();

                //add checkout attributes as order items
                //var checkoutAttributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(_genericAttributeService
                //    .GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.CheckoutAttributes, _storeContext.CurrentStore.Id));

                //foreach (var (attribute, values) in checkoutAttributeValues)
                //{
                //    foreach (var attributeValue in values)
                //    {
                //        var attributePrice = _taxService.GetCheckoutAttributePrice(attribute, attributeValue, false, _workContext.CurrentCustomer);
                //        var roundedAttributePrice = Math.Round(attributePrice, 2);

                //        itemTotal += roundedAttributePrice;
                //        purchaseUnit.Items.Add(new Item
                //        {
                //            Name = CommonHelper.EnsureMaximumLength(attribute.Name, 127),
                //            Description = CommonHelper.EnsureMaximumLength($"{attribute.Name} - {attributeValue.Name}", 127),
                //            Quantity = 1.ToString(),
                //            UnitAmount = new PayPalCheckoutSdk.Orders.Money { CurrencyCode = currency, Value = roundedAttributePrice.ToString("0.00", CultureInfo.InvariantCulture) }
                //        });
                //    }
                //}

                //set totals
                itemTotal = Math.Round(itemTotal, 2);
                var discountTotal = Math.Round(itemTotal + taxTotal + shippingTotal - orderTotal, 2);

                purchaseUnit.AmountWithBreakdown = new AmountWithBreakdown
                {
                    CurrencyCode = currency,
                    Value = orderTotal.ToString("0.00", CultureInfo.InvariantCulture),
                    AmountBreakdown = new AmountBreakdown
                    {
                        ItemTotal = new PayPalCheckoutSdk.Orders.Money { CurrencyCode = currency, Value = itemTotal.ToString("0.00", CultureInfo.InvariantCulture) },
                        TaxTotal = new PayPalCheckoutSdk.Orders.Money { CurrencyCode = currency, Value = taxTotal.ToString("0.00", CultureInfo.InvariantCulture) },
                        Shipping = new PayPalCheckoutSdk.Orders.Money { CurrencyCode = currency, Value = shippingTotal.ToString("0.00", CultureInfo.InvariantCulture) },
                        Discount = new PayPalCheckoutSdk.Orders.Money { CurrencyCode = currency, Value = discountTotal.ToString("0.00", CultureInfo.InvariantCulture) }
                    }
                };

                orderDetails.PurchaseUnits = new List<PurchaseUnitRequest> { purchaseUnit };

                var orderRequest = new OrdersCreateRequest().RequestBody(orderDetails);
                var order = await HandleCheckoutRequestAsync<OrdersCreateRequest, Order>(settings, orderRequest);
                return order;
            });
        }

        /// <summary>
        /// Authorize a previously created order
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="orderId">Order id</param>
        /// <returns>Authorized order; error message if exists</returns>
        public async Task<(Order Order, string ErrorMessage)> AuthorizeAsync(SwiftCoreSettings settings, string orderId)
        {
            return await HandleFunctionAsync(settings, async () =>
            {
                var request = new OrdersAuthorizeRequest(orderId).RequestBody(new AuthorizeRequest());
                return await HandleCheckoutRequestAsync<OrdersAuthorizeRequest, Order>(settings, request);
            });
        }



        /// <summary>
        /// Get access token
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <returns>Access token; error message if exists</returns>

        public async Task<(AccessToken AccessToken, string ErrorMessage)> GetAccessTokenAsync(SwiftCoreSettings settings)
        {
            //try to get access token
            return await HandleFunctionAsync(settings, async () =>
             {
                 var environment = settings.PayPalUseSandbox
                     ? new SandboxEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPalEnvironment
                     : new LiveEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPalEnvironment;
                 var request = new AccessTokenRequest(environment);

                 return await HandleCheckoutRequestAsync<AccessTokenRequest, AccessToken>(settings, request);
             });
        }
        /// <summary>
        /// Get access token
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <returns>Access token; error message if exists</returns>
        public async Task<string> GetClientTokenAsync(SwiftCoreSettings settings, string accessToken)
        {
            var baseUrl = settings.PayPalUseSandbox ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";

            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);

                if (string.IsNullOrEmpty(accessToken))
                {
                    await _logger.WarningAsync($"Paypal Access Token -> ", new Exception("Paypal Access Token is empty"));
                    throw new Exception("Paypal Access Token is empty");
                }

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var resource = $"/v1/identity/generate-token";

                client.DefaultRequestHeaders.Accept.Clear();

                var response = client.PostAsync(resource, null).Result;

                // throw error if not successful
                response.EnsureSuccessStatusCode();

                var tokenResponse = JsonConvert.DeserializeObject<PaypalClientTokenResponse>(await response.Content.ReadAsStringAsync());

                return tokenResponse.client_token;
            }
        }


        #endregion
    }




    public class PaypalClientTokenResponse
    {
        public string client_token { get; set; }
        public int expires_in { get; set; }
    }
}

