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
        private readonly ERPApiProvider _eRPApiProvider;

        #endregion

        #region Ctor

        public InvoiceController(IWorkContext workContext, ICustomerService customerService, IInvoiceModelFactory invoiceModelFactory, ICustomerCompanyService customerCompanyService, SwiftCoreSettings swiftCoreSettings, ERPApiProvider eRPApiProvider)
        {
            _workContext = workContext;
            _customerService = customerService;
            _invoiceModelFactory = invoiceModelFactory;
            _customerCompanyService = customerCompanyService;
            _swiftCoreSettings = swiftCoreSettings;
            _eRPApiProvider = eRPApiProvider;
        }

        #endregion

        #region Methods

        [HttpsRequirement]
        public IActionResult CompanyInvoices()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Common.GetSavedERPCompanyIdFromCookies(Request.Cookies[compIdCookieKey]);

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.AP))
                return AccessDeniedView();

            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            // get credit summary
            var customerCompany = _customerCompanyService.GetCustomerCompanyByErpCompId(_workContext.CurrentCustomer.Id, eRPCompanyId);
            var creditSummary = new CompanyInvoiceListModel.CreditSummaryModel
            {
                ApplyForCreditUrl = _swiftCoreSettings.ApplyForCreditUrl ?? "https://www.nssco.com/assets/files/newaccountform.pdf",
                CanCredit = customerCompany?.CanCredit ?? false
            };

            if (creditSummary.CanCredit)
            {
                var creditResposne = _eRPApiProvider.GetCompanyCreditBalance(eRPCompanyId);

                creditSummary.CreditAmount = creditResposne?.CreditAmount ?? decimal.Zero;
                creditSummary.CreditLimit = creditResposne?.CreditLimit ?? decimal.Zero;
                creditSummary.OpenInvoiceAmount = creditResposne?.OpenInvoiceAmount ?? decimal.Zero;
                creditSummary.PastDueAmount = creditResposne?.PastDueAmount ?? decimal.Zero;
            }

            var model = new CompanyInvoiceListModel
            {
                CreditSummary = creditSummary
            };

            return View(model);
        }

      
        [IgnoreAntiforgeryToken]
        public PartialViewResult SearchCompanyInvoices([FromBody]CompanyInvoiceListModel.SearchFilter filter)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Common.GetSavedERPCompanyIdFromCookies(Request.Cookies[compIdCookieKey]);

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.AP))
                return (PartialViewResult)AccessDeniedView();

            var model = new CompanyInvoiceListModel();

            if (eRPCompanyId > 0)
                model = _invoiceModelFactory.PrepareInvoiceListModel(eRPCompanyId, filter);
            model.IsClosed = filter.IsClosed;
            return PartialView("_InvoiceGrid", model);
        }

        #endregion
    }
}
