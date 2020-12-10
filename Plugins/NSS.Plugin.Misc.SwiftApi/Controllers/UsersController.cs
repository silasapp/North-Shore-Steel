using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftApi.Delta;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.DTOs.Users;
using NSS.Plugin.Misc.SwiftApi.JSON.ActionResults;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.ModelBinders;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly WorkFlowMessageServiceOverride _workFlowMessageService;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly ICompanyService _companyService;
        private readonly CustomGenericAttributeService _genericAttributeService;
        public UsersController(IStoreContext storeContext, WorkFlowMessageServiceOverride workFlowMessageService, IUserRegistrationService userRegistrationService, ICustomerCompanyService customerCompanyService, ICompanyService companyService, CustomGenericAttributeService genericAttributeService,
            IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService, IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _userRegistrationService = userRegistrationService;
            _workFlowMessageService = workFlowMessageService;
            _customerService = customerService;
            _storeContext = storeContext;
            _customerCompanyService = customerCompanyService;
            _companyService = companyService;
            _genericAttributeService = genericAttributeService;
        }


        //[HttpGet]
        //[Route("/api/users")]
        //[ProducesResponseType(typeof(IList<UserDto>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ErrorsRootObject), 422)]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        //public IActionResult RetriveUserList()
        //{

        //}

        [HttpPost]
        [Route("/api/users")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult CreateUser([ModelBinder(typeof(JsonModelBinder<UserDto>))] Delta<UserDto> userDelta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            UserRegistration registration = null;

            if (userDelta.Dto.RegistrationId > 0)
            {
                // call user registration create
                registration = _userRegistrationService.GetById(userDelta.Dto.RegistrationId);

                if (registration == null)
                    return Error(HttpStatusCode.NotFound, "userRegistration", "not found");

                if (registration.StatusId != (int)UserRegistrationStatus.Pending)
                    return Error(HttpStatusCode.BadRequest, "userRegistration", "user registration is approved or rejected");

                if (_customerService.GetCustomerByEmail(registration.WorkEmail) != null)
                    return Error(HttpStatusCode.BadRequest, "userRegistration", "user exists");
            }

            if (userDelta.Dto.WorkEmail == null)
                return Error(HttpStatusCode.BadRequest, "user", "work email required");

            if (_customerService.GetCustomerByEmail(userDelta.Dto.WorkEmail) != null)
                return Error(HttpStatusCode.BadRequest, "user", "user exists");

            if(registration == null)
            {
                registration = new UserRegistration
                {
                    FirstName = userDelta.Dto.FirstName,
                    LastName = userDelta.Dto.LastName,
                    Cell = userDelta.Dto.Cell,
                    Phone = userDelta.Dto.Phone,
                    WorkEmail = userDelta.Dto.WorkEmail,
                    CompanyName = userDelta.Dto.CompanyName,
                    HearAboutUs = userDelta.Dto.HearAboutUs,
                    Other = userDelta.Dto.Other,
                    IsExistingCustomer = userDelta.Dto.IsExistingCustomer,
                    ItemsForNextProject = userDelta.Dto.ItemsForNextProject,
                    PreferredLocationId = userDelta.Dto.PreferredLocationId,
                    Status = UserRegistrationStatus.Approved,

                    CreatedOnUtc = DateTime.UtcNow,
                    ModifiedOnUtc = DateTime.UtcNow
                };
            }

            //generate password
            string password = Common.GenerateRandomPassword();

            var customer = _userRegistrationService.CreateCustomer(
                registration,
                password,
                userDelta.Dto.WintrixId
                );

            if (customer == null)
                return Error(HttpStatusCode.NotFound, "customer", "not created successfully");

            // create companies and associate to customer
            foreach (var userCompany in userDelta.Dto.UserCompanies)
            {
                var company = _companyService.GetCompanyEntityByErpEntityId(userCompany.CompanyId);
                if(company == null)
                {
                    company = new Company
                    {
                        ErpCompanyId = userCompany.CompanyId,
                        Name = userCompany.CompanyName,
                        SalesContactName = userCompany.SalesContactName,
                        SalesContactEmail = userCompany.SalesContactEmail,
                        SalesContactPhone = userCompany.SalesContactPhone,
                        SalesContactImageUrl = userCompany.SalesContactImageUrl,
                        CreatedOnUtc = DateTime.UtcNow, 
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    _companyService.InsertCompany(company);
                }

                var customerCompany = _customerCompanyService.GetCustomerCompany(customer.Id, company.Id);
                if(customerCompany == null)
                {
                    customerCompany = new CustomerCompany
                    {
                        CompanyId = company.Id,
                        CustomerId = customer.Id,
                        Buyer = userCompany.Buyer,
                        CanCredit = userCompany.CanCredit,
                        AP = userCompany.AP,
                        Operations = userCompany.Operations,
                    };

                    _customerCompanyService.InsertCustomerCompany(customerCompany);
                }
            }

            // send email
            _workFlowMessageService.SendCustomerWelcomeMessage(customer, password, _storeContext.CurrentStore.DefaultLanguageId);


            return Ok();
        }

        [HttpGet]
        [Route("/api/users/{id}")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult RetrieveUserDetails(int id)
        {
            int.TryParse(_genericAttributeService.GetAttributeByKeyValue(Constants.ErpKeyAttribute, id.ToString(), nameof(Nop.Core.Domain.Customers.Customer))?.Value, out int customerId);

            var customer = _customerService.GetCustomerById(customerId);

            if (customer == null)
                return Error(HttpStatusCode.BadRequest, "user", "not found.");

            var customerCompanyList = _customerCompanyService.GetCustomerCompanies(customer.Id);

            foreach (var customerCompany in customerCompanyList)
            {
                var company = _companyService.GetCompanyById(customerCompany.CompanyId);


            }


        }

        [HttpPut]
        [Route("/api/users/{id}")]
        [ProducesResponseType(typeof(CustomersRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult UpdateUser([ModelBinder(typeof(JsonModelBinder<UserDto>))] Delta<UserDto> userDelta)
        {
        }

        [HttpDelete]
        [Route("/api/users/{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult DeleteUser([ModelBinder(typeof(JsonModelBinder<UserDto>))] Delta<UserDto> userDelta)
        {
        }
    }
}
