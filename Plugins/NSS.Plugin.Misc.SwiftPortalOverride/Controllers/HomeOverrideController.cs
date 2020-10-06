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
            string ERPId;
            //var ERPCompanyId = SwiftPortalOverrideDefaults.ERPCompanyId;
            var ERPCompanyId = "141713";
            if (Request.Cookies[ERPCompanyId] != null)
            {
                ERPId = Request.Cookies[ERPCompanyId].ToString();
                var model = new TransactionModel();
                model.RecentOrders = _nSSApiProvider.GetRecentOrders(ERPId);
                model.RecentInvoices = _nSSApiProvider.GetRecentInvoices(ERPId);
                model.CompanyInfo = _nSSApiProvider.GetCompanyInfo(ERPId);

                return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
            }
            else
            {

                return View("~/Plugins/Misc.SwiftPortalOverride/Views/SelectAccount.cshtml");
            }









            // nop.core.customer.customer.id
            // getcurrent loggined custom
            // return a list o companies
            //    if count > 1 show select account
            //    if coount = 1; setcookie

            //name, phone number and email


            //show company name on select account

            //onclick pass company id :: cookies.insert(redirect to homescreen)

            // return View("~/Plugins/Misc.SwiftPortalOverride/Views/SelectAccount.cshtml");
            //var model = new TransactionModel();
            //model.RecentOrders = _nSSApiProvider.GetRecentOrders(141713);
            //model.RecentInvoices = _nSSApiProvider.GetRecentInvoices(141713);

            //return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);

        }

        //private IActionResult NavigateToHome(TransactionModel model)
        //{
        //    // var model = new TransactionModel();
        //    // 141713 = erpcompanyid
        //    model.RecentOrders = _nSSApiProvider.GetRecentOrders(141713);
        //    model.RecentInvoices = _nSSApiProvider.GetRecentInvoices(141713);

        //    //if no company show no recent order
        //    // 

        //    return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
        //}
    }
}
