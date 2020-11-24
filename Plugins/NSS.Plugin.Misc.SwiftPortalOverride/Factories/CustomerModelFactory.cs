using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
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
        public virtual CustomerNavigationModel PrepareCustomerNavigationModel(int selectedTabId = 0)
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

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
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


            model.SelectedTab = (CustomerNavigationEnum)selectedTabId;

            return model;
        }


        /// <summary>
        /// Prepare the customer register model
        /// </summary>
        /// <param name="model">Customer register model</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomCustomerAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
        /// <param name="setDefaultValues">Whether to populate model properties by default values</param>
        /// <returns>Customer register model</returns>
        public virtual RegisterModel PrepareRegisterModel(RegisterModel model, bool excludeProperties,
            string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableAvenues.Add(new SelectListItem
            {
                Text = "Social Media",
                Value = 1.ToString(),
                Selected = true
            });

            model.AvailableAvenues.Add(new SelectListItem
            {
                Text = "Website",
                Value = 2.ToString(),
                Selected = false
            });

            model.AvailableAvenues.Add(new SelectListItem
            {
                Text = "Other",
                Value = 3.ToString(),
                Selected = false
            });

            //form fields
            model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
            model.LastNameEnabled = _customerSettings.LastNameEnabled;
            model.FirstNameRequired = _customerSettings.FirstNameRequired;
            model.LastNameRequired = _customerSettings.LastNameRequired;
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountyEnabled = _customerSettings.CountyEnabled;
            model.CountyRequired = _customerSettings.CountyRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.CountryRequired = _customerSettings.CountryRequired;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.AcceptPrivacyPolicyEnabled = _customerSettings.AcceptPrivacyPolicyEnabled;
            model.AcceptPrivacyPolicyPopup = _commonSettings.PopupForTermsOfServiceLinks;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.EnteringEmailTwice = _customerSettings.EnteringEmailTwice;
            
            return model;
        }
    }
}