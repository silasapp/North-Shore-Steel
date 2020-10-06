using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Security;
using Nop.Plugin.Misc.SwiftPortalOverride.Models;
using Nop.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Configuration;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.SwiftPortalOverride.Controllers
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
            if (Request.Cookies["mycookie"] == null)
            {
                //nop.core.customer.customer.id
                // getcurrent loggined custom
                //return a list o companies
                //    if count > 1 show select account
                //    if coount = 1; setcookie

                //name, phone number and email
            }
                
        //show company name on select account

            //onclick pass company id :: cookies.insert(redirect to homescreen)

            return View("~/Plugins/Misc.SwiftPortalOverride/Views/SelectAccount.cshtml");
            //var model = new TransactionModel();
            //model.RecentOrders = _nSSApiProvider.GetRecentOrders(141713);
            //model.RecentInvoices = _nSSApiProvider.GetRecentInvoices(141713);

            //return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);

        }

        private virtual NavigateToHome()
        {
            var model = new TransactionModel();
            // 141713 = erpcompanyid
            model.RecentOrders = _nSSApiProvider.GetRecentOrders(141713);
            model.RecentInvoices = _nSSApiProvider.GetRecentInvoices(141713);

            //if no company show no recent order
            // 

            return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
        }
    }
}
