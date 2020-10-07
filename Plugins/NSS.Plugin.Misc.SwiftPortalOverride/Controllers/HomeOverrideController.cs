using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Configuration;
using Nop.Web.Controllers;
using NSS.Plugin.Misc.SwiftCore.Services;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class HomeOverrideController : HomeController
    {
        private readonly NSSApiProvider _nSSApiProvider;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICustomerCompanyService _customerCompanyService;

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

        public override IActionResult Index()
        {
            string ERPCId = "";
            var ERPComId = SwiftPortalOverrideDefaults.ERPCompanyId;

            if (Request.Cookies[ERPComId] != null && (!string.IsNullOrEmpty(Request.Cookies[ERPComId].ToString())))
            {
                ERPCId = Request.Cookies[ERPComId].ToString();
                TransactionModel model = GetTransactions(ERPCId);

                return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
            }
            else
            {
                int customerId = _workContext.CurrentCustomer.Id;
                var customerCompanies = _customerCompanyService.GetCustomerCompanies(customerId);
                //check if no company is returned
                if (customerCompanies == null)
                {
                    var model = new TransactionModel();
                    return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model = null);
                }


                if (customerCompanies.Count() == 1)
                {
                    // get erpid from customer company
                    // var ERPCId = 
                    Response.Cookies.Append(ERPComId, ERPCId);
                    TransactionModel model = GetTransactions(ERPCId);
                    return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);

                }
                else if (customerCompanies.Count() > 1)
                {
                    var accountModel = new SelectAccountModel();

                    return View("~/Plugins/Misc.SwiftPortalOverride/Views/SelectAccount.cshtml", accountModel);

                }
            }


            //show company name on select account

            //onclick pass company id :: cookies.insert(redirect to homescreen)

            //    //if no company show no recent order
            return 
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
