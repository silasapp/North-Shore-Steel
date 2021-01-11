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
using Nop.Services.Common;
using Nop.Core.Domain.Customers;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class HomeOverrideController : HomeController
    {
        #region Fields
        private readonly ERPApiProvider _nSSApiProvider;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICompanyService _companyService;

        #endregion

        #region Constructor
        public HomeOverrideController(
            ISettingService settingService,
            IStoreContext storeContext,
            ERPApiProvider nSSApiProvider,
            IWorkContext workContext,
            ICustomerCompanyService customerCompanyService,
            IGenericAttributeService genericAttributeService,
            ICompanyService companyService

            )
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _nSSApiProvider = nSSApiProvider;
            _workContext = workContext;
            _customerCompanyService = customerCompanyService;
            _genericAttributeService = genericAttributeService;
            _companyService = companyService;
        }
        #endregion

       
        public override IActionResult Index()
        {
            string ERPCId = string.Empty;
            var currentCustomer = _workContext.CurrentCustomer;
            int customerId = currentCustomer.Id;
            Company company = new Company();

            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, customerId);
            TransactionModel model = new TransactionModel();

            // get all companies assigned to customer
            IEnumerable<CustomerCompany> customerCompanies = _customerCompanyService.GetCustomerCompanies(customerId);

            ERPCId = _genericAttributeService.GetAttribute<string>(currentCustomer, compIdCookieKey);
            if (ERPCId != null)
            {
                //ERPCId = Request.Cookies[compIdCookieKey].ToString();
                company = _companyService.GetCompanyEntityByErpEntityId(Convert.ToInt32(ERPCId));
                // check if customer still has access to previously selected company
                IEnumerable<CustomerCompany> cc = customerCompanies.Where(x => x.Company.ErpCompanyId.ToString() == ERPCId);
                if (cc.Count() > 0)
                {
                    model = GetTransactions(ERPCId);
                    _genericAttributeService.SaveAttribute(currentCustomer, NopCustomerDefaults.CompanyAttribute, company.Name);
                    return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
                }

                // remove cookie
                //Response.Cookies.Delete(compIdCookieKey);
                saveAttributeERPCompanyId(currentCustomer, compIdCookieKey, "");
                //_genericAttributeService.SaveAttribute(currentCustomer, compIdCookieKey, "");


            }

            if (customerCompanies.Count() == 1)
            {
                ERPCId = customerCompanies.First().Company.ErpCompanyId.ToString();
                company = _companyService.GetCompanyEntityByErpEntityId(Convert.ToInt32(ERPCId));
                //Response.Cookies.Append(compIdCookieKey, ERPCId);
                saveAttributeERPCompanyId(currentCustomer, compIdCookieKey, ERPCId);
                //_genericAttributeService.SaveAttribute(currentCustomer, compIdCookieKey, ERPCId);
                model = GetTransactions(ERPCId);
                _genericAttributeService.SaveAttribute(currentCustomer, NopCustomerDefaults.CompanyAttribute, company.Name);
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

        public void SelectCompany(string ERPCompanyId)
        {
            var currentCustomer = _workContext.CurrentCustomer;
            int customerId = currentCustomer.Id;
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, customerId);
            saveAttributeERPCompanyId(currentCustomer, compIdCookieKey, ERPCompanyId);
        }

        private void saveAttributeERPCompanyId(Nop.Core.Domain.Customers.Customer currentCustomer, string compIdCookieKey, string ERPCompanyId)
        {
            _genericAttributeService.SaveAttribute(currentCustomer, compIdCookieKey, ERPCompanyId);

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
