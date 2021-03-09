using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Themes;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using Nop.Services.Common;
using Nop.Web.Models.Common;
using Nop.Web.Factories;
using Nop.Services.Customers;
using Nop.Services.Stores;
using Nop.Services.Directory;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.DTOs;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public partial class CustomerModelFactory : ICustomerModelFactory
    {

        private readonly IThemeContext _themeContext;
        private readonly CustomerSettings _customerSettings;
        private readonly CommonSettings _commonSettings;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICompanyService _companyService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly AddressSettings _addressSettings;
        private readonly ICountryService _countryService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly IApiService _apiService;
        public CustomerModelFactory(
            IApiService apiService,
            IAddressService addressService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            ICountryService countryService,
            AddressSettings addressSettings,
            IAddressModelFactory addressModelFactory,
            IGenericAttributeService genericAttributeService,
            ICompanyService companyService,
            IWorkContext workContext,
        IThemeContext themeContext, ICustomerCompanyService customerCompanyService, CommonSettings commonSettings, CustomerSettings customerSettings)
        {
            _themeContext = themeContext;
            _customerSettings = customerSettings;
            _commonSettings = commonSettings;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _companyService = companyService;
            _customerCompanyService = customerCompanyService;
            _addressModelFactory = addressModelFactory;
            _customerService = customerService;
            _storeMappingService = storeMappingService;
            _countryService = countryService;
            _addressSettings = addressSettings;
            _addressService = addressService;
            _apiService = apiService;
        }
        /// <summary>
        /// Prepare the customer navigation model
        /// </summary>
        /// <param name="selectedTabId">Identifier of the selected tab</param>
        /// <returns>Customer navigation model</returns>
        public virtual CustomerNavigationModel PrepareCustomerNavigationModel(bool isABuyer, bool isOperations = false, int selectedTabId = 0)
        {
            var model = new CustomerNavigationModel();
            var themeName = _themeContext.WorkingThemeName;

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerInfo",
                Title = "MY PROFILE",
                Tab = CustomerNavigationEnum.Info,
                ItemClass = "customer-info",
                ItemLogo = "/Themes/" + @themeName + "/Content/assets/icn-person.svg"
            });

            model.CompanyNavigationItems.Add(new CompanyNavigationItemModel
            {
                RouteName = "CustomerAddresses",
                Title = "ADDRESSES",
                Tab = CustomerNavigationEnum.Addresses,
                ItemClass = "customer-addresses",
                ItemLogo = "/Themes/" + @themeName + "/Content/assets/icn-location.svg"
            });

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerChangePassword",
                Title = "CHANGE PASSWORD",
                Tab = CustomerNavigationEnum.ChangePassword,
                ItemClass = "change-password",
                ItemLogo = "/Themes/" + @themeName + "/Content/assets/icn-key.svg"
            });
            if (isABuyer || isOperations)
            {
                model.CompanyNavigationItems.Add(new CompanyNavigationItemModel
                {
                    RouteName = "CustomerNotifications",
                    Title = "NOTIFICATIONS",
                    Tab = CustomerNavigationEnum.NotificationPreferences,
                    ItemClass = "customer-notifications",
                    ItemLogo = "/Themes/" + @themeName + "/Content/assets/icn-notifications.svg"
                });
            }


            model.SelectedTab = (CustomerNavigationEnum)selectedTabId;

            return model;
        }

        public NotificationsModel PrepareNotificationsModel(int ERPCompanyId, string error, IDictionary<string, bool> notifications)
        {
            bool isOperations = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, ERPCompanyId, ERPRole.Operations);
            bool isBuyer = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, ERPCompanyId, ERPRole.Buyer);

            var model = new NotificationsModel
            {
                Error = error
            };

            if (notifications != null && notifications.Count > 0)
            {
                foreach (var keyValue in notifications)
                {
                    model = populateNotificationPreferenceModel(isOperations, isBuyer, model, keyValue);
                }

            }
            else
            {
                IDictionary<string, bool> notifs = new Dictionary<string, bool>();
                if (isBuyer)
                {
                    notifs.Add(Constants.MyOrderConfirmedEmail, false);
                    notifs.Add(Constants.MyOrderConfirmedSms, false);
                    notifs.Add(Constants.MyOrderScheduleEmail, false);
                    notifs.Add(Constants.MyOrderScheduleSms, false);
                    notifs.Add(Constants.MyOrderPromiseEmail, false);
                    notifs.Add(Constants.MyOrderPromiseSms, false);
                    notifs.Add(Constants.MyOrderReadyEmail, false);
                    notifs.Add(Constants.MyOrderReadySms, false);
                    notifs.Add(Constants.MyOrderLoadingEmail, false);
                    notifs.Add(Constants.MyOrderLoadingSms, false);
                    notifs.Add(Constants.MyOrderShippedEmail, false);
                    notifs.Add(Constants.MyOrderShippedSms, false);
                    notifs.Add(Constants.AnyOrderConfirmedEmail, false);
                    notifs.Add(Constants.AnyOrderConfirmedSms, false);
                }
                if (isBuyer || isOperations)
                {
                    notifs.Add(Constants.AnyOrderShippedEmail, false);
                    notifs.Add(Constants.AnyOrderShippedSms, false);
                }
                foreach (var keyValue in notifs)
                {
                    model = populateNotificationPreferenceModel(isOperations, isBuyer, model, keyValue);
                }

            }

            return model;
        }

        private static NotificationsModel populateNotificationPreferenceModel(bool isOperations, bool isBuyer, NotificationsModel model, KeyValuePair<string, bool> keyValue)
        {
            switch (keyValue.Key)
            {
                case Constants.MyOrderConfirmedEmail:
                case Constants.MyOrderConfirmedSms:
                    if (isBuyer)
                        AddPreference(keyValue, "When my order has been confirmed (offline orders only).", ref model);
                    break;
                case Constants.MyOrderScheduleEmail:
                case Constants.MyOrderScheduleSms:
                    if (isBuyer)
                        AddPreference(keyValue, "When my order has a scheduled date change.", ref model);
                    break;
                case Constants.MyOrderPromiseEmail:
                case Constants.MyOrderPromiseSms:
                    if (isBuyer)
                        AddPreference(keyValue, "When my order has a promise date change.", ref model);
                    break;
                case Constants.MyOrderReadyEmail:
                case Constants.MyOrderReadySms:
                    if (isBuyer)
                        AddPreference(keyValue, "When my order is ready.", ref model);
                    break;
                case Constants.MyOrderLoadingEmail:
                case Constants.MyOrderLoadingSms:
                    if (isBuyer)
                        AddPreference(keyValue, "When my order is loading.", ref model);
                    break;
                case Constants.MyOrderShippedEmail:
                case Constants.MyOrderShippedSms:
                    if (isBuyer)
                        AddPreference(keyValue, "When my order has shipped.", ref model);
                    break;
                case Constants.AnyOrderConfirmedEmail:
                case Constants.AnyOrderConfirmedSms:
                    if (isBuyer || isOperations)
                        AddPreference(keyValue, "When any order has been confirmed.", ref model);
                    break;
                case Constants.AnyOrderShippedEmail:
                case Constants.AnyOrderShippedSms:
                    if (isBuyer || isOperations)
                        AddPreference(keyValue, "When any order has shipped.", ref model);
                    break;
                default:
                    break;
            }

            return model;
        }

        private static void AddPreference(KeyValuePair<string, bool> keyValue, string title, ref NotificationsModel model)
        {
            if (model.Notifications.Any(x => x.Title == title))
                    model.Notifications.FirstOrDefault(x => x.Title == title).Preferences.Add(new NotificationsModel.PreferenceModel { Key = keyValue.Key, Value = keyValue.Value });
                else
                {
                    var itemModel = new NotificationsModel.NotificationItemModel();
                    itemModel.Title = string.IsNullOrEmpty(itemModel.Title) ? title : itemModel.Title;
                    itemModel.Preferences.Add(new NotificationsModel.PreferenceModel { Key = keyValue.Key, Value = keyValue.Value });

                    model.Notifications.Add(itemModel);
                }
        }


        public TransactionModel PrepareCustomerHomeModel(string companyId)
        {
            var openOrdersResponse = new List<ERPSearchOrdersResponse>();
            var closedOrdersResponse = new List<ERPSearchOrdersResponse>();
            bool isAp = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, Int32.Parse(companyId), ERPRole.AP);
            var request = new ERPSearchOrdersRequest()
            {
                FromDate = DateTimeOffset.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"),
                ToDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
                OrderId = null,
                PONo = null,
            };
            var model = new TransactionModel();
            var currentCustomer = _workContext.CurrentCustomer;

            openOrdersResponse = _apiService.SearchOpenOrders(Convert.ToInt32(companyId), request);
            (_, closedOrdersResponse) = _apiService.SearchClosedOrders(Convert.ToInt32(companyId), request);

            var openOrders = openOrdersResponse.Select(order => new CompanyOrderListModel.OrderDetailsModel
            {
                OrderId = order.OrderId,
                PoNo = order.PoNo,
                PromiseDate = order.PromiseDate,
                ScheduledDate = order.ScheduledDate,
                OrderStatusName = order.OrderStatusName
            }).ToList();

            var closedOrders = closedOrdersResponse.Select(order => new CompanyOrderListModel.OrderDetailsModel
            {
                OrderId = order.OrderId,
                PoNo = order.PoNo,
                DeliveryDate = order.DeliveryDate,
                DeliveryStatus = order.DeliveryStatus,
                DeliveryTicketFile = $"{order.DeliveryTicketFile}",
                DeliveryTicketCount = order.DeliveryTicketCount
            }).ToList();

            var customerCompany = _customerCompanyService.GetCustomerCompanyByErpCompId(_workContext.CurrentCustomer.Id, Convert.ToInt32(companyId));
            model.CompanySalesContact = customerCompany?.Company ?? new Company();
            model.OpenOrders = openOrders?.OrderByDescending(x => x.OrderId)?.Take(5)?.ToList();
            model.ClosedOrders = closedOrders?.OrderByDescending(x => x.OrderId)?.Take(5)?.ToList();
            var companyStats = _apiService.GetCompanyStats(companyId);

            if (companyStats != null && companyStats.Count > 0)
            {

                foreach (var stats in companyStats)
                {
                    var cStats = new CompanyStats
                    {
                        StatName = stats.StatName,
                        StatValue = stats.StatValue
                    };

                    model.CompanyStats.Add(cStats);
                }
            }

            // build credit summary
            var companyInfo = _apiService.GetCompanyInfo(companyId);

            var creditSummary = new CompanyInvoiceListModel.CreditSummaryModel
            {
                CanCredit = customerCompany?.CanCredit ?? false,
                CompanyHasCreditTerms = companyInfo?.HasCredit ?? false
            };

            if ( creditSummary.CompanyHasCreditTerms && (creditSummary.CanCredit || isAp))
            {
                var creditResponse = _apiService.GetCompanyCreditBalance(Convert.ToInt32(companyId));

                creditSummary.CreditAmount = creditResponse?.CreditAmount ?? decimal.Zero;
                creditSummary.CreditLimit = creditResponse?.CreditLimit ?? decimal.Zero;
                creditSummary.OpenInvoiceAmount = creditResponse?.OpenInvoiceAmount ?? decimal.Zero;
                creditSummary.PastDueAmount = creditResponse?.PastDueAmount ?? decimal.Zero;
            }

            model.CreditSummary = creditSummary;

            // save selected company name to generic attributes
            // displayed in customer info screen
            _genericAttributeService.SaveAttribute(currentCustomer, NopCustomerDefaults.CompanyAttribute, customerCompany?.Company?.Name);

            return model;
        }


        /// <summary>
        /// Prepare the customer register model
        /// </summary>
        /// <param name="model">Customer register model</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="setDefaultValues">Whether to populate model properties by default values</param>
        /// <returns>Customer register model</returns>
        public virtual RegisterModel PrepareRegisterModel(RegisterModel model, bool excludeProperties,
            string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));


            //form fields
            model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
            model.LastNameEnabled = _customerSettings.LastNameEnabled;
            model.FirstNameRequired = _customerSettings.FirstNameRequired;
            model.LastNameRequired = _customerSettings.LastNameRequired;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.AcceptPrivacyPolicyEnabled = _customerSettings.AcceptPrivacyPolicyEnabled;
            model.AcceptPrivacyPolicyPopup = _commonSettings.PopupForTermsOfServiceLinks;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.EnteringEmailTwice = _customerSettings.EnteringEmailTwice;

            return model;
        }

        public virtual Nop.Web.Models.Customer.CustomerAddressListModel PrepareCustomerAddressListModel()
        {
            int addressId;
            var currentCustomer = _workContext.CurrentCustomer;
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, currentCustomer.Id);
            int ERPCId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(currentCustomer, compIdCookieKey));
            var company = _companyService.GetCompanyEntityByErpEntityId(ERPCId);
            //get address by entity id
            var attributes = _genericAttributeService.GetAttributesForEntity(company.Id, "Company");
            List<Address> addresses = new List<Address>();


            foreach (var attr in attributes)
            {
                int.TryParse(attr.Value, out addressId);
                var addy = _addressService.GetAddressById(addressId);
                addresses.Add(addy);
            }


            var model = new Nop.Web.Models.Customer.CustomerAddressListModel();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                _addressModelFactory.PrepareAddressModel(addressModel,
                    address: address,
                    excludeProperties: false,
                    addressSettings: _addressSettings,
                    loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));
                model.Addresses.Add(addressModel);
            }
            return model;
        }
    }
}