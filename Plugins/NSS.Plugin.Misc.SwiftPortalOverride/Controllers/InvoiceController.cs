using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using System;
using Nop.Services.Common;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class InvoiceController : BasePublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IInvoiceModelFactory _invoiceModelFactory;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly SwiftCoreSettings _swiftCoreSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IApiService _apiService;

        #endregion

        #region Ctor

        public InvoiceController(IApiService apiService, IGenericAttributeService genericAttributeService, IWorkContext workContext, ICustomerService customerService, IInvoiceModelFactory invoiceModelFactory, ICustomerCompanyService customerCompanyService, SwiftCoreSettings swiftCoreSettings)
        {
            _workContext = workContext;
            _customerService = customerService;
            _invoiceModelFactory = invoiceModelFactory;
            _customerCompanyService = customerCompanyService;
            _swiftCoreSettings = swiftCoreSettings;
            _genericAttributeService = genericAttributeService;
            _apiService = apiService;
        }

        #endregion

        #region Methods

        [HttpsRequirement]
        public IActionResult CompanyInvoices()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));
            bool isAp = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.AP);
            bool isBuyer = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer);
            bool isOperations = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Operations);

            if (!isAp && !isBuyer)
                return AccessDeniedView();

            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            // get company info
            var customerCompany = _customerCompanyService.GetCustomerCompanyByErpCompId(_workContext.CurrentCustomer.Id, eRPCompanyId);
            var company = _apiService.GetCompanyInfo(eRPCompanyId.ToString());

            // build credit summary
            var creditSummary = new CompanyInvoiceListModel.CreditSummaryModel
            {
                ApplyForCreditUrl = string.IsNullOrEmpty(_swiftCoreSettings.ApplyForCreditUrl) ? "https://www.nssco.com/assets/files/newaccountform.pdf" : _swiftCoreSettings.ApplyForCreditUrl,
                CanCredit = customerCompany?.CanCredit ?? false,
                CompanyHasCreditTerms = company?.HasCredit ?? false
            };

            if ( creditSummary.CompanyHasCreditTerms && (isAp))
            {
                var creditResposne = _apiService.GetCompanyCreditBalance(eRPCompanyId);

                creditSummary.CreditAmount = creditResposne?.CreditAmount ?? decimal.Zero;
                creditSummary.CreditLimit = creditResposne?.CreditLimit ?? decimal.Zero;
                creditSummary.OpenInvoiceAmount = creditResposne?.OpenInvoiceAmount ?? decimal.Zero;
                creditSummary.PastDueAmount = creditResposne?.PastDueAmount ?? decimal.Zero;
            }
            var customerRoles = new CompanyInvoiceListModel.CustomerRolesModel
            {
                IsAP = isAp,
                IsBuyer = isBuyer
            };

            var model = new CompanyInvoiceListModel
            {
                CreditSummary = creditSummary,
                CustomerRoles = customerRoles
                
            };

            return View(model);
        }

      
        [IgnoreAntiforgeryToken]
        public PartialViewResult SearchCompanyInvoices([FromBody]CompanyInvoiceListModel.SearchFilter filter)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            var model = new CompanyInvoiceListModel();

            if (eRPCompanyId > 0)
                model = _invoiceModelFactory.PrepareInvoiceListModel(eRPCompanyId, filter);
            model.IsClosed = filter.IsClosed;
            return PartialView("_InvoiceGrid", model);
        }

        #endregion
    }
}
