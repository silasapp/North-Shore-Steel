using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftApi.Attributes;
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
using System.Threading.Tasks;
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
        private readonly ILogger _logger;
        public UsersController(ILogger logger, IStoreContext storeContext, WorkFlowMessageServiceOverride workFlowMessageService, ICustomerApiService customerApiService, INewsLetterSubscriptionService newsLetterSubscriptionService,
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
            _logger = logger;
        }

        [HttpPost]
        [Route("/api/users")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> CreateUser([ModelBinder(typeof(JsonModelBinder<UserDto>))] Delta<UserDto> userDelta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - CreateUser - regId = {userDelta.Dto.RegistrationId}, wintrixId = {userDelta.Dto.WintrixId}", $"request => {JsonConvert.SerializeObject(userDelta.Dto)}");

            UserRegistration registration = null;

            if (userDelta.Dto.RegistrationId.HasValue && userDelta.Dto.RegistrationId > 0)
            {
                // call user registration create
                registration = await _userRegistrationService.GetByIdAsync(userDelta.Dto.RegistrationId.Value);

                if (registration == null)
                    return Error(HttpStatusCode.NotFound, "userRegistration", "not found.");

                if (registration.StatusId == (int)UserRegistrationStatus.Rejected)
                    return Error(HttpStatusCode.BadRequest, "userRegistration", "user registration is rejected.");

                if (_customerService.GetCustomerByEmailAsync(registration.WorkEmail) != null)
                    return Error(HttpStatusCode.BadRequest, "userRegistration", "email is already registered.");
            }

            if (userDelta.Dto.WorkEmail == null)
                return Error(HttpStatusCode.BadRequest, "user", "work email required.");

            if (userDelta.Dto.WintrixId == 0)
                return Error(HttpStatusCode.BadRequest, "user", "wintrix id is required.");

            if (_customerService.GetCustomerByEmailAsync(userDelta.Dto.WorkEmail) != null)
                return Error(HttpStatusCode.BadRequest, "user", "email is already registered.");

            int customerId = (await _genericAttributeService.GetAttributeByKeyValueAsync(Constants.ErpKeyAttribute, userDelta.Dto.WintrixId.ToString(), nameof(Customer)))?.EntityId ?? 0;
            var customer = await _customerApiService.GetCustomerEntityByIdAsync(customerId);

            if (customer != null)
                return Error(HttpStatusCode.BadRequest, "user", "user with same wintrix id already exists.");

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

            customer = await _userRegistrationService.CreateCustomerAsync(registration, password, userDelta.Dto.WintrixId);

            if (customer == null)
                return Error(HttpStatusCode.NotFound, "customer", "not created successfully");

            // create companies and associate to customer
            foreach (var userCompany in userDelta.Dto.UserCompanies)
            {
                var company = await _companyService.GetCompanyEntityByErpEntityIdAsync(userCompany.CompanyId);
                if (company == null)
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

                    await _companyService.InsertCompanyAsync(company);

                    await CustomerActivityService.InsertActivityAsync("APIService", await LocalizationService.GetResourceAsync("ActivityLog.AddNewCompany"), company);
                }

                var customerCompany = await _customerCompanyService.GetCustomerCompanyAsync(customer.Id, company.Id);
                if (customerCompany == null)
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

                    await _customerCompanyService.InsertCustomerCompanyAsync(customerCompany);
                    await CustomerActivityService.InsertActivityAsync("APIService", await LocalizationService.GetResourceAsync("ActivityLog.AddNewCustomerCompany"), customerCompany);
                }
            }

            // send email
            await _workFlowMessageService.SendCustomerWelcomeMessageAsync(customer, password, (await _storeContext.GetCurrentStoreAsync()).DefaultLanguageId);

            await CustomerActivityService.InsertActivityAsync("APIService", await LocalizationService.GetResourceAsync("ActivityLog.AddNewCustomer"), customer);

            var userDto = GetUserDtoAsync(customer);

            return new RawJsonActionResult(JsonConvert.SerializeObject(userDto));
        }

        [HttpGet]
        [Route("/api/users/{id}")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> RetrieveUserDetails(int id)
        {
            int customerId = (await _genericAttributeService.GetAttributeByKeyValueAsync(Constants.ErpKeyAttribute, id.ToString(), nameof(Customer)))?.EntityId ?? 0;

            var customer = await _customerApiService.GetCustomerEntityByIdAsync(customerId);

            if (customer == null)
                return Error(HttpStatusCode.BadRequest, "user", "not found.");

            UserDto userDto = await GetUserDtoAsync(customer);

            return new RawJsonActionResult(JsonConvert.SerializeObject(userDto));
        }


        [HttpPut]
        [Route("/api/users/{id}")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> UpdateUser([ModelBinder(typeof(JsonModelBinder<UserDto>))] Delta<UserDto> userDelta)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - UpdateUser - wintrixId = {userDelta.Dto.Id}", $"request => {JsonConvert.SerializeObject(userDelta.Dto)}");

            int customerId = (await _genericAttributeService.GetAttributeByKeyValueAsync(Constants.ErpKeyAttribute, userDelta.Dto.Id.ToString(), nameof(Customer)))?.EntityId ?? 0;

            // Updateting the customer
            var currentCustomer = await _customerApiService.GetCustomerEntityByIdAsync(customerId);
            
            if (currentCustomer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            await UpdateCustomerGenAttributeAsync(currentCustomer, userDelta.Dto.FirstName, userDelta.Dto.LastName, userDelta.Dto.Phone, userDelta.Dto.Cell, userDelta.Dto.PreferredLocationId);

            // update customer status
            currentCustomer.Active = userDelta.Dto.Active ?? currentCustomer.Active;

            await _customerService.UpdateCustomerAsync(currentCustomer);

            await CustomerActivityService.InsertActivityAsync("APIService", await LocalizationService.GetResourceAsync("ActivityLog.UpdateCustomer"), currentCustomer);

            return NoContent();
        }

        [HttpDelete]
        [Route("/api/users/{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            int customerId = (await _genericAttributeService.GetAttributeByKeyValueAsync(Constants.ErpKeyAttribute, id.ToString(), nameof(Customer)))?.EntityId ?? 0;

            var customer = await _customerApiService.GetCustomerEntityByIdAsync(customerId);

            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            await _customerService.DeleteCustomerAsync(customer);

            //remove newsletter subscription (if exists)
            foreach (var store in (await StoreService.GetAllStoresAsync()))
            {
                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                if (subscription != null)
                {
                    await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(subscription);
                }
            }

            //activity log
            await CustomerActivityService .InsertActivityAsync("DeleteCustomer", await LocalizationService.GetResourceAsync("ActivityLog.DeleteCustomer"), customer);
            
            return NoContent();
        }


        private async Task<UserDto> GetUserDtoAsync(Customer customer)
        {
            var customerCompanyList = await _customerCompanyService.GetCustomerCompaniesAsync(customer.Id);

            var customerAttributes = await _genericAttributeService.GetAttributesForEntityAsync(customer.Id, nameof(Customer));

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

        private async Task UpdateCustomerGenAttributeAsync(Customer customerToUpdate, string firstname, string lastName, string phone, string cell, int preferredLocationId)
        {
            if (!string.IsNullOrEmpty(firstname))
                await _genericAttributeService.SaveAttributeAsync(customerToUpdate, NopCustomerDefaults.FirstNameAttribute, firstname);

            if (!string.IsNullOrEmpty(lastName))
                await _genericAttributeService.SaveAttributeAsync(customerToUpdate, NopCustomerDefaults.LastNameAttribute, lastName);

            if (!string.IsNullOrEmpty(phone))
                await _genericAttributeService.SaveAttributeAsync(customerToUpdate, NopCustomerDefaults.PhoneAttribute, phone);

            if (!string.IsNullOrEmpty(cell))
                await _genericAttributeService.SaveAttributeAsync(customerToUpdate, Constants.CellAttribute, cell);

            if (preferredLocationId > 0)
                await _genericAttributeService.SaveAttributeAsync(customerToUpdate, Constants.PreferredLocationIdAttribute, preferredLocationId);
        }
    }
}
