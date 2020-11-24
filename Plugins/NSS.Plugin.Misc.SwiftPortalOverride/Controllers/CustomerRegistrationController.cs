using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using NSS.Plugin.Misc.SwiftPortalOverride.Requests;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using Nop.Web.Factories;
using ICustomerModelFactory = NSS.Plugin.Misc.SwiftPortalOverride.Factories.ICustomerModelFactory;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{

    public partial class CustomerRegistrationController : BasePublicController
    {
        #region fields
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressService _addressService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ERPApiProvider _nSSApiProvider;
        private readonly WorkFlowMessageServiceOverride _workFlowMessageServiceOverride;


        #endregion

        #region Constructor

        public CustomerRegistrationController(
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            GdprSettings gdprSettings,
            IAddressService addressService,
            IAuthenticationService authenticationService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            TaxSettings taxSettings,
            ERPApiProvider nSSApiProvider,
            WorkFlowMessageServiceOverride workFlowMessageServiceOverride
            )
        {
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _gdprSettings = gdprSettings;
            _addressService = addressService;
            _authenticationService = authenticationService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _taxSettings = taxSettings;
            _nSSApiProvider = nSSApiProvider;
            _workFlowMessageServiceOverride = workFlowMessageServiceOverride;
        }

        #endregion


        #region Utilities
        protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IFormCollection form)
        {
            foreach (var consent in consents)
            {
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
                {
                    ModelState.AddModelError("", consent.RequiredMessage);
                }
            }
        }

        protected virtual string ParseCustomCustomerAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = "";
            var attributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        protected void RegisterNSSUser(RegisterModel model, IFormCollection form, Customer customer)
        {
            try
            {
                // save customer as no NSSApproved by default
                _genericAttributeService.SaveAttribute(customer, Constants.NSSApprovedAttribute, false);

                // prepare request for create api call

                //var request = new ERPCreateUserRequest
                //{
                //    SwiftUserId = customer.Id.ToString(),
                //    Firstname = model.FirstName,
                //    LastName = model.LastName,
                //    WorkEmail = model.Email,
                //    //CellPhone = model.CellPhone,
                //    //WorkPhone = model.WorkPhone,
                //    CompanyName = model.Company,
                //    IsExistingCustomer = "0"
                //};

                #region BuildCustomAttributes
                //var attributes = _customerAttributeService.GetAllCustomerAttributes();
                //buildCustomAttributes(form, request, attributes);

                #endregion

                var response = _nSSApiProvider.CreateNSSUser(null);

                if (response != null && response.WitnrixId != null)
                {
                    // save wintrix id
                    _genericAttributeService.SaveAttribute(customer, Constants.ErpKeyAttribute, response.WitnrixId);
                }

                // send nss an email
                _workFlowMessageServiceOverride.SendNSSCustomerRegisteredNotificationMessage(customer, _workContext.WorkingLanguage.Id, response?.WitnrixId);
            }
            catch (Exception)
            {

                // silent NSS error
            }

        }

        protected void buildCustomAttributes(IFormCollection form, ERPCreateUserRequest request, IList<CustomerAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                {
                                    //var val = attribute.Values.Where(x => x.Id == selectedAttributeId).FirstOrDefault();
                                    var values = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                                    var val = values.Where(x => x.Id == selectedAttributeId).FirstOrDefault();
                                    if (val != null)
                                    {
                                        if (attribute.Name == Constants.HearAboutUsAttribute)
                                            request.HearAboutUs = val.Name;
                                        if (attribute.Name == Constants.PreferredLocationIdAttribute)
                                        {
                                            if (val.Name.ToLower() == "houston")
                                            {
                                                val.Id = 1;
                                            }
                                            else if (val.Name.ToLower() == "beaumont")
                                            {
                                                val.Id = 2;
                                            }
                                            request.PreferredLocationid = val.Id.ToString();
                                        }

                                    }
                                }
                            }

                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                var items = cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (items.Length > 0)
                                {
                                    if (attribute.Name == Constants.IsExistingCustomerAttribute)
                                        request.IsExistingCustomer = "1";
                                }
                                else
                                {
                                    request.IsExistingCustomer = "0";
                                }
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();

                                if (attribute.Name == Constants.ItemsForNextProjectAttribute)
                                    request.ItemsForNextProject = enteredText;

                                if (attribute.Name == Constants.OtherAttribute)
                                    request.Other = enteredText;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion


        #region Methods


        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Register()
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            var model = new RegisterModel();
            model = _customerModelFactory.PrepareRegisterModel(model, false, setDefaultValues: true);

            //For view give full path of your published plugin
            return View(model);

        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            if (_customerService.IsRegistered(_workContext.CurrentCustomer))
            {
                //Already registered customer. 
                _authenticationService.SignOut();

                //raise logged out event       
                _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

                //Save a new record
                _workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;
            customer.RegisteredInStoreId = _storeContext.CurrentStore.Id;

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(form);
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            

           

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer,
                    model.Email,
                    _customerSettings.UsernamesEnabled ? model.Username : model.Email,
                    model.Password,
                    _customerSettings.DefaultPasswordFormat,
                    _storeContext.CurrentStore.Id,
                    isApproved);
                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.TimeZoneIdAttribute, model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberAttribute, model.VatNumber);

                        var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out _, out var vatAddress);
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                    if (_customerSettings.FirstNameEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                    if (_customerSettings.LastNameEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        var dateOfBirth = model.ParseDateOfBirth();
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CompanyAttribute, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CityAttribute, model.City);
                    if (_customerSettings.CountyEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountyAttribute, model.County);
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountryIdAttribute, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute,
                            model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.CellPhone);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.WorkPhone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                    

                    if (_customerSettings.AcceptPrivacyPolicyEnabled)
                    {
                        //privacy policy is required
                        //GDPR
                        if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                        {
                            _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.PrivacyPolicy"));
                        }
                    }

                   
                    //save customer attributes
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                    //login customer now
                    if (isApproved)
                        _authenticationService.SignIn(customer, true);

                    //insert default address (if possible)
                    var defaultAddress = new Address
                    {
                        FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                        LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute),
                        Email = customer.Email,
                        Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute),
                        CountryId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute) > 0
                            ? (int?)_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute)
                            : null,
                        StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute) > 0
                            ? (int?)_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute)
                            : null,
                        County = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CountyAttribute),
                        City = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CityAttribute),
                        Address1 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddressAttribute),
                        Address2 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddress2Attribute),
                        ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute),
                        PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute),
                        FaxNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FaxAttribute),
                        CreatedOnUtc = customer.CreatedOnUtc
                    };
                    if (_addressService.IsAddressValid(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        //customer.Addresses.Add(defaultAddress);

                        _addressService.InsertAddress(defaultAddress);

                        _customerService.InsertCustomerAddress(customer, defaultAddress);

                        customer.BillingAddressId = defaultAddress.Id;
                        customer.ShippingAddressId = defaultAddress.Id;

                        _customerService.UpdateCustomer(customer);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer,
                            _localizationSettings.DefaultAdminLanguageId);

                    //NSS User Registeration
                    RegisterNSSUser(model, form, customer);

                    //raise event       
                    _eventPublisher.Publish(new CustomerRegisteredEvent(customer));

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            {
                                //email validation message
                                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                                //result
                                return RedirectToRoute("RegisterResult",
                                    new { resultId = (int)UserRegistrationType.EmailValidation });
                            }
                        case UserRegistrationType.AdminApproval:
                            {
                                return RedirectToRoute("RegisterResult",
                                    new { resultId = (int)UserRegistrationType.AdminApproval });
                            }
                        case UserRegistrationType.Standard:
                            {
                                //send customer welcome message
                                _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

                                //raise event       
                                _eventPublisher.Publish(new CustomerActivatedEvent(customer));

                                return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomerOverride/Confirmation.cshtml");
                                //var redirectUrl = Url.RouteUrl("RegisterResult",
                                //    new { resultId = (int)UserRegistrationType.Standard, returnUrl }, _webHelper.CurrentRequestProtocol);
                                //return Redirect(redirectUrl);
                            }
                        default:
                            {
                                return RedirectToRoute("Homepage");
                            }
                    }
                }

                //errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareRegisterModel(model, true, customerAttributesXml);
            return View(model);
        }

     
        #endregion
        
       

    }
}
