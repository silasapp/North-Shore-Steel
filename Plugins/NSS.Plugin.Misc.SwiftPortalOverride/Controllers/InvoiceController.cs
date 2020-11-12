using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class InvoiceController : BasePublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public InvoiceController(IWorkContext workContext, ICustomerService customerService)
        {
            _workContext = workContext;
            _customerService = customerService;
        }

        #endregion

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
        public PartialViewResult SearchCompanyInvoices(bool IsClosedInvoice)
        {

            var model = new CompanyInvoiceListModel();
            if (!IsClosedInvoice)
            {
                model.Invoices = new List<InvoiceDetailsModel> {
                new InvoiceDetailsModel{ Id = 1, InvoiceId = 12345, OrderNo = 34553, PoNo = "P-32443", BalanceAmount = (decimal)100.21, InvoiceAmount = (decimal)135.33, InvoiceDate = DateTime.Now.AddDays(-20), InvoiceDueDate = DateTime.Now.AddDays(-5), InvoiceStatusName = "Pending" },
                new InvoiceDetailsModel{ Id = 2, InvoiceId = 43434, OrderNo = 32893, PoNo = "P-98943", BalanceAmount = (decimal)230.25, InvoiceAmount = (decimal)320.00, InvoiceDate = DateTime.Now.AddDays(-5), InvoiceDueDate = DateTime.Now.AddDays(2), InvoiceStatusName = "Pending" },
                new InvoiceDetailsModel{ Id = 3, InvoiceId = 53983, OrderNo = 99098, PoNo = "P-08933", BalanceAmount = (decimal)30.11, InvoiceAmount = (decimal)3400.50, InvoiceDate = DateTime.Now.AddDays(-3), InvoiceDueDate = DateTime.Now.AddDays(-2), InvoiceStatusName = "Pending" },
                new InvoiceDetailsModel{ Id = 4, InvoiceId = 09473, OrderNo = 89933, PoNo = "P-32083", BalanceAmount = (decimal)930.10, InvoiceAmount = (decimal)1000.11, InvoiceDate = DateTime.Now.AddDays(-25), InvoiceDueDate = DateTime.Now.AddDays(-1), InvoiceStatusName = "Pending" },
            };
            }
            else
            {
                model.Invoices = new List<InvoiceDetailsModel> {
                new InvoiceDetailsModel{ Id = 1, InvoiceId = 33333, OrderNo = 89893, PoNo = "P-42445", BalanceAmount = 0, InvoiceAmount = (decimal)135.33, InvoiceDate = DateTime.Now.AddDays(-9), InvoiceDueDate = DateTime.Now.AddDays(-8), InvoiceStatusName = "Paid", InvoicePaidDate = DateTime.Now.AddDays(-6) },
                new InvoiceDetailsModel{ Id = 2, InvoiceId = 77873, OrderNo = 32422, PoNo = "P-89832", BalanceAmount = 0, InvoiceAmount = (decimal)148.40, InvoiceDate = DateTime.Now.AddDays(-11), InvoiceDueDate = DateTime.Now.AddDays(2), InvoiceStatusName = "Paid", InvoicePaidDate = DateTime.Now.AddDays(-1) },
                new InvoiceDetailsModel{ Id = 3, InvoiceId = 12332, OrderNo = 66454, PoNo = "P-32344", BalanceAmount = 0, InvoiceAmount = (decimal)3800.50, InvoiceDate = DateTime.Now.AddDays(-4), InvoiceDueDate = DateTime.Now.AddDays(3), InvoiceStatusName = "Paid", InvoicePaidDate = DateTime.Now.AddDays(-2) },
                new InvoiceDetailsModel{ Id = 4, InvoiceId = 97777, OrderNo = 09373, PoNo = "P-39839", BalanceAmount = 0, InvoiceAmount = (decimal)4700.00, InvoiceDate = DateTime.Now.AddDays(-15), InvoiceDueDate = DateTime.Now.AddDays(-12), InvoiceStatusName = "Paid", InvoicePaidDate = DateTime.Now },
            };
            }
            return PartialView("_InvoiceGrid", model);
        }
    }
}
