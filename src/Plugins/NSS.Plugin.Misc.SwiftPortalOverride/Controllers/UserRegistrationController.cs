﻿using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.DTOs;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using ICustomerModelFactory = NSS.Plugin.Misc.SwiftPortalOverride.Factories.ICustomerModelFactory;
using System.Threading.Tasks;
using Nop.Core.Events;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{

    public partial class UserRegistrationController : BasePublicController
    {
        #region Fields
        private readonly CustomerSettings _customerSettings;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly WorkFlowMessageServiceOverride _workflowMessageService;
        private readonly SwiftCoreSettings _swiftCoreSettings;
        private readonly IApiService _apiService;
        #endregion

        #region Ctor
        public UserRegistrationController(
            CustomerSettings customerSettings,
            IAuthenticationService authenticationService,
            ICustomerModelFactory customerModelFactory,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IStoreContext storeContext,
            IWorkContext workContext,
            IUserRegistrationService userRegistrationService,
            WorkFlowMessageServiceOverride workflowMessageService,
            SwiftCoreSettings swiftCoreSettings,
            IApiService apiService
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
            _workflowMessageService = workflowMessageService;
            _workflowMessageService = workflowMessageService;
            _swiftCoreSettings = swiftCoreSettings;
            _apiService = apiService;
        }

        #endregion


        #region Utilities

        #endregion


        #region Methods

        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> Register()
        {
            var model = new RegisterModel();
            model = await _customerModelFactory.PrepareRegisterModelAsync(model, false, setDefaultValues: true);
            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> Register(RegisterModel model, string returnUrl)
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
                /***** _workContext.CurrentCustomer changed to _workContext.SetCurrentCustomerAsync() *****/
                await _workContext.SetCurrentCustomerAsync(await _customerService.InsertGuestCustomerAsync());
            }
            var customer = await _workContext.GetCurrentCustomerAsync();
            customer.RegisteredInStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var warnings = new List<string>();
            if(!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.ConfirmEmail))
            {
                if (model.Email.ToLower() != model.ConfirmEmail.ToLower())
                    warnings.Add("Emails does not match.");
            }
            if (String.IsNullOrEmpty(model.FirstName))
                warnings.Add("Please enter First Name.");
            if (String.IsNullOrEmpty(model.LastName))
                warnings.Add("Please enter Last Name.");
            if (String.IsNullOrEmpty(model.Email))
                warnings.Add("Please enter Work Email.");
            if (String.IsNullOrEmpty(model.Phone))
                warnings.Add("Please enter Work Phone.");
            if (String.IsNullOrEmpty(model.Company))
                warnings.Add("Please enter Company Name.");



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
                    IsExistingCustomer = model.IsExistingCustomer,
                    HearAboutUs = model.HearAboutUs.ToString(),
                    Other = model.Other,
                    ItemsForNextProject = model.ItemsForNextProject,
                    PreferredLocationId = model.PreferredPickupLocationId,
                    StatusId = (int)UserRegistrationStatus.Pending,
                    CreatedOnUtc = DateTime.UtcNow,
                    ModifiedOnUtc = DateTime.UtcNow
                };

                userRegistrationRequest.APRole = model.IsExistingCustomer ? model.APRole : true;
                userRegistrationRequest.OperationsRole = model.IsExistingCustomer ? model.OperationsRole : true;
                userRegistrationRequest.BuyerRole = model.IsExistingCustomer ? model.BuyerRole : true;


                var registrationResult = await _userRegistrationService.InsertUserAsync(userRegistrationRequest);
                if (registrationResult.Item1.Success)
                {
                    // prepare request for create api create user registration call
                    var res = registrationResult.Item2;
                    var request = new ERPRegisterUserRequest
                    {
                        SwiftRegistrationId = res.Id,
                        FirstName = res.FirstName,
                        LastName = res.LastName,
                        WorkEmail = res.WorkEmail,
                        Cell = res.Cell,
                        Phone = res.Phone,
                        CompanyName = res.CompanyName,
                        IsExistingCustomer = res.IsExistingCustomer,
                        PreferredLocationId = res.PreferredLocationId,
                        HearAboutUs = res.HearAboutUs,
                        Other = res.Other,
                        ItemsForNextProject = res.ItemsForNextProject,
                        CreatedOnUtc = res.CreatedOnUtc.ToString("s")
                    };

                    request.AP = res.IsExistingCustomer ? res.APRole : (bool?)null;
                    request.Buyer = res.IsExistingCustomer ? res.BuyerRole : (bool?)null;
                    request.Operations = res.IsExistingCustomer ? res.OperationsRole : (bool?)null;

                    var response = await _apiService.CreateUserRegistrationAsync(request);
                    // check if error in response
                    // return error to screen


                    // registration successful
                    await _workflowMessageService.SendNewCustomerPendingApprovalEmailNotificationMessageAsync(res.WorkEmail, $"{res.FirstName} {res.LastName}", res.IsExistingCustomer, (await _storeContext.GetCurrentStoreAsync()).DefaultLanguageId);
                    await _workflowMessageService.SendNSSCustomerRegisteredNotificationMessageAsync(res.Id, res.WorkEmail, $"{res.FirstName} {res.LastName}", res.IsExistingCustomer, (await _storeContext.GetCurrentStoreAsync()).DefaultLanguageId);

                    // redirect to confirmation page
                    model.MarketingVideoUrl = _swiftCoreSettings.MarketingVideoUrl;
                    return View("~/Plugins/Misc.SwiftPortalOverride/Views/UserRegistration/Confirmation.cshtml", model);
                }

                // errors
                foreach (var error in registrationResult.Item1.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareRegisterModelAsync(model, true, setDefaultValues: true);
            return View(model);
        }


        public virtual async Task<IActionResult> ConfirmRegistration(int regId)
        {
            UserRegistration model = await GetRegisteredUser(regId);
            return View(model);
        }

        public virtual async Task<IActionResult> Approve(int regId)
        {
            UserRegistration userRegistration = await GetRegisteredUser(regId);
            if(userRegistration.StatusId != 0)
            {
                return View("~/Plugins/Misc.SwiftPortalOverride/Views/UserRegistration/ConfirmRegistration.cshtml", userRegistration);
            }

            var warnings = new List<string>();

            // check model values
            //if (_customerService.IsRegistered(customer))
            //{
            //    warnings.Add("Current customer is already registered");
            //    //return Error();
            //}

            if (!CommonHelper.IsValidEmail(userRegistration.WorkEmail))
            {
                warnings.Add("Email is not valid");
                //return;
            }

            //validate unique user
            if (await _customerService.GetCustomerByEmailAsync(userRegistration.WorkEmail) != null)
            {
                warnings.Add("Email Already Exists");
                //return;
            }

            foreach (var error in warnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                // approve user from nss
                var (response, error) = await _apiService.ApproveUserRegistrationAsync(regId);

                if (!string.IsNullOrEmpty(error))
                {
                    ModelState.AddModelError("", error);
                    return View("~/Plugins/Misc.SwiftPortalOverride/Views/UserRegistration/ConfirmRegistration.cshtml", userRegistration);
                }

            }


            // update user state and modified state 
            await _userRegistrationService.UpdateRegisteredUserAsync(regId, (int)UserRegistrationStatus.Approved);

            userRegistration = await GetRegisteredUser(regId);
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/UserRegistration/ConfirmRegistration.cshtml", userRegistration);

        }

        public virtual async Task<IActionResult> Reject(int regId)
        {

            var userRegistration = await GetRegisteredUser(regId);
            if (userRegistration.StatusId != 0)
            {
                return View("~/Plugins/Misc.SwiftPortalOverride/Views/UserRegistration/ConfirmRegistration.cshtml", userRegistration);
            }

            var response = await _apiService.RejectUserRegistrationAsync(regId);
            // update user state and modified state 
            await _userRegistrationService.UpdateRegisteredUserAsync(regId, (int)UserRegistrationStatus.Rejected);

            // send reject email
            await _workflowMessageService.SendNewCustomerRejectionEmailNotificationMessageAsync(userRegistration.WorkEmail, $"{userRegistration.FirstName} {userRegistration.LastName}", (await _storeContext.GetCurrentStoreAsync()).DefaultLanguageId);

            userRegistration = await GetRegisteredUser(regId);
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/UserRegistration/ConfirmRegistration.cshtml", userRegistration);
        }

        #endregion

        private async Task<UserRegistration> GetRegisteredUser(int regId)
        {
            UserRegistration model = await _userRegistrationService.GetByIdAsync(regId);
            return model;
        }

    }
}
