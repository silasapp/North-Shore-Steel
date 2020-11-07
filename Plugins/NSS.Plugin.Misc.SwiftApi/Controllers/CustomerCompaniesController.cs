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
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftCore.Helpers;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class CustomerCompaniesController : BaseApiController
    {
        private readonly ICompanyService _companyService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerCompanyProductService _customerCompanyProductService;
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
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift.ApproveUser -> Customer Id: {id}", JsonConvert.SerializeObject(input));

            Nop.Core.Domain.Customers.Customer customer = _customerService.GetCustomerById(id);

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
                CompanyId = company.Id,
                CanCredit = input.Dto.CanCredit
            };

            CustomerCompany cc = _customerCompanyService.GetCustomerCompany(customerCompany.CustomerId, customerCompany.CompanyId);
            if (cc != null)
            {
                _customerCompanyService.UpdateCustomerCompany(customerCompany);
            } else
            {
                _customerCompanyService.InsertCustomerCompany(customerCompany);
            }


            // update customer as NSS Approved
            _genericAttributeService.SaveAttribute(customer, Constants.NSSApprovedAttribute, true);

            #region Log Approved Status
            var approvedStatus = _genericAttributeService.GetAttribute<bool>(customer, Constants.NSSApprovedAttribute);
            // log nssapproved status
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift.ApproveUser -> {customer.Email} approval status = '{approvedStatus}'");
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

            Nop.Core.Domain.Customers.Customer customer = _customerService.GetCustomerById(id);

            _genericAttributeService.SaveAttribute(customer, Constants.NSSApprovedAttribute, false);

            return Ok();
        }

    }
}
