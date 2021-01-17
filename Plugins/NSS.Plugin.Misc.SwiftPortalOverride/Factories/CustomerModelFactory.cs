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
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;
using Nop.Web.Models.Common;
using Nop.Web.Factories;
using Nop.Services.Customers;
using Nop.Services.Stores;
using Nop.Services.Directory;

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
        private readonly ERPApiProvider _nSSApiProvider;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly ERPApiProvider _eRPApiProvider;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICompanyService _companyService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly AddressSettings _addressSettings;
        private readonly ICountryService _countryService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        public CustomerModelFactory(
            IAddressService addressService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            ICountryService countryService,
            AddressSettings addressSettings,
            IAddressModelFactory addressModelFactory,
            IGenericAttributeService genericAttributeService,
            ICompanyService companyService,
            ERPApiProvider eRPApiProvider,
            IWorkContext workContext,
        IThemeContext themeContext, ICustomerCompanyService customerCompanyService, ERPApiProvider nSSApiProvider, CommonSettings commonSettings, CustomerSettings customerSettings)
        {
            _themeContext = themeContext;
            _customerSettings = customerSettings;
            _commonSettings = commonSettings;
            _eRPApiProvider = eRPApiProvider;
            _workContext = workContext;
            _nSSApiProvider = nSSApiProvider;
            _genericAttributeService = genericAttributeService;
            _companyService = companyService;
            _customerCompanyService = customerCompanyService;
            _addressModelFactory = addressModelFactory;
            _customerService = customerService;
            _storeMappingService = storeMappingService;
            _countryService = countryService;
            _addressSettings = addressSettings;
            _addressService = addressService;
        }
        /// <summary>
        /// Prepare the customer navigation model
        /// </summary>
        /// <param name="selectedTabId">Identifier of the selected tab</param>
        /// <returns>Customer navigation model</returns>
        public virtual CustomerNavigationModel PrepareCustomerNavigationModel(bool isABuyer, int selectedTabId = 0)
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
            if (isABuyer)
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

        public NotificationsModel PrepareNotificationsModel(string error, IDictionary<string, bool> notifications)
        {
            var model = new NotificationsModel
            {
                Error = error
            };

            if (notifications != null && notifications.Count > 0)
            {
                foreach (var keyValue in notifications)
                {
                    switch (keyValue.Key)
                    {
                        case "my-order-confirmed-company-email":
                        case "my-order-confirmed-personal-sms":
                            AddPreference(keyValue, "When my order has been confirmed (offline orders only)", ref model);
                            break;
                        case "my-order-schedule-date-change-company-email":
                        case "my-order-schedule-date-change-personal-sms":
                            AddPreference(keyValue, "When my order has a scheduled date change", ref model);
                            break;
                        case "my-order-promise-date-change-company-email":
                        case "my-order-promise-date-change-personal-sms":
                            AddPreference(keyValue, "When my order has a promise date change", ref model);
                            break;
                        case "my-order-ready-company-email":
                        case "my-order-ready-personal-sms":
                            AddPreference(keyValue, "When my order is ready", ref model);
                            break;
                        case "my-order-loading-company-email":
                        case "my-order-loading-personal-sms":
                            AddPreference(keyValue, "When my order is loading", ref model);
                            break;
                        case "my-order-shipped-company-email":
                        case "my-order-shipped-personal-sms":
                            AddPreference(keyValue, "When my order has shipped", ref model);
                            break;
                        case "any-order-confirmed-company-email":
                        case "any-order-confirmed-personal-sms":
                            AddPreference(keyValue, "When any order has been confirmed", ref model);
                            break;
                        case "any-order-shipped-company-email":
                        case "any-order-shipped-personal-sms":
                            AddPreference(keyValue, "When any order has shipped", ref model);
                            break;
                        default:
                            break;
                    }
                }

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
            var request = new ERPSearchOrdersRequest()
            {
                FromDate = DateTimeOffset.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"),
                ToDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
                OrderId = null,
                PONo = null,
            };
            var model = new TransactionModel();
            var currentCustomer = _workContext.CurrentCustomer;

            openOrdersResponse = _nSSApiProvider.SearchOpenOrders(Convert.ToInt32(companyId), request, useMock: false);
            closedOrdersResponse = _nSSApiProvider.SearchClosedOrders(Convert.ToInt32(companyId), request, useMock: false);

            var openOrders = openOrdersResponse.Select(order => new CompanyOrderListModel.OrderDetailsModel
            {
                OrderId = order.OrderId,
                PoNo = order.PoNo,
                PromiseDate = order.PromiseDate,
                ScheduledDate = order.ScheduledDate,
                OrderStatusName = order.OrderStatusName
            }).Take(5).ToList();

            var closedOrders = closedOrdersResponse.Select(order => new CompanyOrderListModel.OrderDetailsModel
            {
                OrderId = order.OrderId,
                PoNo = order.PoNo,
                DeliveryDate = order.DeliveryDate,
                DeliveryStatus = order.DeliveryStatus
            }).Take(5).ToList();

            model.RecentOrders = _nSSApiProvider.GetRecentOrders(companyId);
            var (token, recentInvoices) = _nSSApiProvider.GetRecentInvoices(companyId);

            if (recentInvoices != null && recentInvoices.Count > 0)
            {
                foreach (var invoice in recentInvoices)
                {
                    var recentInvoice = new Invoice
                    {
                        InvoiceId = invoice.InvoiceId,
                        OrderNo = invoice.OrderNo,
                        InvoiceAmount = invoice.InvoiceAmount,
                        InvoiceDate = invoice.InvoiceDate,
                        InvoiceDueDate = invoice.InvoiceDueDate,
                        InvoiceStatusName = invoice.InvoiceStatusName,
                        InvoiceFile = $"{invoice.InvoiceFile}{token}"
                    };

                    model.RecentInvoices.Add(recentInvoice);
                }
            }

            model.CompanyInfo = _nSSApiProvider.GetCompanyInfo(companyId);
            model.OpenOrders = openOrders;
            model.ClosedOrders = closedOrders;
            var companyStats = _nSSApiProvider.GetCompanyStats(companyId);

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

            // get credit summary
            var customerCompany = _customerCompanyService.GetCustomerCompanyByErpCompId(_workContext.CurrentCustomer.Id, Convert.ToInt32(companyId));
            var creditSummary = new CompanyInvoiceListModel.CreditSummaryModel
            {
                //ApplyForCreditUrl = string.IsNullOrEmpty(_swiftCoreSettings.ApplyForCreditUrl) ? "https://www.nssco.com/assets/files/newaccountform.pdf" : _swiftCoreSettings.ApplyForCreditUrl,
                CanCredit = customerCompany?.CanCredit ?? false
            };

            if (creditSummary.CanCredit)
            {
                var creditResponse = _eRPApiProvider.GetCompanyCreditBalance(Convert.ToInt32(companyId));

                creditSummary.CreditAmount = creditResponse?.CreditAmount ?? decimal.Zero;
                creditSummary.CreditLimit = creditResponse?.CreditLimit ?? decimal.Zero;
                creditSummary.OpenInvoiceAmount = creditResponse?.OpenInvoiceAmount ?? decimal.Zero;
                creditSummary.PastDueAmount = creditResponse?.PastDueAmount ?? decimal.Zero;
            }

            model.CreditSummary = creditSummary;

            // save selected company name to generic attributes
            // display in customer info screen
            Company company = _companyService.GetCompanyEntityByErpEntityId(Convert.ToInt32(companyId));
            _genericAttributeService.SaveAttribute(currentCustomer, NopCustomerDefaults.CompanyAttribute, company.Name);

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



            //var addresses = _customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id)
            //    //enabled for the current store
            //    .Where(a => a.CountryId == null || _storeMappingService.Authorize(_countryService.GetCountryByAddress(a)))
            //    .ToList();


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