using Microsoft.AspNetCore.Mvc;
using Nop.Core;
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
            try
            {
                var storeScope = _storeContext.ActiveStoreScopeConfiguration;
                var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftPortalOverrideSettings>(storeScope);

                string baseUrl = swiftPortalOverrideSettings.NSSApiBaseUrl;

                // create recent order 

                baseUrl = "https://private-anon-ab0bddfbec-nssswift.apiary-mock.com/";
                var requestUrl = $"{ baseUrl}companies/3/orders/recent";
                if (!baseUrl.EndsWith('/')) requestUrl = $"{ baseUrl}/companies/3/orders/recent";


                var model = _nSSApiProvider.GetRecentOrders(requestUrl);

                return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
