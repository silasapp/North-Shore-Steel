﻿using Nop.Core;
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
using System.Threading.Tasks;

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
        public virtual async Task<CustomerNavigationModel> PrepareCustomerNavigationModelAsync(bool isBuyer, bool isOperations = false, int selectedTabId = 0)
        {
            var model = new CustomerNavigationModel();
            var themeName = await _themeContext.GetWorkingThemeNameAsync();

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerInfo",
                Title = "MY PROFILE",
                Tab = CustomerNavigationEnum.Info,
                ItemClass = "customer-info",
                ItemLogo = "/Themes/" + @themeName + "/Content/assets/icn-person.svg"
            });

            if (isBuyer)
            {
                model.CompanyNavigationItems.Add(new CompanyNavigationItemModel
                {
                    RouteName = "CustomerAddresses",
                    Title = "ADDRESSES",
                    Tab = CustomerNavigationEnum.Addresses,
                    ItemClass = "customer-addresses",
                    ItemLogo = "/Themes/" + @themeName + "/Content/assets/icn-location.svg"
                });
            }

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerChangePassword",
                Title = "CHANGE PASSWORD",
                Tab = CustomerNavigationEnum.ChangePassword,
                ItemClass = "change-password",
                ItemLogo = "/Themes/" + @themeName + "/Content/assets/icn-key.svg"
            });

            if (isBuyer || isOperations)
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

        public async Task<NotificationsModel> PrepareNotificationsModelAsync(int ERPCompanyId, string error, IDictionary<string, bool> notifications)
        {
            bool isOperations = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, ERPCompanyId, ERPRole.Operations);
            bool isBuyer = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, ERPCompanyId, ERPRole.Buyer);

            var model = new NotificationsModel
            {
                Error = error
            };

            if (notifications != null && notifications.Count > 0)
            {
                foreach (var keyValue in notifications)
                {
                    model = await PopulateNotificationPreferenceModelAsync(isOperations, isBuyer, model, keyValue);
                }

            }
            else
            {
                IDictionary<string, bool> notifs = new Dictionary<string, bool>();
                if (isBuyer)
                {
                    notifs.Add(Constants.MyOrderScheduleEmail, false);
                    notifs.Add(Constants.MyOrderScheduleSms, false);
                    notifs.Add(Constants.MyOrderPromiseEmail, false);
                    notifs.Add(Constants.MyOrderPromiseSms, false);
                    notifs.Add(Constants.MyOrderLoadingEmail, false);
                    notifs.Add(Constants.MyOrderLoadingSms, false);
                    notifs.Add(Constants.MyOrderShippedEmail, false);
                    notifs.Add(Constants.MyOrderShippedSms, false);
                    notifs.Add(Constants.AnyOrderConfirmedEmail, false);
                }
                if (isBuyer || isOperations)
                {
                    notifs.Add(Constants.AnyOrderShippedEmail, false);
                }
                foreach (var keyValue in notifs)
                {
                    model = await PopulateNotificationPreferenceModelAsync(isOperations, isBuyer, model, keyValue);
                }

            }

            return model;
        }

        private static async Task<NotificationsModel> PopulateNotificationPreferenceModelAsync(bool isOperations, bool isBuyer, NotificationsModel model, KeyValuePair<string, bool> keyValue)
        {
            switch (keyValue.Key)
            {
                case Constants.MyOrderConfirmedEmail:
                case Constants.MyOrderConfirmedSms:
                    if (isBuyer)
                      await AddPreferenceAsync(keyValue, "When my order has been confirmed (offline orders only).",  model);
                    break;
                case Constants.MyOrderScheduleEmail:
                case Constants.MyOrderScheduleSms:
                    if (isBuyer)
                      await AddPreferenceAsync(keyValue, "When my order has a scheduled date change.",  model);
                    break;
                case Constants.MyOrderPromiseEmail:
                case Constants.MyOrderPromiseSms:
                    if (isBuyer)
                     await AddPreferenceAsync(keyValue, "When my order has a promise date change.",  model);
                    break;
                case Constants.MyOrderReadyEmail:
                case Constants.MyOrderReadySms:
                    if (isBuyer)
                      await  AddPreferenceAsync(keyValue, "When my order is ready.", model);
                    break;
                case Constants.MyOrderLoadingEmail:
                case Constants.MyOrderLoadingSms:
                    if (isBuyer)
                      await AddPreferenceAsync(keyValue, "When my order is loading.",  model);
                    break;
                case Constants.MyOrderShippedEmail:
                case Constants.MyOrderShippedSms:
                    if (isBuyer)
                       await AddPreferenceAsync(keyValue, "When my order has shipped.",  model);
                    break;
                case Constants.AnyOrderConfirmedEmail:
                case Constants.AnyOrderConfirmedSms:
                    if (isBuyer || isOperations)
                      await AddPreferenceAsync(keyValue, "When any order has been confirmed.", model);
                    break;
                case Constants.AnyOrderShippedEmail:
                case Constants.AnyOrderShippedSms:
                    if (isBuyer || isOperations)
                      await  AddPreferenceAsync(keyValue, "When any order has shipped.", model);
                    break;
                default:
                    break;
            }

            return model;
        }

        private static async Task AddPreferenceAsync(KeyValuePair<string, bool> keyValue, string title,  NotificationsModel model)
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


        public async Task<TransactionModel> PrepareCustomerHomeModelAsync(string companyId)
        {
            var openOrdersResponse = new List<ERPSearchOrdersResponse>();
            var closedOrdersResponse = new List<ERPSearchOrdersResponse>();
            bool isAp = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, Int32.Parse(companyId), ERPRole.AP);
            var request = new ERPSearchOrdersRequest()
            {
                FromDate = DateTimeOffset.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"),
                ToDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
                OrderId = null,
                PONo = null,
            };
            var model = new TransactionModel();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            openOrdersResponse = await _apiService.SearchOpenOrdersAsync(Convert.ToInt32(companyId), request);
            (_, closedOrdersResponse) = await _apiService.SearchClosedOrdersAsync(Convert.ToInt32(companyId), request);

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

            var customerCompany = await _customerCompanyService.GetCustomerCompanyByErpCompIdAsync((await _workContext.GetCurrentCustomerAsync()).Id, Convert.ToInt32(companyId));
            model.CompanySalesContact = customerCompany?.Company ?? new Company();
            model.OpenOrders = openOrders?.OrderByDescending(x => x.OrderId)?.Take(5)?.ToList();
            model.ClosedOrders = closedOrders?.OrderByDescending(x => x.OrderId)?.Take(5)?.ToList();
            var companyStats = await _apiService.GetCompanyStatsAsync(companyId);

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
            var companyInfo = await _apiService.GetCompanyInfoAsync(companyId);

            var creditSummary = new CompanyInvoiceListModel.CreditSummaryModel
            {
                CanCredit = customerCompany?.CanCredit ?? false,
                CompanyHasCreditTerms = companyInfo?.HasCredit ?? false
            };

            if (creditSummary.CompanyHasCreditTerms && isAp)
            {
                var creditResponse = await _apiService.GetCompanyCreditBalanceAsync(Convert.ToInt32(companyId));

                creditSummary.CreditAmount = creditResponse?.CreditAmount ?? decimal.Zero;
                creditSummary.CreditLimit = creditResponse?.CreditLimit ?? decimal.Zero;
                creditSummary.OpenInvoiceAmount = creditResponse?.OpenInvoiceAmount ?? decimal.Zero;
                creditSummary.PastDueAmount = creditResponse?.PastDueAmount ?? decimal.Zero;
            }

            model.CreditSummary = creditSummary;

            // save selected company name to generic attributes
            // displayed in customer info screen
           await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.CompanyAttribute, customerCompany?.Company?.Name);

            return model;
        }


        /// <summary>
        /// Prepare the customer register model
        /// </summary>
        /// <param name="model">Customer register model</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="setDefaultValues">Whether to populate model properties by default values</param>
        /// <returns>Customer register model</returns>
        public virtual async Task<RegisterModel> PrepareRegisterModelAsync(RegisterModel model, bool excludeProperties,
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

        public virtual async Task <Nop.Web.Models.Customer.CustomerAddressListModel> PrepareCustomerAddressListModelAsync(int erpCompanyId)
        {
            var company = await _companyService.GetCompanyEntityByErpEntityIdAsync(erpCompanyId);
            //get address by entity id
            List<Address> addresses = new List<Address>();

            if (company != null)
            {
                var attributes = await _genericAttributeService.GetAttributesForEntityAsync(company.Id, "Company");
                foreach (var attr in attributes)
                {
                    int.TryParse(attr.Value, out int addressId);
                    var addy = await _addressService.GetAddressByIdAsync(addressId);
                    addresses.Add(addy);
                }
            }


            var model = new Nop.Web.Models.Customer.CustomerAddressListModel();
            if(addresses != null)
            {
                foreach (var address in addresses)
                {
                    var addressModel = new AddressModel();
                    await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                        address: address,
                        excludeProperties: false,
                        addressSettings: _addressSettings,
                        loadCountries: null);
                    model.Addresses.Add(addressModel);
                }
            }
            return model;
        }
    }
}