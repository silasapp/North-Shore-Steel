using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Misc.SwiftPortalOverride;
using Nop.Plugin.Swift.Api.Domain.Customers;
using Nop.Plugin.Swift.Api.DTOs.CustomerCompanies;
using Nop.Plugin.Swift.Api.Services;
using Nop.Services.Common;
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
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;

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
            ICustomerCompanyService customerCompanyService,
            IGenericAttributeService genericAttributeService,
            ILogger logger) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
                 localizationService, pictureService)
        {
            _customerService = customerService;
            _companyService = companyService;
            _customerCompanyService = customerCompanyService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
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

            // log request
            _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, $"Swift.ApproveUser -> Customer Id: {id}", JsonConvert.SerializeObject(input));

            Core.Domain.Customers.Customer customer = _customerService.GetCustomerById(id);

            if (customer == null)
            {
                return NotFound();
            }

            Company company = _companyService.GetCompanyEntityByErpEntityId(input.Dto.CompanyId);

            if (company == null)
            {
                company = new Company
                {
                    ErpCompanyId = input.Dto.CompanyId,
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

            // update customer as NSS Approved
            _genericAttributeService.SaveAttribute(customer, SwiftPortalOverrideDefaults.NSSApprovedAttribute, true);

            #region Log Approved Status
            var approvedStatus = _genericAttributeService.GetAttribute<bool>(customer, SwiftPortalOverrideDefaults.NSSApprovedAttribute);
            // log nssapproved status
            _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, $"Swift.ApproveUser -> {customer.Email} approval status = '{approvedStatus}'");
            #endregion

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
            Company company = _companyService.GetCompanyEntityByErpEntityId(companyId);

            CustomerCompany customerCompany =_customerCompanyService.GetCustomerCompany(id, company.Id);

            if (customerCompany == null)
            {
                return NotFound();
            }

            _customerCompanyService.DeleteCustomerCompany(customerCompany);

            Core.Domain.Customers.Customer customer = _customerService.GetCustomerById(id);

            _genericAttributeService.SaveAttribute(customer, SwiftPortalOverrideDefaults.NSSApprovedAttribute, false);

            return Ok();
        }

    }
}
