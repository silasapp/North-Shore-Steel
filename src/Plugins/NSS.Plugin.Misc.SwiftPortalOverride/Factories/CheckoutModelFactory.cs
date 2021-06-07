using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
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
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using NSS.Plugin.Misc.SwiftCore.Services;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public partial class CheckoutModelFactory : ICheckoutModelFactory
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPickupPluginManager _pickupPluginManager;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ICompanyService _companyService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public CheckoutModelFactory(
            AddressSettings addressSettings,
            CommonSettings commonSettings,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPickupPluginManager pickupPluginManager,
            IPriceFormatter priceFormatter,
            IRewardPointService rewardPointService,
            IShippingPluginManager shippingPluginManager,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            ITaxService taxService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ICompanyService companyService
            )
        {
            _addressSettings = addressSettings;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pickupPluginManager = pickupPluginManager;
            _priceFormatter = priceFormatter;
            _rewardPointService = rewardPointService;
            _shippingPluginManager = shippingPluginManager;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _taxService = taxService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _companyService = companyService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepares the checkout pickup points model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>The checkout pickup points model</returns>
        protected virtual async Task<CheckoutPickupPointsModel> PrepareCheckoutPickupPointsModelAsync(IList<ShoppingCartItem> cart)
        {
            var model = new CheckoutPickupPointsModel()
            {
                AllowPickupInStore = _shippingSettings.AllowPickupInStore
            };
            if (model.AllowPickupInStore)
            {
                model.DisplayPickupPointsOnMap =  _shippingSettings.DisplayPickupPointsOnMap;
                model.GoogleMapsApiKey = _shippingSettings.GoogleMapsApiKey;
                var pickupPointProviders = await _pickupPluginManager.LoadActivePluginsAsync(await _workContext.GetCurrentCustomerAsync(),(await _storeContext.GetCurrentStoreAsync()).Id);
                if (pickupPointProviders.Any())
                {
                    var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    var pickupPointsResponse =await _shippingService.GetPickupPointsAsync((await _workContext.GetCurrentCustomerAsync()).BillingAddressId ?? 0,
                       await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (pickupPointsResponse.Success)
                        model.PickupPoints = pickupPointsResponse.PickupPoints.SelectAwait(async point =>
                        {
                            var country =  _countryService.GetCountryByTwoLetterIsoCodeAsync(point.CountryCode);
                            var state = _stateProvinceService.GetStateProvinceByAbbreviationAsync(point.StateAbbreviation, country?.Id);

                            var pickupPointModel = new CheckoutPickupPointModel
                            {
                                Id = point.Id,
                                Name = point.Name,
                                Description = point.Description,
                                ProviderSystemName = point.ProviderSystemName,
                                Address = point.Address,
                                City = point.City,
                                County = point.County,
                                StateName = state != null ? await _localizationService.GetLocalizedAsync(state, x => x.Name, languageId) : string.Empty,
                                CountryName = country != null ? await _localizationService.GetLocalizedAsync(country, x => x.Name, languageId) : string.Empty,
                                ZipPostalCode = point.ZipPostalCode,
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                OpeningHours = point.OpeningHours
                            };

                            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                            var amount = await _orderTotalCalculationService.IsFreeShippingAsync(cart) ? 0 : point.PickupFee;

                            if (amount > 0)
                            {
                                (amount,_) = await _taxService.GetShippingPriceAsync(amount, await _workContext.GetCurrentCustomerAsync());
                                amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount,await _workContext.GetWorkingCurrencyAsync());
                                pickupPointModel.PickupFee = await _priceFormatter.FormatShippingPriceAsync(amount, true);
                            }

                            //adjust rate
                           var(shippingTotal,_) = await _orderTotalCalculationService.AdjustShippingRateAsync(point.PickupFee, cart, true);
                            var (_,rateBase) = await _taxService.GetShippingPriceAsync(shippingTotal,await _workContext.GetCurrentCustomerAsync());
                            var rate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rateBase,await _workContext.GetWorkingCurrencyAsync());
                            pickupPointModel.PickupFee = await _priceFormatter.FormatShippingPriceAsync(rate, true);

                            return pickupPointModel;
                        }).ToList();
                    else
                        foreach (var error in pickupPointsResponse.Errors)
                            model.Warnings.Add(error);
                }

                //only available pickup points
                var shippingProviders = await _shippingPluginManager.LoadActivePluginsAsync(await _workContext.GetCurrentCustomerAsync(),(await _storeContext.GetCurrentStoreAsync()).Id);
                if (!shippingProviders.Any())
                {
                    if (!pickupPointProviders.Any())
                    {
                        model.Warnings.Add( await _localizationService.GetResourceAsync("Checkout.ShippingIsNotAllowed"));
                        model.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.PickupPoints.NotAvailable"));
                    }
                    model.PickupInStoreOnly = true;
                    model.PickupInStore = true;
                    return model;
                }
            }

            return model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare billing address model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="selectedCountryId">Selected country identifier</param>
        /// <param name="prePopulateNewAddressWithCustomerFields">Pre populate new address with customer fields</param>
        /// <param name="overrideAttributesXml">Override attributes xml</param>
        /// <returns>Billing address model</returns>
        public virtual async Task<CheckoutBillingAddressModel> PrepareBillingAddressModelAsync(IList<ShoppingCartItem> cart,
            int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false,
            string overrideAttributesXml = "")
        {
            int addressId;
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, currentCustomer.Id);
            int ERPCId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, compIdCookieKey));
            var company = await _companyService.GetCompanyEntityByErpEntityIdAsync(ERPCId);
            //get address by entity id
            var attributes = await _genericAttributeService.GetAttributesForEntityAsync(company.Id, "Company");
            List<Address> addresses = new List<Address>();


            foreach (var attr in attributes)
            {
                int.TryParse(attr.Value, out addressId);
                var addy = await _addressService.GetAddressByIdAsync(addressId);
                addresses.Add(addy);
            }

            var model = new CheckoutBillingAddressModel
            {
                ShipToSameAddressAllowed = _shippingSettings.ShipToSameAddress && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart),
                //allow customers to enter (choose) a shipping address if "Disable Billing address step" setting is enabled
                ShipToSameAddress = !_orderSettings.DisableBillingAddressCheckoutStep
            };

            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                    address: address,
                    excludeProperties: false,
                    addressSettings: _addressSettings);

                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.BillingNewAddress.CountryId = selectedCountryId;
            await _addressModelFactory.PrepareAddressModelAsync(model.BillingNewAddress,
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: () =>  _countryService.GetAllCountriesForBillingAsync( _workContext.GetWorkingLanguageAsync().Result.Id),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer:await _workContext.GetCurrentCustomerAsync(),
                overrideAttributesXml: overrideAttributesXml);
            return model;
        }

        /// <summary>
        /// Prepare shipping address model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="selectedCountryId">Selected country identifier</param>
        /// <param name="prePopulateNewAddressWithCustomerFields">Pre populate new address with customer fields</param>
        /// <param name="overrideAttributesXml">Override attributes xml</param>
        /// <returns>Shipping address model</returns>
        public virtual async Task<CheckoutShippingAddressModel> PrepareShippingAddressModelAsync(IList<ShoppingCartItem> cart,
            int? selectedCountryId = null, bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            int addressId;
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, currentCustomer.Id);
            int ERPCId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, compIdCookieKey));
            var company = await _companyService.GetCompanyEntityByErpEntityIdAsync(ERPCId);
            //get address by entity id
            var attributes = await _genericAttributeService.GetAttributesForEntityAsync(company.Id, "Company");
            List<Address> addresses = new List<Address>();


            foreach (var attr in attributes)
            {
                int.TryParse(attr.Value, out addressId);
                var addy = await _addressService.GetAddressByIdAsync(addressId);
                addresses.Add(addy);
            }

            var model = new CheckoutShippingAddressModel()
            {
                DisplayPickupInStore = !_orderSettings.DisplayPickupInStoreOnShippingMethodPage
            };

            if (!_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
                model.PickupPointsModel = await PrepareCheckoutPickupPointsModelAsync(cart);

            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
               await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                    address: address,
                    excludeProperties: false,
                    addressSettings: _addressSettings);

                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.ShippingNewAddress.CountryId = selectedCountryId;
          await  _addressModelFactory.PrepareAddressModelAsync(model.ShippingNewAddress,
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async() => await _countryService.GetAllCountriesForShippingAsync( (await _workContext.GetWorkingLanguageAsync()).Id),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: await _workContext.GetCurrentCustomerAsync(),
                overrideAttributesXml: overrideAttributesXml);

            return model;
        }



        /// <summary>
        /// Prepare one page checkout model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>One page checkout model</returns>
        public virtual async Task<OnePageCheckoutModel> PrepareOnePageCheckoutModelAsync(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var model = new OnePageCheckoutModel
            {
                ShippingRequired = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart),
                DisableBillingAddressCheckoutStep =  _orderSettings.DisableBillingAddressCheckoutStep && await _customerService.GetAddressesByCustomerIdAsync((await  _workContext.GetCurrentCustomerAsync()).Id).Any(),
                BillingAddress = await PrepareBillingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true)
            };
            return model;
        }
        #endregion
    }
}