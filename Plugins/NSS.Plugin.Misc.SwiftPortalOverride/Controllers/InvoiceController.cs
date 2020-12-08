using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftCore.Helpers;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class InvoiceController : BasePublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IInvoiceModelFactory _invoiceModelFactory;
        private readonly ICustomerCompanyService _customerCompanyService;

        #endregion

        #region Ctor

        public InvoiceController(IWorkContext workContext, ICustomerService customerService, IInvoiceModelFactory invoiceModelFactory, ICustomerCompanyService customerCompanyService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _invoiceModelFactory = invoiceModelFactory;
            _customerCompanyService = customerCompanyService;
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

            var model = new CompanyInvoiceListModel();

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
