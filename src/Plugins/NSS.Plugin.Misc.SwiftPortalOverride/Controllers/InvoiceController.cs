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
using System.Threading.Tasks;

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
        public async Task<IActionResult> CompanyInvoices()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));
            bool isAp = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.AP);
            bool isBuyer = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer);

            if (!isAp)
                return AccessDeniedView();

            if (!(await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync())))
                return Challenge();

            // get company info
            var customerCompany = await _customerCompanyService.GetCustomerCompanyByErpCompIdAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId);
            var company = await _apiService.GetCompanyInfoAsync(eRPCompanyId.ToString());

            // build credit summary
            var creditSummary = new CompanyInvoiceListModel.CreditSummaryModel
            {
                ApplyForCreditUrl = string.IsNullOrEmpty(_swiftCoreSettings.ApplyForCreditUrl) ? "https://www.nssco.com/assets/files/newaccountform.pdf" : _swiftCoreSettings.ApplyForCreditUrl,
                CanCredit = customerCompany?.CanCredit ?? false,
                CompanyHasCreditTerms = company?.HasCredit ?? false
            };

            if ( creditSummary.CompanyHasCreditTerms && (isAp))
            {
                var creditResposne = await _apiService.GetCompanyCreditBalanceAsync(eRPCompanyId);

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
        public async Task<PartialViewResult> SearchCompanyInvoices([FromBody]CompanyInvoiceListModel.SearchFilter filter)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            var model = new CompanyInvoiceListModel();

            if (eRPCompanyId > 0)
                model = await _invoiceModelFactory.PrepareInvoiceListModelAsync(eRPCompanyId, filter);
            model.IsClosed = filter.IsClosed;
            return PartialView("_InvoiceGrid", model);
        }

        #endregion
    }
}
