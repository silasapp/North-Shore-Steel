using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using NSS.Plugin.Misc.SwiftCore;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    public class CustomOrderProcessingService : OrderProcessingService
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IAddressService _addressService;
        private readonly IAffiliateService _affiliateService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ITaxService _taxService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly PayPalProcessor _payPalServiceManager;
        private readonly SwiftCoreSettings _swiftCoreSettings;

        #endregion

        #region Ctor

        public CustomOrderProcessingService(PayPalProcessor payPalServiceManager, SwiftCoreSettings swiftCoreSettings, CurrencySettings currencySettings, IAddressService addressService, IAffiliateService affiliateService, ICheckoutAttributeFormatter checkoutAttributeFormatter, ICountryService countryService, ICurrencyService currencyService, ICustomerActivityService customerActivityService, ICustomerService customerService, ICustomNumberFormatter customNumberFormatter, IDiscountService discountService, IEncryptionService encryptionService, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, ILanguageService languageService, ILocalizationService localizationService, ILogger logger, IOrderService orderService, IOrderTotalCalculationService orderTotalCalculationService, IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IPdfService pdfService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, IProductAttributeFormatter productAttributeFormatter, IProductAttributeParser productAttributeParser, IProductService productService, IRewardPointService rewardPointService, IShipmentService shipmentService, IShippingService shippingService, IShoppingCartService shoppingCartService, IStateProvinceService stateProvinceService, ITaxService taxService, IVendorService vendorService, IWebHelper webHelper, IWorkContext workContext, IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings, OrderSettings orderSettings, PaymentSettings paymentSettings, RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings, TaxSettings taxSettings) : base(currencySettings, addressService, affiliateService, checkoutAttributeFormatter, countryService, currencyService, customerActivityService, customerService, customNumberFormatter, discountService, encryptionService, eventPublisher, genericAttributeService, giftCardService, languageService, localizationService, logger, orderService, orderTotalCalculationService, paymentPluginManager, paymentService, pdfService, priceCalculationService, priceFormatter, productAttributeFormatter, productAttributeParser, productService, rewardPointService, shipmentService, shippingService, shoppingCartService, stateProvinceService, taxService, vendorService, webHelper, workContext, workflowMessageService, localizationSettings, orderSettings, paymentSettings, rewardPointsSettings, shippingSettings, taxSettings)
        {
            _currencySettings = currencySettings;
            _addressService = addressService;
            _affiliateService = affiliateService;
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _customNumberFormatter = customNumberFormatter;
            _discountService = discountService;
            _encryptionService = encryptionService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _languageService = languageService;
            _localizationService = localizationService;
            _logger = logger;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _rewardPointService = rewardPointService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _taxService = taxService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
            _swiftCoreSettings = swiftCoreSettings;
            _payPalServiceManager = payPalServiceManager;
        }

        #endregion

        public override async Task<PlaceOrderResult> PlaceOrderAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentNullException(nameof(processPaymentRequest));

            var result = new PlaceOrderResult();
            try
            {
                if (processPaymentRequest.OrderGuid == Guid.Empty)
                    throw new Exception("Order GUID is not generated");

                //prepare order details
                var details = await PreparePlaceOrderDetailsAsync(processPaymentRequest);

                var processPaymentResult =await GetProcessPaymentResultAsync(processPaymentRequest, details);

                if (processPaymentResult == null)
                    throw new NopException("processPaymentResult is not available");

                if (processPaymentResult.Success)
                {
                    var order =await SaveOrderDetailsAsync(processPaymentRequest, processPaymentResult, details);
                    result.PlacedOrder = order;

                    //move shopping cart items to order items
                  await  MoveShoppingCartItemsToOrderItemsAsync(details, order);

                    //discount usage history
                   await SaveDiscountUsageHistoryAsync(details, order);

                    //gift card usage history
                   await SaveGiftCardUsageHistoryAsync(details, order);

                    //recurring orders
                    if (details.IsRecurringShoppingCart)
                    {
                       await CreateFirstRecurringPaymentAsync(processPaymentRequest, order);
                    }

                    //notifications
                   await SendNotificationsAndSaveNotesAsync(order);

                    //reset checkout data
                   await _customerService.ResetCheckoutDataAsync(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);
                  await  _customerActivityService.InsertActivityAsync("PublicStore.PlaceOrder",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.PlaceOrder"), order.Id), order);

                    //check order status
                   await CheckOrderStatusAsync(order);

                    //raise event       
                    await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));

                    if (order.PaymentStatus == PaymentStatus.Paid)
                       await ProcessOrderPaidAsync(order);
                }
                else
                    foreach (var paymentError in processPaymentResult.Errors)
                        result.AddError(string.Format(await _localizationService.GetResourceAsync("Checkout.PaymentError"), paymentError));
            }
            catch (Exception exc)
            {
              await  _logger.ErrorAsync(exc.Message, exc);
                result.AddError(exc.Message);
            }

            if (result.Success)
                return result;

            //log errors
            var logError = result.Errors.Aggregate("Error while placing order. ",
                (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            await _logger.ErrorAsync(logError, customer: customer);

            return result;
        }

        /// <summary>
        /// Prepare details to place an order. It also sets some properties to "processPaymentRequest"
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Details</returns>
        protected override async Task<PlaceOrderContainer> PreparePlaceOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainer
            {
                //customer
                Customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId)
            };
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

            //affiliate
            var affiliate = await _affiliateService.GetAffiliateByIdAsync(details.Customer.AffiliateId);
            if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                details.AffiliateId = affiliate.Id;

            //check whether customer is guest
            if (await _customerService.IsGuestAsync(details.Customer) && ! _orderSettings.AnonymousCheckoutAllowed)
                throw new NopException("Anonymous checkout is not allowed");

            //customer currency
            var currencyTmp =await _currencyService.GetCurrencyByIdAsync(
               await _genericAttributeService.GetAttributeAsync<int>(details.Customer, NopCustomerDefaults.CurrencyIdAttribute, processPaymentRequest.StoreId));
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp :await _workContext.GetWorkingCurrencyAsync();
            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
            details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;

            //customer language
            details.CustomerLanguage = await _languageService.GetLanguageByIdAsync(
              await  _genericAttributeService.GetAttributeAsync<int>(details.Customer, NopCustomerDefaults.LanguageIdAttribute, processPaymentRequest.StoreId));
            if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
                details.CustomerLanguage =await _workContext.GetWorkingLanguageAsync();

            //billing address
            if (details.Customer.BillingAddressId != null)
            {
                if (details.Customer.BillingAddressId is null)
                    throw new NopException("Billing address is not provided");

                var billingAddress = await _customerService.GetCustomerBillingAddressAsync(details.Customer);

                if (!CommonHelper.IsValidEmail(billingAddress?.Email))
                    throw new NopException("Email is not valid");

                details.BillingAddress = _addressService.CloneAddress(billingAddress);

                if (await _countryService.GetCountryByAddressAsync(details.BillingAddress) is Country billingCountry && !billingCountry.AllowsBilling)
                    throw new NopException($"Country '{billingCountry.Name}' is not allowed for billing");
            }


            //checkout attributes
            details.CheckoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(details.Customer, NopCustomerDefaults.CheckoutAttributes, processPaymentRequest.StoreId);
            details.CheckoutAttributeDescription =await _checkoutAttributeFormatter.FormatAttributesAsync(details.CheckoutAttributesXml, details.Customer);

            //load shopping cart
            details.Cart =await _shoppingCartService.GetShoppingCartAsync(details.Customer, ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId);

            if (!details.Cart.Any())
                throw new NopException("Cart is empty");

            //validate the entire shopping cart
            var warnings =await _shoppingCartService.GetShoppingCartWarningsAsync(details.Cart, details.CheckoutAttributesXml, true);
            if (warnings.Any())
                throw new NopException(warnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));

            //validate individual cart items
            foreach (var sci in details.Cart)
            {
                var product =await _productService.GetProductByIdAsync(sci.ProductId);
                product.OrderMaximumQuantity = product.OrderMaximumQuantity > 0 ? product.OrderMaximumQuantity : 0;

                var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(details.Customer,
                    sci.ShoppingCartType, product, processPaymentRequest.StoreId, sci.AttributesXml,
                    sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false, sci.Id);
                if (sciWarnings.Any())
                    throw new NopException(sciWarnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));
            }

            //min totals validation
            if (! await ValidateMinOrderSubtotalAmountAsync(details.Cart))
            {
                var minOrderSubtotalAmount =await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderSubtotalAmount,await _workContext.GetWorkingCurrencyAsync());
                throw new NopException(string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderSubtotalAmount"),
                   await _priceFormatter.FormatPriceAsync(minOrderSubtotalAmount, true, false)));
            }

            if (!await ValidateMinOrderTotalAmountAsync(details.Cart))
            {
                var minOrderTotalAmount =await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderTotalAmount,await _workContext.GetWorkingCurrencyAsync());
                throw new NopException(string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderTotalAmount"),
                   await _priceFormatter.FormatPriceAsync(minOrderTotalAmount, true, false)));
            }

            //tax display type
            if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
                details.CustomerTaxDisplayType = (TaxDisplayType)await _genericAttributeService.GetAttributeAsync<int>(details.Customer, NopCustomerDefaults.TaxDisplayTypeIdAttribute, processPaymentRequest.StoreId);
            else
                details.CustomerTaxDisplayType = _taxSettings.TaxDisplayType;

            //sub total (incl tax)
            var(orderSubTotalDiscountAmount, orderSubTotalAppliedDiscounts,  subTotalWithoutDiscountBase, _,_)  = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(details.Cart, true);
            details.OrderSubTotalInclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount;

            //discount history
            foreach (var disc in orderSubTotalAppliedDiscounts)
                if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                    details.AppliedDiscounts.Add(disc);

            //sub total (excl tax)
          // await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(details.Cart, false, out orderSubTotalDiscountAmount,
               // out orderSubTotalAppliedDiscounts, out subTotalWithoutDiscountBase, out _);
           (orderSubTotalDiscountAmount,orderSubTotalAppliedDiscounts, subTotalWithoutDiscountBase, _,_)
                =await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(details.Cart, false);
            details.OrderSubTotalExclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount;

            //shipping info
            if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(details.Cart))
            {
                var pickupPoint =await _genericAttributeService.GetAttributeAsync<PickupPoint>(details.Customer,
                    NopCustomerDefaults.SelectedPickupPointAttribute, processPaymentRequest.StoreId);
                if (_shippingSettings.AllowPickupInStore && pickupPoint != null)
                {
                    var country =await _countryService.GetCountryByTwoLetterIsoCodeAsync(pickupPoint.CountryCode);
                    var state =await _stateProvinceService.GetStateProvinceByAbbreviationAsync(pickupPoint.StateAbbreviation, country?.Id);

                    details.PickupInStore = true;
                    details.PickupAddress = new Address
                    {
                        Address1 = pickupPoint.Address,
                        City = pickupPoint.City,
                        County = pickupPoint.County,
                        CountryId = country?.Id,
                        StateProvinceId = state?.Id,
                        ZipPostalCode = pickupPoint.ZipPostalCode,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                }
                else
                {
                    if (details.Customer.ShippingAddressId == null)
                        throw new NopException("Shipping address is not provided");

                    var shippingAddress =await _customerService.GetCustomerShippingAddressAsync(details.Customer);

                    if (!CommonHelper.IsValidEmail(shippingAddress?.Email))
                        throw new NopException("Email is not valid");

                    //clone shipping address
                    details.ShippingAddress = _addressService.CloneAddress(shippingAddress);

                    if (await _countryService.GetCountryByAddressAsync(details.ShippingAddress) is Country shippingCountry && !shippingCountry.AllowsShipping)
                        throw new NopException($"Country '{shippingCountry.Name}' is not allowed for shipping");
                }

                var shippingOption =await _genericAttributeService.GetAttributeAsync<ShippingOption>(details.Customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute, processPaymentRequest.StoreId);
                if (shippingOption != null)
                {
                    details.ShippingMethodName = shippingOption.Name;
                    details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                }

                details.ShippingStatus = ShippingStatus.NotYetShipped;
            }
            else
                details.ShippingStatus = ShippingStatus.ShippingNotRequired;

            //shipping total
          //  var orderShippingTotalInclTax =await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(details.Cart, true, out var _, out var shippingTotalDiscounts);
            var (orderShippingTotalInclTax, taxRate, shippingTotalDiscounts) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(details.Cart, true);
            var (orderShippingTotalExclTax, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(details.Cart, false);
            if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                throw new NopException("Shipping total couldn't be calculated");

            details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
            details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

            foreach (var disc in shippingTotalDiscounts)
                if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                    details.AppliedDiscounts.Add(disc);

            //payment total
            var paymentAdditionalFee =await _paymentService.GetAdditionalHandlingFeeAsync(details.Cart, processPaymentRequest.PaymentMethodSystemName);
            (details.PaymentAdditionalFeeInclTax,_) = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, true, details.Customer);
            (details.PaymentAdditionalFeeExclTax,_) = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, false, details.Customer);

            //tax amount
           var(orderTaxTotal, taxRatesDictionary) = await _orderTotalCalculationService.GetTaxTotalAsync(details.Cart);
            details.OrderTaxTotal = orderTaxTotal;

            //VAT number
            var customerVatStatus = (VatNumberStatus) await _genericAttributeService.GetAttributeAsync<int>(details.Customer, NopCustomerDefaults.VatNumberStatusIdAttribute);
            if (_taxSettings.EuVatEnabled && customerVatStatus == VatNumberStatus.Valid)
                details.VatNumber =await _genericAttributeService.GetAttributeAsync<string>(details.Customer, NopCustomerDefaults.VatNumberAttribute);

            //tax rates
            details.TaxRates = taxRatesDictionary.Aggregate(string.Empty, (current, next) =>
                $"{current}{next.Key.ToString(CultureInfo.InvariantCulture)}:{next.Value.ToString(CultureInfo.InvariantCulture)};   ");

            //order total (and applied discounts, gift cards, reward points)
          //  var orderTotal =await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(details.Cart, out var orderDiscountAmount, out var orderAppliedDiscounts, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount);
           var (orderTotal, orderDiscountAmount, orderAppliedDiscounts, appliedGiftCards,  redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(details.Cart);
            if (!orderTotal.HasValue)
                throw new NopException("Order total couldn't be calculated");

            details.OrderDiscountAmount = orderDiscountAmount;
            details.RedeemedRewardPoints = redeemedRewardPoints;
            details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
            details.AppliedGiftCards = appliedGiftCards;
            details.OrderTotal = orderTotal.Value;

            //discount history
            foreach (var disc in orderAppliedDiscounts)
                if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                    details.AppliedDiscounts.Add(disc);

            processPaymentRequest.OrderTotal = details.OrderTotal;

            //recurring or standard shopping cart?
            details.IsRecurringShoppingCart =await _shoppingCartService.ShoppingCartIsRecurringAsync(details.Cart);
            if (!details.IsRecurringShoppingCart)
                return details;

            //var recurringCyclesError = await _shoppingCartService.GetRecurringCycleInfoAsync(details.Cart,
            //  out var recurringCycleLength, out var recurringCyclePeriod, out var recurringTotalCycles);
            var (recurringCyclesError, recurringCycleLength, recurringCyclePeriod, recurringTotalCycles) = await _shoppingCartService.GetRecurringCycleInfoAsync(details.Cart);
            if (!string.IsNullOrEmpty(recurringCyclesError))
                throw new NopException(recurringCyclesError);

            processPaymentRequest.RecurringCycleLength = recurringCycleLength;
            processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
            processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;

            return details;
        }

        /// <summary>
        /// Save order and add order notes
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <param name="processPaymentResult">Process payment result</param>
        /// <param name="details">Details</param>
        /// <returns>Order</returns>
        protected override async Task<Order> SaveOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest,
            ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
        {
            var order = new Order
            {
                StoreId = processPaymentRequest.StoreId,
                OrderGuid = processPaymentRequest.OrderGuid,
                CustomerId = details.Customer.Id,
                CustomerLanguageId = details.CustomerLanguage.Id,
                CustomerTaxDisplayType = details.CustomerTaxDisplayType,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                OrderSubtotalInclTax = details.OrderSubTotalInclTax,
                OrderSubtotalExclTax = details.OrderSubTotalExclTax,
                OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
                OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
                OrderShippingInclTax = details.OrderShippingTotalInclTax,
                OrderShippingExclTax = details.OrderShippingTotalExclTax,
                PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
                PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
                TaxRates = details.TaxRates,
                OrderTax = details.OrderTaxTotal,
                OrderTotal = details.OrderTotal,
                RefundedAmount = decimal.Zero,
                OrderDiscount = details.OrderDiscountAmount,
                CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                CheckoutAttributesXml = details.CheckoutAttributesXml,
                CustomerCurrencyCode = details.CustomerCurrencyCode,
                CurrencyRate = details.CustomerCurrencyRate,
                AffiliateId = details.AffiliateId,
                OrderStatus = OrderStatus.Pending,
                AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
                CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
                CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
                MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
                CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
                CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
                AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                PaymentStatus = processPaymentResult.NewPaymentStatus,
                PaidDateUtc = null,
                PickupInStore = details.PickupInStore,
                ShippingStatus = details.ShippingStatus,
                ShippingMethod = details.ShippingMethodName,
                ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
                CustomValuesXml = _paymentService.SerializeCustomValues(processPaymentRequest),
                VatNumber = details.VatNumber,
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = string.Empty
            };


            if (details.BillingAddress is null)
            {
                if (details.PickupAddress != null)
                {
                   await _addressService.InsertAddressAsync(details.PickupAddress);
                    order.BillingAddressId = details.PickupAddress.Id;
                }
                else if (details.ShippingAddress != null)
                {
                   await _addressService.InsertAddressAsync(details.ShippingAddress);
                    order.BillingAddressId = details.ShippingAddress.Id;
                }
            }
            else
            {
               await _addressService.InsertAddressAsync(details.BillingAddress);
                order.BillingAddressId = details.BillingAddress.Id;
            }

            if (details.PickupAddress != null)
            {
              await  _addressService.InsertAddressAsync(details.PickupAddress);
                order.PickupAddressId = details.PickupAddress.Id;
            }

            if (details.ShippingAddress != null)
            {
               await _addressService.InsertAddressAsync(details.ShippingAddress);
                order.ShippingAddressId = details.ShippingAddress.Id;
            }

           await _orderService.InsertOrderAsync(order);

            //generate and set custom order number
            order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
           await _orderService.UpdateOrderAsync(order);

            //reward points history
            if (details.RedeemedRewardPointsAmount <= decimal.Zero)
                return order;

           await _rewardPointService.AddRewardPointsHistoryEntryAsync(details.Customer, -details.RedeemedRewardPoints, order.StoreId,
                string.Format(await _localizationService.GetResourceAsync("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.CustomOrderNumber),
                order, details.RedeemedRewardPointsAmount);
           await _customerService.UpdateCustomerAsync(details.Customer);

            return order;
        }

        /// <summary>
        /// Get process payment result
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <param name="details">Place order container</param>
        /// <returns></returns>
        protected override async Task<ProcessPaymentResult> GetProcessPaymentResultAsync(ProcessPaymentRequest processPaymentRequest, PlaceOrderContainer details)
        {
            //process payment
            ProcessPaymentResult processPaymentResult = new ProcessPaymentResult();

            processPaymentRequest.CustomValues.TryGetValue(PaypalDefaults.PaymentMethodTypeKey, out var paymentMethodType);

            if(paymentMethodType != null)
            {
                switch (paymentMethodType.ToString())
                {
                    case "CREDITCARD":
                    case "PAYPAL":
                        processPaymentResult = await ProcessPayPalPaymentAsync(processPaymentRequest);
                        break;
                    case "CREDIT":
                        break;

                    default:
                        break;
                }
            }
            else
            {
                throw new Exception("Payment method type not found");
            }

            return processPaymentResult;
        }

        protected override async Task SendNotificationsAndSaveNotesAsync(Order order)
        {
            //notes, messages
          await  AddOrderNoteAsync(order, _workContext.OriginalCustomerIfImpersonated != null
                ? $"Order placed by a store owner ('{ _workContext.OriginalCustomerIfImpersonated.Email}'. ID = {_workContext.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
                : "Order placed");
        }


        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public async Task<ProcessPaymentResult> ProcessPayPalPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            //try to get an order id from custom values
            var orderIdKey = PaypalDefaults.PayPalOrderIdKey;
            if (!processPaymentRequest.CustomValues.TryGetValue(orderIdKey, out var orderId) || string.IsNullOrEmpty(orderId?.ToString()))
                throw new NopException("Failed to get the PayPal order ID");

            //authorize or capture the order
            var (order, error) = await _payPalServiceManager.AuthorizeAsync(_swiftCoreSettings, orderId.ToString());

            if (!string.IsNullOrEmpty(error))
                return new ProcessPaymentResult { Errors = new[] { error } };

            //request succeeded
            var result = new ProcessPaymentResult();

            var purchaseUnit = order.PurchaseUnits.FirstOrDefault(item => item.ReferenceId.Equals(processPaymentRequest.OrderGuid.ToString()));
            var authorization = purchaseUnit.Payments?.Authorizations?.FirstOrDefault();
            if (authorization != null)
            {
                result.AuthorizationTransactionId = authorization.Id;
                result.AuthorizationTransactionResult = authorization.Status;
                result.NewPaymentStatus = PaymentStatus.Authorized;
            }
            var capture = purchaseUnit.Payments?.Captures?.FirstOrDefault();
            if (capture != null)
            {
                result.CaptureTransactionId = capture.Id;
                result.CaptureTransactionResult = capture.Status;
                result.NewPaymentStatus = PaymentStatus.Paid;
            }

            return result;
        }
    }
}
