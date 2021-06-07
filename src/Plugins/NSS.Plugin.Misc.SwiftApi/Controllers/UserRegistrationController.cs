using Microsoft.AspNetCore.Mvc;
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
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class UserRegistrationController : BaseApiController
    {
        private readonly WorkFlowMessageServiceOverride _workFlowMessageService;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly ILogger _logger;

        public UserRegistrationController(ILogger logger, WorkFlowMessageServiceOverride workFlowMessageService, IUserRegistrationService userRegistrationService, IStoreContext storeContext, IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService, IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _workFlowMessageService = workFlowMessageService;
            _customerService = customerService;
            _storeContext = storeContext;
            _userRegistrationService = userRegistrationService;
            _logger = logger;
        }

        [HttpPut]
        [Route("/api/userregistration/{id}/approve")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> ApproveUserRegistration(int id, UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // call user registration create
            var registration = await _userRegistrationService.GetByIdAsync(id);

            if (registration == null)
                return Error(HttpStatusCode.NotFound, "userRegistration", "not found");

            if (registration.StatusId != (int)UserRegistrationStatus.Pending)
                return Error(HttpStatusCode.BadRequest, "userRegistration", "user registration is approved or rejected");

            if(await _customerService.GetCustomerByEmailAsync(registration.WorkEmail) != null)
                return Error(HttpStatusCode.BadRequest, "userRegistration", "user exists");

            //generate password
            string password = Common.GenerateRandomPassword();

            var cc = await _userRegistrationService.CreateUserAsync(
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
                registrationDto.Operations,
                registrationDto.WintrixId
                );

            // get customer
            var customer = await _customerService.GetCustomerByIdAsync(cc.CustomerId);

            if (customer == null)
                return Error(HttpStatusCode.NotFound, "customer", "not created successfully");

            // send email
            await _workFlowMessageService.SendCustomerWelcomeMessageAsync(customer, password, (await _storeContext.GetCurrentStoreAsync()).DefaultLanguageId);

            return new RawJsonActionResult(JsonConvert.SerializeObject(new { swiftUserId = customer.Id, swiftCompanyId = cc.CompanyId }));
        }

        [HttpPut]
        [Route("/api/userregistration/{id}/reject")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> RejectUserRegistration(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // log request
            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - RejectUserRegistration -> regId: {id}");

            var userRegistration = await _userRegistrationService.GetByIdAsync(id);

            if (userRegistration == null)
                return Error(HttpStatusCode.NotFound, "userRegistration", "not found");

            if (userRegistration.StatusId != (int)UserRegistrationStatus.Pending)
                return Error(HttpStatusCode.BadRequest, "userRegistration", "user registration is approved or rejected");

            // call user registration reject
            await _userRegistrationService.UpdateRegisteredUserAsync(id, (int)UserRegistrationStatus.Rejected);

            // send reject email
            await _workFlowMessageService.SendNewCustomerRejectionEmailNotificationMessageAsync(userRegistration.WorkEmail, $"{userRegistration.FirstName} {userRegistration.LastName}", (await _storeContext.GetCurrentStoreAsync()).DefaultLanguageId);

            return NoContent();
        }
    }
}
