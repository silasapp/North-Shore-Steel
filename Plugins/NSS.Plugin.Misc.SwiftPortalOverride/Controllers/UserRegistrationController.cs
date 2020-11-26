﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Tax;
using NSS.Plugin.Misc.SwiftPortalOverride.Requests;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using ICustomerModelFactory = NSS.Plugin.Misc.SwiftPortalOverride.Factories.ICustomerModelFactory;
using NSS.Plugin.Misc.SwiftCore.Services;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{

    public partial class UserRegistrationController : BasePublicController
    {
        #region fields
        private readonly CustomerSettings _customerSettings;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IUserRegistrationService _userRegistrationService;


        #endregion

        #region Constructor

        public UserRegistrationController(
            CustomerSettings customerSettings,
            IAuthenticationService authenticationService,
            ICustomerModelFactory customerModelFactory,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IStoreContext storeContext,
            IWorkContext workContext,
            IUserRegistrationService userRegistrationService
            )
        {
            _customerSettings = customerSettings;
            _authenticationService = authenticationService;
            _customerModelFactory = customerModelFactory;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
           _storeContext = storeContext;
            _workContext = workContext;
            _userRegistrationService = userRegistrationService;
        }

        #endregion


        #region Utilities
  
        #endregion


        #region Methods

        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Register()
        {
            var model = new RegisterModel();
            model = _customerModelFactory.PrepareRegisterModel(model, false, setDefaultValues: true);
            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Register(RegisterModel model, string returnUrl)
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

            var warnings = new List<string>();

            if (String.IsNullOrEmpty(model.FirstName))
                warnings.Add("Please Enter First Name");
            if (String.IsNullOrEmpty(model.LastName))
                warnings.Add("Please Enter Last Name");
            if (String.IsNullOrEmpty(model.Email))
                warnings.Add("Please Enter Work Email");
            if (String.IsNullOrEmpty(model.CellPhone))
                warnings.Add("Please Enter Cell Phone");
            if (String.IsNullOrEmpty(model.Phone))
                warnings.Add("Please Enter Work Phone");
            if (String.IsNullOrEmpty(model.Company))
                warnings.Add("Please Enter Company Name");
            if (String.IsNullOrEmpty(model.HearAboutUs.ToString()))
                warnings.Add("Please Enter How did you hear about us");
            if (String.IsNullOrEmpty(model.ItemsForNextProject))
                warnings.Add("Please Enter How we can help");
            if (String.IsNullOrEmpty(model.PreferredPickupLocationId.ToString()))
                warnings.Add("Please Enter Preferred Pickup Location");

            

            foreach (var error in warnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var userRegistrationRequest = new SwiftCore.Domain.Customers.UserRegistration
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    WorkEmail = model.Email,
                    Cell = model.CellPhone,
                    Phone = model.Phone,
                    CompanyName = model.Company,
                    IsExistingCustomer = model.IsCurrentCustomer,
                    HearAboutUs = model.HearAboutUs.ToString(),
                    Other = model.Other,
                    ItemsForNextProject = model.ItemsForNextProject,
                    PreferredLocationId = model.PreferredPickupLocationId,
                    StatusId = (int)UserRegistrationStatus.Pending,
                    CreatedOnUtc = DateTime.UtcNow,
                    ModifiedOnUtc = DateTime.UtcNow
            };

                if (!model.IsCurrentCustomer)
                {
                    userRegistrationRequest.APRole = true;
                    userRegistrationRequest.OperationsRole = true;
                    userRegistrationRequest.BuyerRole = true;
                }
                else
                {
                    userRegistrationRequest.APRole = model.APRole;
                    userRegistrationRequest.BuyerRole = model.BuyerRole;
                    userRegistrationRequest.OperationsRole = model.OperationsRole;
                }


                var registrationResult = _userRegistrationService.InsertUser(userRegistrationRequest);
                if (registrationResult.Success)
                {
                    // registration successful
                    // redirect to confirmation page
                    return View("~/Plugins/Misc.SwiftPortalOverride/Views/UserRegistration/Confirmation.cshtml", model);
                }

                // errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareRegisterModel(model, true, setDefaultValues: true);
            return View(model);
        }

     

        #endregion
        
       

    }
}
