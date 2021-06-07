using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftApi.Delta;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.DTOs.Companies;
using NSS.Plugin.Misc.SwiftApi.JSON.ActionResults;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.MappingExtensions;
using NSS.Plugin.Misc.SwiftApi.ModelBinders;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class CompaniesController : BaseApiController
    {
        private readonly ICompanyService _companyService;
        private readonly ILogger _logger;
        public CompaniesController(
            ILogger logger,
            ICompanyService companyService,
            IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService, IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _companyService = companyService;
            _logger = logger;
        }

        [HttpGet]
        [Route("/api/companies")]
        [ProducesResponseType(typeof(IList<CompanyDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> RetrieveCompanyList()
        {
            var companiesDto = new List<CompanyDto>();

            var companyList = await _companyService.GetCompanyListAsync();

            foreach (var company in companyList)
            {
                companiesDto.Add(company.ToDto());
            }

            return new RawJsonActionResult(JsonConvert.SerializeObject(companiesDto));
        }

        [HttpPost]
        [Route("/api/companies")]
        [ProducesResponseType(typeof(IList<CompanyDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateCompany([ModelBinder(typeof(JsonModelBinder<CompanyDto>))] Delta<CompanyDto> companyDelta)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - CreateCompany - erpCompId = {companyDelta.Dto.ErpCompanyId}", $"request => {JsonConvert.SerializeObject(companyDelta.Dto)}");

            var newCompany = await _companyService.GetCompanyEntityByErpEntityIdAsync(companyDelta.Dto.ErpCompanyId);

            if (newCompany != null)
            {
                return Error(HttpStatusCode.BadRequest, "company", "duplicate record found.");
            }

            newCompany = new Company { CreatedOnUtc = DateTime.UtcNow, UpdatedOnUtc =  DateTime.UtcNow};

            companyDelta.Merge(newCompany);

            await _companyService.InsertCompanyAsync(newCompany);

            var companyDto = newCompany.ToDto();

            return new RawJsonActionResult(JsonConvert.SerializeObject(companyDto));
        }

        [HttpPut]
        [Route("/api/companies/{id}")]
        [ProducesResponseType(typeof(CompanyDto),(int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> UpdateCompany([ModelBinder(typeof(JsonModelBinder<CompanyDto>))] Delta<CompanyDto> companyDelta)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - UpdateCompany - erpCompId = {companyDelta.Dto.Id}", $"request => {JsonConvert.SerializeObject(companyDelta.Dto)}");

            var companyToUpdate = await _companyService.GetCompanyEntityByErpEntityIdAsync(companyDelta.Dto.Id);

            if(companyToUpdate == null)
            {
                return Error(HttpStatusCode.NotFound, "company", "not found");
            }

            companyDelta.Dto.Id = companyToUpdate.Id;

            companyDelta.Merge(companyToUpdate);

            // reset id
            companyToUpdate.Id = companyDelta.Dto.Id;

            await _companyService.UpdateCompanyAsync(companyToUpdate);

            var companyDto = companyToUpdate.ToDto();

            return new RawJsonActionResult(JsonConvert.SerializeObject(companyDto));
        }

        [HttpGet]
        [Route("/api/companies/{id}")]
        [ProducesResponseType(typeof(CompanyDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> RetrieveCompanyDetails(int id)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var currentCompany = await _companyService.GetCompanyEntityByErpEntityIdAsync(id);

            if (currentCompany == null)
            {
                return Error(HttpStatusCode.NotFound, "company", "not found");
            }

            var companyDto = currentCompany.ToDto();

            return new RawJsonActionResult(JsonConvert.SerializeObject(companyDto));
        }

        [HttpDelete]
        [Route("/api/companies/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - DeleteCompany - erpCompId = {id}");

            var company = await _companyService.GetCompanyEntityByErpEntityIdAsync(id);

            if (company == null)
            {
                return Error(HttpStatusCode.NotFound, "company", "not found");
            }

            await _companyService.DeleteCompanyAsync(company);

            return NoContent();
        }
    }
}
