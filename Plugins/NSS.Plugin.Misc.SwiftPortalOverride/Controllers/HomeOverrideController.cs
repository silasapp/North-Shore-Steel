using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Configuration;
using Nop.Web.Controllers;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class HomeOverrideController : HomeController
    {
        private readonly NSSApiProvider _nSSApiProvider;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerCompanyService _customerCompanyService;

        public HomeOverrideController(
            ISettingService settingService,
            IStoreContext storeContext,
            NSSApiProvider nSSApiProvider
            )
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _nSSApiProvider = nSSApiProvider;
        }

        public override IActionResult Index()
        {
            string ERPCId = "";
            var ERPComId = SwiftPortalOverrideDefaults.ERPCompanyId;

            if (Request.Cookies[ERPComId] != null && (!string.IsNullOrEmpty(Request.Cookies[ERPComId].ToString())))
            {
                ERPCId = Request.Cookies[ERPComId].ToString();
                var model = new TransactionModel();
                model.RecentOrders = _nSSApiProvider.GetRecentOrders(ERPCId);
                model.RecentInvoices = _nSSApiProvider.GetRecentInvoices(ERPCId);
                model.CompanyInfo = _nSSApiProvider.GetCompanyInfo(ERPCId);

                return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
            }
            else
            {
                

                // Response.Cookies.Append(ERPComId, ERPCId);
                return View("~/Plugins/Misc.SwiftPortalOverride/Views/SelectAccount.cshtml");
            }



            // nop.core.customer.customer.id
            // getcurrent loggined custom
            // return a list o companies
            //    if count > 1 show select account
            //    if coount = 1; setcookie



            //show company name on select account

            //onclick pass company id :: cookies.insert(redirect to homescreen)

        //    //if no company show no recent order
        }

    }
}
