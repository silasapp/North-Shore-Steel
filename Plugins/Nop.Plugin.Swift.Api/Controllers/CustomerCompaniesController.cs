using System.Net;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Swift.Api.Domain.Customers;
using Nop.Plugin.Swift.Api.DTOs.CustomerCompanies;
using Nop.Plugin.Swift.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class CustomerCompaniesController : BaseApiController
    {
        private readonly ICompanyService _companyService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerCompanyService _customerCompanyService;

        public CustomerCompaniesController(
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
            ICustomerCompanyService customerCompanyService) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
                 localizationService, pictureService)
        {
            _customerService = customerService;
            _companyService = companyService;
            _customerCompanyService = customerCompanyService;
        }

        [HttpPost]
        [Route("/api/users/{id}/companies")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult CreateCustomerCompany(
            int id,
            [ModelBinder(typeof(JsonModelBinder<CustomerCompaniesDto>))]
            Delta<CustomerCompaniesDto> input
            )
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            Core.Domain.Customers.Customer customer = _customerService.GetCustomerById(id);

            if (customer == null)
            {
                return NotFound();
            }

            Company company = _companyService.GetCompanyEntityById(input.Dto.CompanyId);

            if (company == null)
            {
                company = new Company
                {
                    Id = input.Dto.CompanyId,
                    Name = input.Dto.CompanyName,
                    SalesContactEmail = input.Dto.SalesContact.Email,
                    SalesContactLiveChatId = input.Dto.SalesContact.LiveChatId,
                    SalesContactName = input.Dto.SalesContact.Name,
                    SalesContactPhone = input.Dto.SalesContact.Phone
                };

                _companyService.InsertCompany(company);
            }

            CustomerCompany customerCompany = new CustomerCompany
            {
                CustomerId = customer.Id,
                CompanyId = company.Id
            };

            _customerCompanyService.InsertCustomerCompany(customerCompany);

            return Ok();
        }

        [HttpDelete]
        [Route("/api/users/{id}/companies/{companyId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult DeleteCustomerCompany(
            int id,
            int companyId
            )
        {
            CustomerCompany customerCompany =_customerCompanyService.GetCustomerCompany(id, companyId);

            if (customerCompany == null)
            {
                return NotFound();
            }

            _customerCompanyService.DeleteCustomerCompany(customerCompany);

            return Ok();
        }

    }
}
