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
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class CompaniesController : BaseApiController
    {
        private readonly ICompanyService _companyService;
        public CompaniesController( 
            ICompanyService companyService,
            IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService, IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        [Route("/api/companies")]
        [ProducesResponseType(typeof(IList<CompanyDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult RetrieveCompanyList()
        {
            var companiesDto = new List<CompanyDto>();

            var companyList = _companyService.GetCompanyList();

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
        public IActionResult CreateCompany([ModelBinder(typeof(JsonModelBinder<CompanyDto>))] Delta<CompanyDto> companyDelta)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var newCompany = _companyService.GetCompanyEntityByErpEntityId(companyDelta.Dto.ErpCompanyId);

            if (newCompany != null)
            {
                return Error(HttpStatusCode.BadRequest, "company", "duplicate record found.");
            }

            newCompany = new Company { CreatedOnUtc = DateTime.UtcNow, UpdatedOnUtc =  DateTime.UtcNow};

            companyDelta.Merge(newCompany);

            _companyService.InsertCompany(newCompany);

            var companyDto = newCompany.ToDto();

            return new RawJsonActionResult(JsonConvert.SerializeObject(companyDto));
        }

        [HttpPut]
        [Route("/api/companies/{id}")]
        [ProducesResponseType(typeof(CompanyDto),(int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult UpdateCompany([ModelBinder(typeof(JsonModelBinder<CompanyDto>))] Delta<CompanyDto> companyDelta)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var companyToUpdate = _companyService.GetCompanyEntityByErpEntityId(companyDelta.Dto.Id);

            if(companyToUpdate == null)
            {
                return Error(HttpStatusCode.NotFound, "company", "not found");
            }

            companyDelta.Dto.Id = companyToUpdate.Id;
            //companyDelta.Dto.ErpCompanyId = companyToUpdate.ErpCompanyId;


            companyDelta.Merge(companyToUpdate);

            // reset id
            companyToUpdate.Id = companyDelta.Dto.Id;

            _companyService.UpdateCompany(companyToUpdate);

            var companyDto = companyToUpdate.ToDto();

            return new RawJsonActionResult(JsonConvert.SerializeObject(companyDto));
        }

        [HttpDelete]
        [Route("/api/companies/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult DeleteCompany(int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var company = _companyService.GetCompanyEntityByErpEntityId(id);

            if (company == null)
            {
                return Error(HttpStatusCode.NotFound, "company", "not found");
            }

            _companyService.DeleteCompany(company);

            return NoContent();
        }
    }
}
