using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Configuration;
using Nop.Web.Controllers;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Linq;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class HomeOverrideController : HomeController
    {
        #region Fields
        private readonly NSSApiProvider _nSSApiProvider;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICustomerCompanyService _customerCompanyService;

        #endregion

        #region Constructor
        public HomeOverrideController(
            ISettingService settingService,
            IStoreContext storeContext,
            NSSApiProvider nSSApiProvider,
            IWorkContext workContext,
            ICustomerCompanyService customerCompanyService

            )
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _nSSApiProvider = nSSApiProvider;
            _workContext = workContext;
            _customerCompanyService = customerCompanyService;
        }
        #endregion

       
        public override IActionResult Index()
        {
            string ERPCId;
            int customerId = _workContext.CurrentCustomer.Id;
            string ERPComId = SwiftPortalOverrideDefaults.ERPCompanyId;
            ERPComId += customerId;
            TransactionModel model = new TransactionModel();

            // get all companies assigned to customer
            IEnumerable<CustomerCompany> customerCompanies = _customerCompanyService.GetCustomerCompanies(customerId);

            if (Request.Cookies[ERPComId] != null && (!string.IsNullOrEmpty(Request.Cookies[ERPComId].ToString())))
            {
                ERPCId = Request.Cookies[ERPComId].ToString();

                // check if customer still has access to previously selected company
                IEnumerable<CustomerCompany> cc = customerCompanies.Where(x => x.Company.ErpCompanyId.ToString() == ERPCId);
                if (cc.Count() > 0)
                {
                    model = GetTransactions(ERPCId);
                    return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
                }

                // remove cookie
                Response.Cookies.Delete(ERPComId);

            }

            if (customerCompanies.Count() == 1)
            {
                ERPCId = customerCompanies.First().Company.ErpCompanyId.ToString();
                Response.Cookies.Append(ERPComId, ERPCId);
                model = GetTransactions(ERPCId);
                return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
            }
            else if (customerCompanies.Count() > 1)
            {
                CustomerSelectAccountModel selectAccountModel = new CustomerSelectAccountModel();
                selectAccountModel.Companies = customerCompanies.Select(cc => cc.Company);
                selectAccountModel.loggedInCustomerId = customerId;
                return View("~/Plugins/Misc.SwiftPortalOverride/Views/SelectAccount.cshtml", selectAccountModel);
            }

            // has access to no company
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);

        }



        private TransactionModel GetTransactions(string ERPCId)
        {
            var model = new TransactionModel();
            model.RecentOrders = _nSSApiProvider.GetRecentOrders(ERPCId);
            model.RecentInvoices = _nSSApiProvider.GetRecentInvoices(ERPCId);
            model.CompanyInfo = _nSSApiProvider.GetCompanyInfo(ERPCId);
            return model;
        }
    }
}
