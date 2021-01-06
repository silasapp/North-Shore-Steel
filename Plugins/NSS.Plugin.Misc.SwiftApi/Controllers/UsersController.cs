using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftApi.Delta;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.DTOs.CustomerCompanies;
using NSS.Plugin.Misc.SwiftApi.DTOs.Users;
using NSS.Plugin.Misc.SwiftApi.JSON.ActionResults;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.ModelBinders;
using NSS.Plugin.Misc.SwiftApi.Services;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Customer = Nop.Core.Domain.Customers.Customer;

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
        private readonly ICustomerApiService _customerApiService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        public UsersController(IStoreContext storeContext, WorkFlowMessageServiceOverride workFlowMessageService, ICustomerApiService customerApiService, INewsLetterSubscriptionService newsLetterSubscriptionService,
            IUserRegistrationService userRegistrationService, ICustomerCompanyService customerCompanyService, ICompanyService companyService, CustomGenericAttributeService genericAttributeService,
            IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService, IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _userRegistrationService = userRegistrationService;
            _workFlowMessageService = workFlowMessageService;
            _customerService = customerService;
            _storeContext = storeContext;
            _customerCompanyService = customerCompanyService;
            _companyService = companyService;
            _genericAttributeService = genericAttributeService;
            _customerApiService = customerApiService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
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

            if (userDelta.Dto.RegistrationId.HasValue && userDelta.Dto.RegistrationId > 0)
            {
                // call user registration create
                registration = _userRegistrationService.GetById(userDelta.Dto.RegistrationId.Value);

                if (registration == null)
                    return Error(HttpStatusCode.NotFound, "userRegistration", "not found");

                if (registration.StatusId != (int)UserRegistrationStatus.Rejected)
                    return Error(HttpStatusCode.BadRequest, "userRegistration", "user registration is rejected");

                if (_customerService.GetCustomerByEmail(registration.WorkEmail) != null)
                    return Error(HttpStatusCode.BadRequest, "userRegistration", "email is already registered");
            }

            if (userDelta.Dto.WorkEmail == null)
                return Error(HttpStatusCode.BadRequest, "user", "work email required");

            if (userDelta.Dto.WintrixId == 0)
                return Error(HttpStatusCode.BadRequest, "user", "wintrix id is required");

            if (_customerService.GetCustomerByEmail(userDelta.Dto.WorkEmail) != null)
                return Error(HttpStatusCode.BadRequest, "user", "email is already registered");

            int customerId = _genericAttributeService.GetAttributeByKeyValue(Constants.ErpKeyAttribute, userDelta.Dto.WintrixId.ToString(), nameof(Customer))?.EntityId ?? 0;
            var customer = _customerApiService.GetCustomerEntityById(customerId);

            if (customer != null)
                return Error(HttpStatusCode.BadRequest, "user", "user with same wintrix id already exists");

            if (registration == null)
            {
                registration = new UserRegistration
                {
                    FirstName = userDelta.Dto.FirstName,
                    LastName = userDelta.Dto.LastName,
                    Cell = userDelta.Dto.Cell,
                    Phone = userDelta.Dto.Phone,
                    WorkEmail = userDelta.Dto.WorkEmail,
                    //CompanyName = userDelta.Dto.CompanyName,
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

            customer = _userRegistrationService.CreateCustomer(
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

                    CustomerActivityService.InsertActivity("APIService", LocalizationService.GetResource("ActivityLog.AddNewCompany"), company);
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
                    CustomerActivityService.InsertActivity("APIService", LocalizationService.GetResource("ActivityLog.AddNewCustomerCompany"), customerCompany);
                }
            }

            // send email
            _workFlowMessageService.SendCustomerWelcomeMessage(customer, password, _storeContext.CurrentStore.DefaultLanguageId);

            CustomerActivityService.InsertActivity("APIService", LocalizationService.GetResource("ActivityLog.AddNewCustomer"), customer);

            var userDto = GetUserDto(customer);

            return new RawJsonActionResult(JsonConvert.SerializeObject(userDto));
        }

        [HttpGet]
        [Route("/api/users/{id}")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult RetrieveUserDetails(int id)
        {
            int customerId = _genericAttributeService.GetAttributeByKeyValue(Constants.ErpKeyAttribute, id.ToString(), nameof(Customer))?.EntityId ?? 0;

            var customer = _customerApiService.GetCustomerEntityById(customerId);

            if (customer == null)
                return Error(HttpStatusCode.BadRequest, "user", "not found.");

            UserDto userDto = GetUserDto(customer);

            return new RawJsonActionResult(JsonConvert.SerializeObject(userDto));
        }


        [HttpPut]
        [Route("/api/users/{id}")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult UpdateUser([ModelBinder(typeof(JsonModelBinder<UserDto>))] Delta<UserDto> userDelta)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            int customerId = _genericAttributeService.GetAttributeByKeyValue(Constants.ErpKeyAttribute, userDelta.Dto.Id.ToString(), nameof(Customer))?.EntityId ?? 0;

            // Updateting the customer
            var currentCustomer = _customerApiService.GetCustomerEntityById(customerId);

            if (currentCustomer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            UpdateCustomerGenAttribute(currentCustomer, userDelta.Dto.FirstName, userDelta.Dto.LastName, userDelta.Dto.Phone, userDelta.Dto.Cell, userDelta.Dto.PreferredLocationId);

            // update customer status
            currentCustomer.Active = userDelta.Dto.Active ?? currentCustomer.Active;

            _customerService.UpdateCustomer(currentCustomer);

            CustomerActivityService.InsertActivity("APIService", LocalizationService.GetResource("ActivityLog.UpdateCustomer"), currentCustomer);

            return NoContent();
        }

        [HttpDelete]
        [Route("/api/users/{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult DeleteUser(int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            int customerId = _genericAttributeService.GetAttributeByKeyValue(Constants.ErpKeyAttribute, id.ToString(), nameof(Customer))?.EntityId ?? 0;

            var customer = _customerApiService.GetCustomerEntityById(customerId);

            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            _customerService.DeleteCustomer(customer);

            //remove newsletter subscription (if exists)
            foreach (var store in StoreService.GetAllStores())
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                if (subscription != null)
                {
                    _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
                }
            }

            //activity log
            CustomerActivityService.InsertActivity("DeleteCustomer", LocalizationService.GetResource("ActivityLog.DeleteCustomer"), customer);

            return NoContent();
        }


        private UserDto GetUserDto(Customer customer)
        {
            var customerCompanyList = _customerCompanyService.GetCustomerCompanies(customer.Id);

            var customerAttributes = _genericAttributeService.GetAttributesForEntity(customer.Id, nameof(Customer));

            var userDto = new UserDto()
            {
                FirstName = customerAttributes.FirstOrDefault(x => x.Key == NopCustomerDefaults.FirstNameAttribute)?.Value,
                LastName = customerAttributes.FirstOrDefault(x => x.Key == NopCustomerDefaults.LastNameAttribute)?.Value,
                WorkEmail = customer.Email,
                Phone = customerAttributes.FirstOrDefault(x => x.Key == NopCustomerDefaults.PhoneAttribute)?.Value,
                Cell = customerAttributes.FirstOrDefault(x => x.Key == Constants.CellAttribute)?.Value,
                WintrixId = int.TryParse(customerAttributes.FirstOrDefault(x => x.Key == Constants.ErpKeyAttribute)?.Value, out int wintrixId) ? wintrixId : 0,
                HearAboutUs = customerAttributes.FirstOrDefault(x => x.Key == Constants.HearAboutUsAttribute)?.Value,
                Other = customerAttributes.FirstOrDefault(x => x.Key == Constants.OtherAttribute)?.Value,
                PreferredLocationId = int.TryParse(customerAttributes.FirstOrDefault(x => x.Key == Constants.PreferredLocationIdAttribute)?.Value, out int preferredLocationId) ? preferredLocationId : 0,
                IsExistingCustomer = bool.TryParse(customerAttributes.FirstOrDefault(x => x.Key == Constants.IsExistingCustomerAttribute)?.Value, out bool isExisting) ? isExisting : false,
                Active = customer.Active,
                Id = customer.Id,
                ItemsForNextProject = customerAttributes.FirstOrDefault(x => x.Key == Constants.ItemsForNextProjectAttribute)?.Value,
            };

            var userCompanies = new List<CustomerCompaniesDto>();

            foreach (var customerCompany in customerCompanyList)
            {
                userCompanies.Add(new CustomerCompaniesDto
                {
                    CompanyId = customerCompany.Company.ErpCompanyId,
                    CompanyName = customerCompany.Company.Name,
                    AP = customerCompany.AP,
                    Buyer = customerCompany.Buyer,
                    CanCredit = customerCompany.CanCredit,
                    Operations = customerCompany.Operations,
                    SalesContactName = customerCompany.Company.SalesContactName,
                    SalesContactPhone = customerCompany.Company.SalesContactPhone,
                    SalesContactEmail = customerCompany.Company.SalesContactEmail,
                    SalesContactImageUrl = customerCompany.Company.SalesContactImageUrl
                });
            }

            userDto.UserCompanies = new List<CustomerCompaniesDto>(userCompanies);
            return userDto;
        }

        private void UpdateCustomerGenAttribute(Customer customerToUpdate, string firstname, string lastName, string phone, string cell, int prefferedLocationId)
        {
            if(!string.IsNullOrEmpty(firstname))
                _genericAttributeService.SaveAttribute(customerToUpdate, NopCustomerDefaults.FirstNameAttribute, firstname);

            if (!string.IsNullOrEmpty(lastName))
                _genericAttributeService.SaveAttribute(customerToUpdate, NopCustomerDefaults.LastNameAttribute, lastName);

            if (!string.IsNullOrEmpty(phone))
                _genericAttributeService.SaveAttribute(customerToUpdate, NopCustomerDefaults.PhoneAttribute, phone);

            if (!string.IsNullOrEmpty(cell))
                _genericAttributeService.SaveAttribute(customerToUpdate, Constants.CellAttribute, cell);

            if (prefferedLocationId > 0)
                _genericAttributeService.SaveAttribute(customerToUpdate, Constants.PreferredLocationIdAttribute, prefferedLocationId);
        }
    }
}
