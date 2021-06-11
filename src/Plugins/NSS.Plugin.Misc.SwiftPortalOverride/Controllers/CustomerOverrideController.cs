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
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using RegisterModel = Nop.Web.Models.Customer.RegisterModel;
using Nop.Web.Extensions;
using NSS.Plugin.Misc.SwiftCore.DTOs;
using System.Threading.Tasks;
using Nop.Core.Events;
using Nop.Services.Authentication.MultiFactor;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{

    public partial class CustomerOverrideController : CustomerController
    {
        #region fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly Factories.ICustomerModelFactory _overrideCustomerModelFactory;
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
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly WorkFlowMessageServiceOverride _workFlowMessageServiceOverride;
        private readonly ICountryService _countryService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly ICompanyService _companyService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IApiService _apiService;


        #region Constructor
        public CustomerOverrideController(AddressSettings addressSettings, CaptchaSettings captchaSettings, CustomerSettings customerSettings, DateTimeSettings dateTimeSettings, IDownloadService downloadService, ForumSettings forumSettings, GdprSettings gdprSettings, Factories.ICustomerModelFactory overrideCustomerModelFactory,  IAddressAttributeParser addressAttributeParser, IAddressModelFactory addressModelFactory, IAddressService addressService, IApiService apiService, IAuthenticationService authenticationService, ICompanyService companyService, ICustomerCompanyService customerCompanyService, ICountryService countryService, ICurrencyService currencyService, ICustomerActivityService customerActivityService, ICustomerAttributeParser customerAttributeParser, ICustomerAttributeService customerAttributeService, ICustomerModelFactory customerModelFactory, ICustomerRegistrationService customerRegistrationService, ICustomerService customerService, IEventPublisher eventPublisher, IExportManager exportManager, IExternalAuthenticationService externalAuthenticationService, IGdprService gdprService, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, ILocalizationService localizationService, ILogger logger, IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager, INewsLetterSubscriptionService newsLetterSubscriptionService, INotificationService notificationService, IOrderService orderService, IShoppingCartService shoppingCartService, IPictureService pictureService, IPriceFormatter priceFormatter, IProductService productService, IStateProvinceService stateProvinceService, IStoreContext storeContext, ITaxService taxService, IWebHelper webHelper, IWorkContext workContext, IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings, MediaSettings mediaSettings, MultiFactorAuthenticationSettings multiFactorAuthenticationSettings, StoreInformationSettings storeInformationSettings, TaxSettings taxSettings, WorkFlowMessageServiceOverride workFlowMessageServiceOverride) : base(addressSettings, captchaSettings, customerSettings, dateTimeSettings, downloadService, forumSettings, gdprSettings, addressAttributeParser, addressModelFactory, addressService, authenticationService, countryService, currencyService, customerActivityService, customerAttributeParser, customerAttributeService, customerModelFactory, customerRegistrationService, customerService, eventPublisher, exportManager, externalAuthenticationService, gdprService, genericAttributeService, giftCardService, localizationService, logger, multiFactorAuthenticationPluginManager, newsLetterSubscriptionService, notificationService, orderService, pictureService, priceFormatter, productService, stateProvinceService, storeContext, taxService, workContext, workflowMessageService, localizationSettings, mediaSettings, multiFactorAuthenticationSettings, storeInformationSettings, taxSettings)
        {
            _customerSettings = customerSettings;
            _customerModelFactory = customerModelFactory;
            _overrideCustomerModelFactory = overrideCustomerModelFactory;
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
            _notificationService = notificationService;
            _storeContext = storeContext;
            _taxService = taxService;
            _webHelper = webHelper;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _taxSettings = taxSettings;
            _workFlowMessageServiceOverride = workFlowMessageServiceOverride;
            _countryService = countryService;
            _forumSettings = forumSettings;
            _shoppingCartService = shoppingCartService;
            _customerActivityService = customerActivityService;
            _customerCompanyService = customerCompanyService;
            _companyService = companyService;
            _addressAttributeParser = addressAttributeParser;
            _apiService = apiService;
        }

        #endregion

        

        #endregion


        #region Utilities

        /* _workContext.CurrentCustomer changed to _workContext.GetCurrentCustomerAsync() */
        /* _workContext.CurrentStore changed to _workContext.GetCurrentStoreAsync() */
        /* _workContext.WorkingLanguage changed to _workContext.GetWorkingLanguageAsync() */
        private async Task<int> GetERPCompanyId()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));
            return eRPCompanyId;
        }

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

        protected override async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = "";
            var attributes = await _customerAttributeService.GetAllCustomerAttributesAsync();
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
                            var attributeValues = await _customerAttributeService.GetCustomerAttributeValuesAsync(attribute.Id);
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


        protected async void RegisterNSSUser(RegisterModel model, IFormCollection form, Customer customer)
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
                var attributes = await _customerAttributeService.GetAllCustomerAttributesAsync();
                buildCustomAttributes(form, request, attributes);

                #endregion

                var response = await _apiService.CreateNSSUserAsync(request);

                if (response != null && response.WitnrixId != null)
                {
                    // save wintrix id
                    await _genericAttributeService.SaveAttributeAsync(customer, Constants.ErpKeyAttribute, response.WitnrixId);
                }

                // send nss an email
                await _workFlowMessageServiceOverride.SendNSSCustomerRegisteredNotificationMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id, response?.WitnrixId);
            }
            catch (Exception)
            {

                // silent NSS error
            }

        }

        protected async void buildCustomAttributes(IFormCollection form, ERPCreateUserRequest request, IList<CustomerAttribute> attributes)
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
                                    var values = await _customerAttributeService.GetCustomerAttributeValuesAsync(attribute.Id);
                                    var val = values.Where(x => x.Id == selectedAttributeId).FirstOrDefault();
                                    if (val != null)
                                    {
                                        if (attribute.Name == Constants.HearAboutUsAttribute)
                                            request.HearAboutUs = val.Name;
                                        if (attribute.Name == Constants.PreferredLocationIdAttribute)
                                        {
                                            if (val.Name.ToLower() == "houston")
                                            {
                                                val.Id = 2;
                                            }
                                            else if (val.Name.ToLower() == "beaumont")
                                            {
                                                val.Id = 1;
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
        public override async Task<IActionResult> Login(bool? checkoutAsGuest)
        {
            var model = await _customerModelFactory.PrepareLoginModelAsync(checkoutAsGuest);
            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public override async Task<IActionResult> Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                var loginResult = await _customerRegistrationService.ValidateCustomerAsync(_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerSettings.UsernamesEnabled
                                ? await _customerService.GetCustomerByUsernameAsync(model.Username)
                                : await _customerService.GetCustomerByEmailAsync(model.Email);

                            bool isPassWordChanged = await _genericAttributeService.GetAttributeAsync<bool>(customer, "IsPasswordChanged");
                            if (!isPassWordChanged)
                            {
                                var changePasswordModel = await _customerModelFactory.PrepareChangePasswordModelAsync();
                                Response.Cookies.Append(SwiftPortalOverrideDefaults.NewUserEmailForPasswordChange, model.Email);
                                await _genericAttributeService.SaveAttributeAsync(customer, SwiftPortalOverrideDefaults.OldPassword, model.Password);
                                return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomerOverride/ChangePasswordFirstTimeLogin.cshtml", changePasswordModel);
                            }

                            //migrate shopping cart
                            await _shoppingCartService.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), customer, true);

                            //sign in new customer
                            await _authenticationService.SignInAsync(customer, model.RememberMe);

                            //raise event       
                            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                            //activity log
                            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                               await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);


                            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                return RedirectToRoute("Homepage");

                            return Redirect(returnUrl);
                        }
                    default:
                        ModelState.AddModelError("", "");
                        break;
                }
            }

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareLoginModelAsync(model.CheckoutAsGuest);
            return View(model);
        }


        #endregion


        #region My account / Register
        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public override async Task<IActionResult> Register(string returnUrl)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            var model = new RegisterModel();
            model = await _customerModelFactory.PrepareRegisterModelAsync(model, false, setDefaultValues: true);

            //For view give full path of your published plugin
            return View(model);

        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public override async Task<IActionResult> Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            if (await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                //Already registered customer. 
                await _authenticationService.SignOutAsync();

                //raise logged out event       
                await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(await _workContext.GetCurrentCustomerAsync()));

                //Save a new record
                /* await _workContext.CurrentCustomer = await _customerService.InsertGuestCustomer(); */
                await _workContext.SetCurrentCustomerAsync(await _customerService.InsertGuestCustomerAsync());
            }
            var customer = await _workContext.GetCurrentCustomerAsync();
            customer.RegisteredInStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                    .GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

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
                    (await _storeContext.GetCurrentStoreAsync()).Id,
                    isApproved);
                var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.TimeZoneIdAttribute, model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.VatNumberAttribute, model.VatNumber);

                        //var vatNumberStatus = await _taxService.GetVatNumberStatusAsync(model.VatNumber, out _, out var vatAddress);
                        var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                    if (_customerSettings.FirstNameEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                    if (_customerSettings.LastNameEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        var dateOfBirth = model.ParseDateOfBirth();
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CompanyAttribute, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CityAttribute, model.City);
                    if (_customerSettings.CountyEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CountyAttribute, model.County);
                    if (_customerSettings.CountryEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CountryIdAttribute, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.StateProvinceIdAttribute,
                            model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(model.Email, (await _storeContext.GetCurrentStoreAsync()).Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
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
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = model.Email,
                                    Active = true,
                                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
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
                            await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
                        }
                    }

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                    {
                        var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
                        foreach (var consent in consents)
                        {
                            var controlId = $"consent{consent.Id}";
                            var cbConsent = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                            {
                                //agree
                                await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                            }
                            else
                            {
                                //disagree
                                await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                            }
                        }
                    }

                    //save customer attributes
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                    //login customer now
                    if (isApproved)
                        await _authenticationService.SignInAsync(customer, true);

                    //insert default address (if possible)
                    var defaultAddress = new Address
                    {
                        FirstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                        LastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute),
                        Email = customer.Email,
                        Company = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CompanyAttribute),
                        CountryId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.CountryIdAttribute) > 0
                            ? (int?) await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.CountryIdAttribute)
                            : null,
                        StateProvinceId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute) > 0
                            ? (int?) await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute)
                            : null,
                        County = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CountyAttribute),
                        City = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CityAttribute),
                        Address1 = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.StreetAddressAttribute),
                        Address2 = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.StreetAddress2Attribute),
                        ZipPostalCode = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute),
                        PhoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute),
                        FaxNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FaxAttribute),
                        CreatedOnUtc = customer.CreatedOnUtc
                    };
                    if (await _addressService.IsAddressValidAsync(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        //customer.Addresses.Add(defaultAddress);

                        await _addressService.InsertAddressAsync(defaultAddress);

                        await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                        customer.BillingAddressId = defaultAddress.Id;
                        customer.ShippingAddressId = defaultAddress.Id;

                        await _customerService.UpdateCustomerAsync(customer);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        await _workflowMessageService.SendCustomerRegisteredNotificationMessageAsync(customer,
                            _localizationSettings.DefaultAdminLanguageId);

                    //NSS User Registeration
                    RegisterNSSUser(model, form, customer);

                    //raise event       
                    await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            {
                                //email validation message
                                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

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
                                await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

                                //raise event       
                                await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

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
            model = await _customerModelFactory.PrepareRegisterModelAsync(model, true, customerAttributesXml);
            return View(model);
        }

        #endregion


        #region My account / Change password

        public override async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!(await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync())))
                return Challenge();

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    // send change password email
                    await _workFlowMessageServiceOverride.SendChangePasswordEmailNotificationMessageAsync(await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).DefaultLanguageId);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.ChangePassword.Success"));
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
        public virtual async Task<IActionResult> NewCustomerChangePassword(ChangePasswordModel model)
        {
            var newUserEmail = Request.Cookies[SwiftPortalOverrideDefaults.NewUserEmailForPasswordChange];
            Customer customer = new Customer();
            if(!string.IsNullOrEmpty(newUserEmail))
                customer = await _customerService.GetCustomerByEmailAsync(newUserEmail);

            if (ModelState.IsValid)
            {
                var oldPassword = await _genericAttributeService.GetAttributeAsync<string>(customer, SwiftPortalOverrideDefaults.OldPassword);
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, oldPassword);
                var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    // send change password email
                    //_workFlowMessageServiceOverride.SendChangePasswordEmailNotificationMessage(_workContext.CurrentCustomer, _storeContext.CurrentStore.DefaultLanguageId);
                    await _genericAttributeService.SaveAttributeAsync(customer, "IsPasswordChanged", true);
                    Response.Cookies.Delete(SwiftPortalOverrideDefaults.NewUserEmailForPasswordChange);

                    //migrate shopping cart
                    await _shoppingCartService.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), customer, true);

                    //sign in new customer
                    await _authenticationService.SignInAsync(customer, true);

                    //raise event       
                    await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                    //activity log
                    await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                        await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

                    await _genericAttributeService.SaveAttributeAsync(customer, SwiftPortalOverrideDefaults.OldPassword, "");
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.ChangePassword.Success"));
                    return RedirectToRoute("Homepage");
                }

                //errors
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomerOverride/ChangePasswordFirstTimeLogin.cshtml", model);
        }

        #endregion

        #region My account / Info
        [HttpsRequirement]
        public override async Task<IActionResult> Info()
        {
            if (! await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var model = new CustomerInfoModel();
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, await _workContext.GetCurrentCustomerAsync(), false);

            return View(model);
        }

        [HttpPost]
        public override async Task<IActionResult> Info(CustomerInfoModel model, IFormCollection form)
        {
            if (!(await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync())))
                return Challenge();

            string cellPhone = form["cell-phone"];
            int.TryParse(form["preferred-location-id"], out int preferredLocationId);

            if (string.IsNullOrEmpty(cellPhone) && string.IsNullOrEmpty(model.Phone))
                ModelState.AddModelError("", "Cell or Work Phone is required.");

            var oldCustomerModel = new CustomerInfoModel();

            var customer = await _workContext.GetCurrentCustomerAsync();

            //get customer info model before changes for gdpr log
            if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                oldCustomerModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);


            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                    .GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    int erpId = await _genericAttributeService.GetAttributeAsync<int>(customer, Constants.ErpKeyAttribute);

                    //username 
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (!customer.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            await _customerRegistrationService.SetUsernameAsync(customer, model.Username.Trim());

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }
                    //email
                    if (!customer.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                        await _customerRegistrationService.SetEmailAsync(customer, model.Email.Trim(), requireValidation);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.TimeZoneIdAttribute,
                            model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.VatNumberAttribute);

                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.VatNumberAttribute,
                            model.VatNumber);
                        if (prevVatNumber != model.VatNumber)
                        {
                            var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                    model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                    if (_customerSettings.FirstNameEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                    if (_customerSettings.LastNameEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        var dateOfBirth = model.ParseDateOfBirth();
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CompanyAttribute, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CityAttribute, model.City);
                    if (_customerSettings.CountyEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CountyAttribute, model.County);
                    if (_customerSettings.CountryEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CountryIdAttribute, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.StateProvinceIdAttribute, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, (await _storeContext.GetCurrentStoreAsync()).Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                var wasActive = newsletter.Active;
                                newsletter.Active = true;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                            }
                            else
                            {
                                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);

                    //save customer attributes
                    await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                        NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                        await LogGdprAsync(customer, oldCustomerModel, model, form);

                    await _genericAttributeService.SaveAttributeAsync(customer, Constants.CellAttribute, cellPhone);
                    await _genericAttributeService.SaveAttributeAsync(customer, Constants.PreferredLocationIdAttribute, preferredLocationId);

                    var request = new ERPUpdateUserRequest
                    {

                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Phone = model.Phone,
                        PreferredLocationId = preferredLocationId,
                        Cell = cellPhone
                    };

                    #region BuildCustomAttributes
                    var attributes = await _customerAttributeService.GetAllCustomerAttributesAsync();
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
                                            var values = await _customerAttributeService.GetCustomerAttributeValuesAsync(attribute.Id);
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

                    await _apiService.UpdateNSSUserAsync(erpId, request);


                    return RedirectToRoute("CustomerInfo");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, true, customerAttributesXml);
            return View(model);
        }

        #endregion

        #region My account / Address

        [HttpsRequirement]
        public override async Task<IActionResult> Addresses()
        {
            if (!(await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync())))
                return Challenge();
            int eRPCompanyId = await GetERPCompanyId();

            bool isBuyer = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer);

            if (!isBuyer)
                return AccessDeniedView();

            var model = await _overrideCustomerModelFactory.PrepareCustomerAddressListModelAsync(eRPCompanyId);
            return View(model);
        }

        [HttpsRequirement]
        public override async Task<IActionResult> AddressEdit(int addressId)
        {
            if (!(await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync())))
                return Challenge();

            var customer = await _workContext.GetCurrentCustomerAsync();
            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            var model = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            return View(model);
        }

        [HttpsRequirement]
        public override async Task<IActionResult> AddressAdd()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var model = new CustomerAddressEditModel();
            model.Address.CountryId = 1;
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            return View(model);
        }

        [HttpPost]
        public override async Task<IActionResult> AddressAdd(CustomerAddressEditModel model, IFormCollection form)
        {
            if (!(await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync())))
                return Challenge();

            //custom address attributes
            var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;


                await _addressService.InsertAddressAsync(address);
                await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(), address);


                SwiftCore.Domain.Customers.Company company;
                string companyAddress;
                (company, companyAddress) = await GetERPCompanyIdAndAddressKeyAsync(address);
                await _genericAttributeService.SaveAttributeAsync<int>(company, companyAddress, address.Id);

                return RedirectToRoute("CustomerAddresses");
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: null,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                overrideAttributesXml: customAttributes);

            return View(model);
        }

        [HttpPost]
        [HttpsRequirement]
        public override async Task<IActionResult> AddressDelete(int addressId)
        {
            if (!(await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync())))
                return Challenge();

            
            var customer = await _workContext.GetCurrentCustomerAsync();

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address != null)
            {
                await _customerService.RemoveCustomerAddressAsync(customer, address);
                await _customerService.UpdateCustomerAsync(customer);
                //now delete the address record
                await _addressService.DeleteAddressAsync(address);

                var (company, companyAddress) = await GetERPCompanyIdAndAddressKeyAsync(address);
                await _genericAttributeService.SaveAttributeAsync<string>(company, companyAddress, "");
            }

            //redirect to the address list page
            return Json(new
            {
                redirect = Url.RouteUrl("CustomerAddresses"),
            });
        }

        private async Task<(SwiftCore.Domain.Customers.Company, string)> GetERPCompanyIdAndAddressKeyAsync(Address address)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            var company = await _companyService.GetCompanyEntityByErpEntityIdAsync(eRPCompanyId);
            var companyAddress = string.Format(SwiftPortalOverrideDefaults.CompanyAddressKey, address.Id);

            return (company, companyAddress);
        }

        #endregion

        #region My account / Notification

        [HttpsRequirement]
        public async Task<IActionResult> Notifications()
        {
            int eRPCompanyId = await GetERPCompanyId();

            bool isBuyer = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer);
            bool isOperations = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Operations);

            if (!isOperations && !isBuyer)
                return AccessDeniedView();

            return View();

        }

        [HttpsRequirement]
        public async Task<IActionResult> GetNotifications()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            bool isBuyer = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer);
            bool isOperations = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Operations);

            if (!isOperations && !isBuyer)
                return AccessDeniedView();

            // call api
            var custNo = await _genericAttributeService.GetAttributeAsync<int>(await _workContext.GetCurrentCustomerAsync(), Constants.ErpKeyAttribute);
            var (result, error) = await _apiService.GetCompanyNotificationPreferencesAsync(custNo, eRPCompanyId);

            var model = await _overrideCustomerModelFactory.PrepareNotificationsModelAsync(eRPCompanyId, error, result);

            return Json(new { model });
        }



        [HttpsRequirement]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateNotifications([FromBody] NotificationsModel.NotificationUpdateModel notificationRequest)
        {
            if (notificationRequest == null)
                throw new ArgumentNullException(nameof(notificationRequest));

            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));
            bool isBuyer = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer);
            bool isOperations = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Operations);

            if (!isOperations && !isBuyer)
                return AccessDeniedView();

            // call api
            var custNo = await _genericAttributeService.GetAttributeAsync<int>(await _workContext.GetCurrentCustomerAsync(), Constants.ErpKeyAttribute);
            var preferences = new Dictionary<string, bool>();

            if (notificationRequest.Preferences != null)
            {
                foreach (var item in notificationRequest.Preferences)
                {
                    preferences.Add(item.Key, item.Value);
                }
            }

            var (result, error) = await _apiService.UpdateCompanyNotificationPreferencesAsync(custNo, eRPCompanyId, preferences);

            var model = await _overrideCustomerModelFactory.PrepareNotificationsModelAsync(eRPCompanyId, error, result);

            //return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomerOverride/Notifications.cshtml", model);

            return Json(new { model });
        }

        #endregion


        #endregion
    }
}
