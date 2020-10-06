using Microsoft.AspNetCore.Mvc;
using Nop.Core;
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
            //var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            //var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftPortalOverrideSettings>(storeScope);

            //string baseUrl = swiftPortalOverrideSettings.NSSApiBaseUrl;



            //var orderRequestUrl = $"{ baseUrl}companies/3/orders/recent";
            //var invoiceRequestUrl = $"{ baseUrl}companies/3/invoices/recent";
            //if (!baseUrl.EndsWith('/'))
            //{
            //    orderRequestUrl = $"{ baseUrl}/companies/3/orders/recent";
            //    invoiceRequestUrl = $"{ baseUrl}/companies/3/invoices/recent";
            //}

            var model = new TransactionModel();
            model.RecentOrders = _nSSApiProvider.GetRecentOrders(141713);
            model.RecentInvoices = _nSSApiProvider.GetRecentInvoices(141713);

            return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);

        }
    }
}
