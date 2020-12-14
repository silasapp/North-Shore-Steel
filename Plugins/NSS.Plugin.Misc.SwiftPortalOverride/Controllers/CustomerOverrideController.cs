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
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests;
using NSS.Plugin.Misc.SwiftCore.Services;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{

    public partial class CustomerOverrideController : CustomerController
    {
        #region fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerService _customerService;
        private readonly ForumSettings _forumSettings;
        private readonly IWorkContext _workContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ERPApiProvider _nSSApiProvider;
        private readonly WorkFlowMessageServiceOverride _workFlowMessageServiceOverride;
        private readonly ICountryService _countryService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerCompanyService _customerCompanyService;

        #endregion

        #region Constructor

        public CustomerOverrideController(ICustomerCompanyService customerCompanyService, AddressSettings addressSettings, CaptchaSettings captchaSettings, CustomerSettings customerSettings, DateTimeSettings dateTimeSettings, IDownloadService downloadService, ForumSettings forumSettings, GdprSettings gdprSettings, IAddressAttributeParser addressAttributeParser, IAddressModelFactory addressModelFactory, IAddressService addressService, IAuthenticationService authenticationService, ICountryService countryService, ICurrencyService currencyService, ICustomerActivityService customerActivityService, ICustomerAttributeParser customerAttributeParser, ICustomerAttributeService customerAttributeService, ICustomerModelFactory customerModelFactory, ICustomerRegistrationService customerRegistrationService, ICustomerService customerService, IEventPublisher eventPublisher, IExportManager exportManager, IExternalAuthenticationService externalAuthenticationService, IGdprService gdprService, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, ILocalizationService localizationService, ILogger logger, INewsLetterSubscriptionService newsLetterSubscriptionService, IOrderService orderService, IPictureService pictureService, IPriceFormatter priceFormatter, IProductService productService, IShoppingCartService shoppingCartService, IStateProvinceService stateProvinceService, IStoreContext storeContext, ITaxService taxService, IWebHelper webHelper, IWorkContext workContext, IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings, MediaSettings mediaSettings, StoreInformationSettings storeInformationSettings, TaxSettings taxSettings, ERPApiProvider nSSApiProvider, WorkFlowMessageServiceOverride workFlowMessageServiceOverride) : base(addressSettings, captchaSettings, customerSettings, dateTimeSettings, downloadService, forumSettings, gdprSettings, addressAttributeParser, addressModelFactory, addressService, authenticationService, countryService, currencyService, customerActivityService, customerAttributeParser, customerAttributeService, customerModelFactory, customerRegistrationService, customerService, eventPublisher, exportManager, externalAuthenticationService, gdprService, genericAttributeService, giftCardService, localizationService, logger, newsLetterSubscriptionService, orderService, pictureService, priceFormatter, productService, shoppingCartService, stateProvinceService, storeContext, taxService, webHelper, workContext, workflowMessageService, localizationSettings, mediaSettings, storeInformationSettings, taxSettings)
        {
            _customerSettings = customerSettings;
            _customerModelFactory = customerModelFactory;
            _customerService = customerService;
            _workContext = workContext;
            _authenticationService = authenticationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _dateTimeSettings = dateTimeSettings;
            _gdprSettings = gdprSettings;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerRegistrationService = customerRegistrationService;
            _eventPublisher = eventPublisher;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _storeContext = storeContext;
            _taxService = taxService;
            _webHelper = webHelper;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _taxSettings = taxSettings;
            _nSSApiProvider = nSSApiProvider;
            _workFlowMessageServiceOverride = workFlowMessageServiceOverride;
            _countryService = countryService;
            _forumSettings = forumSettings;
            _shoppingCartService = shoppingCartService;
            _customerActivityService = customerActivityService;
            _customerCompanyService = customerCompanyService;
        }

        #endregion


        #region Utilities
        protected override void ValidateRequiredConsents(List<GdprConsent> consents, IFormCollection form)
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

        protected override string ParseCustomCustomerAttributes(IFormCollection form)
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
                // prepare request for create api call

                var request = new ERPCreateUserRequest
                {
                    SwiftUserId = customer.Id.ToString(),
                    Firstname = model.FirstName,
                    LastName = model.LastName,
                    WorkEmail = model.Email,
                    Phone = model.Phone,
                    CompanyName = model.Company,
                    IsExistingCustomer = "0"
                };

                #region BuildCustomAttributes
                var attributes = _customerAttributeService.GetAllCustomerAttributes();
                buildCustomAttributes(form, request, attributes);

                #endregion

                var response = _nSSApiProvider.CreateNSSUser(request);

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


        #region Login / logout

        [HttpsRequirement]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public override IActionResult Login(bool? checkoutAsGuest)
        {
            var model = _customerModelFactory.PrepareLoginModel(checkoutAsGuest);
            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public override IActionResult Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                var loginResult = _customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerSettings.UsernamesEnabled
                                ? _customerService.GetCustomerByUsername(model.Username)
                                : _customerService.GetCustomerByEmail(model.Email);

                            bool isPassWordChanged = _genericAttributeService.GetAttribute<bool>(customer, "IsPasswordChanged");
                            if (!isPassWordChanged)
                            {
                                var changePasswordModel = _customerModelFactory.PrepareChangePasswordModel();
                                Response.Cookies.Append(SwiftPortalOverrideDefaults.NewUserEmailForPasswordChange, model.Email);
                                return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomerOverride/ChangePasswordFirstTimeLogin.cshtml", changePasswordModel);
                            }

                            //migrate shopping cart
                            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                            //sign in new customer
                            _authenticationService.SignIn(customer, model.RememberMe);

                            //raise event       
                            _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                            //activity log
                            _customerActivityService.InsertActivity(customer, "PublicStore.Login",
                                _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);

                           
                            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                return RedirectToRoute("Homepage");

                            return Redirect(returnUrl);
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                        break;
                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                        break;
                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                        break;
                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                        break;
                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                        break;
                }
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareLoginModel(model.CheckoutAsGuest);
            return View(model);
        }


        #endregion


        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public override IActionResult Register()
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
        public override IActionResult Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
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

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = _gdprService
                    .GetAllConsents().Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
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
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                                }
                            }
                            //else
                            //{
                            //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                            //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                            //}
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = model.Email,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                                }
                            }
                        }
                    }

                    if (_customerSettings.AcceptPrivacyPolicyEnabled)
                    {
                        //privacy policy is required
                        //GDPR
                        if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                        {
                            _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.PrivacyPolicy"));
                        }
                    }

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                    {
                        var consents = _gdprService.GetAllConsents().Where(consent => consent.DisplayDuringRegistration).ToList();
                        foreach (var consent in consents)
                        {
                            var controlId = $"consent{consent.Id}";
                            var cbConsent = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                            {
                                //agree
                                _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                            }
                            else
                            {
                                //disagree
                                _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                            }
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

        #region My account / Change password

        public override IActionResult ChangePassword(ChangePasswordModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    // send change password email
                    _workFlowMessageServiceOverride.SendChangePasswordEmailNotificationMessage(_workContext.CurrentCustomer, _storeContext.CurrentStore.DefaultLanguageId);
                    model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    return View(model);
                }

                //errors
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }


        [CheckAccessPublicStore(true)]
        public virtual IActionResult NewCustomerChangePassword(ChangePasswordModel model)
        {
            string newUserEmailForPasswordChange = Request.Cookies[SwiftPortalOverrideDefaults.NewUserEmailForPasswordChange].ToString();
            var customer = _customerService.GetCustomerByEmail(newUserEmailForPasswordChange);

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    // send change password email
                    //_workFlowMessageServiceOverride.SendChangePasswordEmailNotificationMessage(_workContext.CurrentCustomer, _storeContext.CurrentStore.DefaultLanguageId);
                    _genericAttributeService.SaveAttribute(customer, "IsPasswordChanged", true);
                    model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    Response.Cookies.Delete(SwiftPortalOverrideDefaults.NewUserEmailForPasswordChange);

                    //migrate shopping cart
                    _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                    //sign in new customer
                    _authenticationService.SignIn(customer, true);

                    //raise event       
                    _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                    //activity log
                    _customerActivityService.InsertActivity(customer, "PublicStore.Login",
                        _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);

                    return RedirectToRoute("Homepage");
                }

                //errors
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError("", error);
            }
            
            //If we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomerOverride/ChangePasswordFirstTimeLogin.cshtml", model);
        }


        [HttpsRequirement]
        public override IActionResult Info()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new CustomerInfoModel();
            model = _customerModelFactory.PrepareCustomerInfoModel(model, _workContext.CurrentCustomer, false);

            return View(model);
        }

        [HttpsRequirement]
        public override IActionResult AddressEdit(int addressId)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = _customerService.GetCustomerAddress(customer.Id, addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            var model = new CustomerAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            return View(model);
        }

        //[HttpsRequirement]
        //public IActionResult Notifications()
        //{
        //    var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
        //    int eRPCompanyId = Common.GetSavedERPCompanyIdFromCookies(Request.Cookies[compIdCookieKey]);

        //    if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
        //        return AccessDeniedView();

        //    return View();

        //}

        //[HttpsRequirement]
        //[IgnoreAntiforgeryToken]
        //public IActionResult UpdateNotifications([FromBody]  NSS.Plugin.Misc.SwiftPortalOverride.Models.Notification notifications)
        //{
        //    var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
        //    int eRPCompanyId = Common.GetSavedERPCompanyIdFromCookies(Request.Cookies[compIdCookieKey]);

        //    if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
        //        return AccessDeniedView();

            
        //    return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomerOverride/Notifications.cshtml", notifications);
        //}

        [HttpsRequirement]
        public override IActionResult AddressAdd()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new CustomerAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            return View(model);
        }


        [HttpPost]
        public override IActionResult Info(CustomerInfoModel model, IFormCollection form)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            string cellPhone = form["cell-phone"];
            string pLocationId = form["preferred-location-id"];
            int preferredLocationId = int.Parse(pLocationId);

            if (string.IsNullOrEmpty(cellPhone) && string.IsNullOrEmpty(model.Phone))
                ModelState.AddModelError("", "Cell or Work Phone is required");

            var oldCustomerModel = new CustomerInfoModel();

            var customer = _workContext.CurrentCustomer;

            //get customer info model before changes for gdpr log
            if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                oldCustomerModel = _customerModelFactory.PrepareCustomerInfoModel(oldCustomerModel, customer, false);

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(form);
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            //foreach (var error in customerAttributeWarnings)
            //{
            //    ModelState.AddModelError("", error);
            //}

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = _gdprService
                    .GetAllConsents().Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    int ErpId = _genericAttributeService.GetAttribute<int>(customer, Constants.ErpKeyAttribute);

                    //username 
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (!customer.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            _customerRegistrationService.SetUsername(customer, model.Username.Trim());

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                _authenticationService.SignIn(customer, true);
                        }
                    }
                    //email
                    if (!customer.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                        _customerRegistrationService.SetEmail(customer, model.Email.Trim(), requireValidation);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                _authenticationService.SignIn(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.TimeZoneIdAttribute,
                            model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.VatNumberAttribute);

                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberAttribute,
                            model.VatNumber);
                        if (prevVatNumber != model.VatNumber)
                        {
                            var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out _, out var vatAddress);
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer,
                                    model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
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
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                var wasActive = newsletter.Active;
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            else
                            {
                                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);

                    //save customer attributes
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                        LogGdpr(customer, oldCustomerModel, model, form);

                    _genericAttributeService.SaveAttribute(customer, Constants.CellAttribute, cellPhone);
                    _genericAttributeService.SaveAttribute(customer, Constants.PreferredLocationIdAttribute, preferredLocationId);

                    var request = new ERPUpdateUserRequest
                    {

                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Phone = model.Phone,
                        PreferredLocationId = 0,
                        Cell = cellPhone
                    };

                    #region BuildCustomAttributes
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
                                        {
                                            var values = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                                            var val = values.Where(x => x.Id == selectedAttributeId).FirstOrDefault();
                                            if (val != null)
                                            {
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
                                                    request.PreferredLocationId = val.Id;
                                                }

                                            }
                                        }
                                    }

                                }
                                break;
                            default:
                                break;
                        }
                    }

                    #endregion
                    
                    _nSSApiProvider.UpdateNSSUser(ErpId, request);


                    return RedirectToRoute("CustomerInfo");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareCustomerInfoModel(model, customer, true, customerAttributesXml);
            return View(model);
        }
        
        #endregion

        #endregion
        
       

    }
}
