using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class InvoiceController : BasePublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IInvoiceModelFactory _invoiceModelFactory;

        #endregion

        #region Ctor

        public InvoiceController(IWorkContext workContext, ICustomerService customerService, IInvoiceModelFactory invoiceModelFactory)
        {
            _workContext = workContext;
            _customerService = customerService;
            _invoiceModelFactory = invoiceModelFactory;
        }

        #endregion

        #region Methods

        [HttpsRequirement]
        public IActionResult OpenInvoices()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new CompanyInvoiceListModel();

            return View(model);
        }

        [HttpsRequirement]
        public IActionResult ClosedInvoices()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new CompanyInvoiceListModel();

            return View(model);
        }


        [IgnoreAntiforgeryToken]
        public PartialViewResult SearchCompanyInvoices(CompanyInvoiceListModel.SearchFilter filter)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);

            int.TryParse(Request.Cookies[compIdCookieKey], out int eRPCompanyId);

            var model = new CompanyInvoiceListModel();

            if (eRPCompanyId > 0)
                model = _invoiceModelFactory.PrepareOrderListModel(eRPCompanyId, filter);

            return PartialView();
        }

        #endregion
    }
}
