﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftApi.Attributes;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.DTOs.UserRegistration;
using NSS.Plugin.Misc.SwiftApi.JSON.ActionResults;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class UserRegistrationController : BaseApiController
    {
        private readonly WorkFlowMessageServiceOverride _workFlowMessageService;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IUserRegistrationService _userRegistrationService;
        public UserRegistrationController(WorkFlowMessageServiceOverride workFlowMessageService, IUserRegistrationService userRegistrationService, IStoreContext storeContext, IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService, IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _workFlowMessageService = workFlowMessageService;
            _customerService = customerService;
            _storeContext = storeContext;
            _userRegistrationService = userRegistrationService;
        }

        [HttpPut]
        [Route("/api/userregistration/{id}/approve")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult ApproveUserRegistration(int id, UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            //generate password
            var password = "pass$$123word";

            // call user registration create
            var registration = _userRegistrationService.GetUserById(id);
            var cc = _userRegistrationService.CreateUser(
                registration, 
                password, 
                (int)UserRegistrationStatus.Approved, 
                registrationDto.CompanyId, 
                registrationDto.CompanyName, 
                registrationDto.SalesContactEmail, 
                registrationDto.SalesContactName, 
                registrationDto.SalesContactPhone,
                registrationDto.Ap,
                registrationDto.Buyer,
                registrationDto.Operations);

            // get customer
            var customer = _customerService.GetCustomerById(cc.CustomerId);

            // send email
            _workFlowMessageService.SendCustomerWelcomeMessage(customer, _storeContext.CurrentStore.DefaultLanguageId);

            return new RawJsonActionResult(JsonConvert.SerializeObject(new { swiftUserId = customer.Id, companyId = cc.CompanyId }));
        }

        [HttpPut]
        [Route("/api/userregistration/{id}/reject")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult RejectUserRegistration(int id, UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userRegistration = _userRegistrationService.GetUserById(id);

            // call user registration reject
            _userRegistrationService.UpdateRegisteredUser(id, (int)UserRegistrationStatus.Rejected);

            // send reject email
            _workFlowMessageService.SendNewCustomerRejectionEmailNotificationMessage(userRegistration.WorkEmail, $"{userRegistration.FirstName} {userRegistration.LastName}", _storeContext.CurrentStore.DefaultLanguageId);

            return Ok();
        }
    }
}