using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
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
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    public class CustomOrderProcessingService : OrderProcessingService
    {
        public CustomOrderProcessingService(CurrencySettings currencySettings, IAddressService addressService, IAffiliateService affiliateService, ICheckoutAttributeFormatter checkoutAttributeFormatter, ICountryService countryService, ICurrencyService currencyService, ICustomerActivityService customerActivityService, ICustomerService customerService, ICustomNumberFormatter customNumberFormatter, IDiscountService discountService, IEncryptionService encryptionService, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, ILanguageService languageService, ILocalizationService localizationService, ILogger logger, IOrderService orderService, IOrderTotalCalculationService orderTotalCalculationService, IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IPdfService pdfService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, IProductAttributeFormatter productAttributeFormatter, IProductAttributeParser productAttributeParser, IProductService productService, IRewardPointService rewardPointService, IShipmentService shipmentService, IShippingService shippingService, IShoppingCartService shoppingCartService, IStateProvinceService stateProvinceService, ITaxService taxService, IVendorService vendorService, IWebHelper webHelper, IWorkContext workContext, IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings, OrderSettings orderSettings, PaymentSettings paymentSettings, RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings, TaxSettings taxSettings) : base(currencySettings, addressService, affiliateService, checkoutAttributeFormatter, countryService, currencyService, customerActivityService, customerService, customNumberFormatter, discountService, encryptionService, eventPublisher, genericAttributeService, giftCardService, languageService, localizationService, logger, orderService, orderTotalCalculationService, paymentPluginManager, paymentService, pdfService, priceCalculationService, priceFormatter, productAttributeFormatter, productAttributeParser, productService, rewardPointService, shipmentService, shippingService, shoppingCartService, stateProvinceService, taxService, vendorService, webHelper, workContext, workflowMessageService, localizationSettings, orderSettings, paymentSettings, rewardPointsSettings, shippingSettings, taxSettings)
        {
        }

        public override PlaceOrderResult PlaceOrder(ProcessPaymentRequest processPaymentRequest)
        {
            return base.PlaceOrder(processPaymentRequest);
        }
    }
}
