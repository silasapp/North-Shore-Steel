using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.Attributes;
using NSS.Plugin.Misc.SwiftApi.Delta;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.ModelBinders;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftApi.DTOs.CustomerCompanies;
using NSS.Plugin.Misc.SwiftApi.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using System;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class CustomerCompaniesController : BaseApiController
    {
        private readonly ICompanyService _companyService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly CustomGenericAttributeService _genericAttributeService;
        private readonly ICustomerCompanyProductService _customerCompanyProductService;
        private readonly ILogger _logger;
        private readonly ICustomerApiService _customerApiService;

        public CustomerCompaniesController(
            ICustomerApiService customerApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ICompanyService companyService,
            ICustomerCompanyService customerCompanyService,
            CustomGenericAttributeService genericAttributeService,
            ICustomerCompanyProductService customerCompanyProductService,
            ILogger logger) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
                 localizationService, pictureService)
        {
            _customerService = customerService;
            _companyService = companyService;
            _customerCompanyService = customerCompanyService;
            _genericAttributeService = genericAttributeService;
            _customerCompanyProductService = customerCompanyProductService;
            _logger = logger;
            _customerApiService = customerApiService;
        }

        [HttpPost]
        [Route("/api/users/{id}/companies")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> CreateCustomerCompany(
            int id,
            [ModelBinder(typeof(JsonModelBinder<CustomerCompaniesDto>))]
            Delta<CustomerCompaniesDto> input
            )
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            // log request
            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - CreateCustomerCompany -> wintrix Id: {id}", $"request => {JsonConvert.SerializeObject(input.Dto)}");

            int customerId = (await _genericAttributeService.GetAttributeByKeyValueAsync(Constants.ErpKeyAttribute, id.ToString(), nameof(Customer)))?.EntityId ?? 0;

            Nop.Core.Domain.Customers.Customer customer = await _customerApiService.GetCustomerEntityByIdAsync(customerId);

            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "userCompany", "user not found");
            }

            Company company = await _companyService.GetCompanyEntityByErpEntityIdAsync(input.Dto.CompanyId);

            if (company == null)
            {
                company = new Company
                {
                    ErpCompanyId = input.Dto.CompanyId,
                    Name = input.Dto.CompanyName,
                    SalesContactEmail = input.Dto.SalesContactEmail,
                    SalesContactName = input.Dto.SalesContactName,
                    SalesContactPhone = input.Dto.SalesContactPhone,
                    SalesContactImageUrl = input.Dto.SalesContactImageUrl,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                };

                await _companyService.InsertCompanyAsync(company);
            }

            CustomerCompany customerCompany = new CustomerCompany
            {
                CompanyId = company.Id,
                CustomerId = customer.Id,
                Buyer = input.Dto.Buyer,
                CanCredit = input.Dto.CanCredit,
                AP = input.Dto.AP,
                Operations = input.Dto.Operations,
            };

            CustomerCompany cc = await _customerCompanyService.GetCustomerCompanyAsync(customerCompany.CustomerId, customerCompany.CompanyId);
            if (cc != null)
            {
                cc.AP = customerCompany.AP;
                cc.Buyer = customerCompany.Buyer;
                cc.CanCredit = customerCompany.CanCredit;
                cc.Operations = customerCompany.Operations;

                await _customerCompanyService.UpdateCustomerCompanyAsync(cc);
            }
            else
            {
                await _customerCompanyService.InsertCustomerCompanyAsync(customerCompany);
            }

            return Ok();
        }

        [HttpDelete]
        [Route("/api/users/{id}/companies/{companyId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> DeleteCustomerCompany(
            int id,
            int companyId
            )
        {

            // log request
            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - DeleteCustomerCompany -> wintrixId: {id}, erpCompId: {companyId}");

            int customerId = (await _genericAttributeService.GetAttributeByKeyValueAsync(Constants.ErpKeyAttribute, id.ToString(), nameof(Customer)))?.EntityId ?? 0;

            Company company = await _companyService.GetCompanyEntityByErpEntityIdAsync(companyId);

            CustomerCompany customerCompany = await _customerCompanyService.GetCustomerCompanyAsync(customerId, company?.Id ?? 0);

            if (customerCompany == null)
            {
                return Error(HttpStatusCode.NotFound, "userCompany", "not found");
            }

            await _customerCompanyService.DeleteCustomerCompanyAsync(customerCompany);

            return Ok();
        }

    }
}
