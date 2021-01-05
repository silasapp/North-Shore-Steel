using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Models.Common;
using Nop.Web.Framework.Themes;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using Nop.Core.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;

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

        public CustomerModelFactory(IThemeContext themeContext, CommonSettings commonSettings, CustomerSettings customerSettings)
        {
            _themeContext = themeContext;
            _customerSettings = customerSettings;
            _commonSettings = commonSettings;
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
    }
}