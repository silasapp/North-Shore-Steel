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
    public class UserConfirmationController : BasePublicController
    {
        #region Fields
        private readonly ERPApiProvider _nSSApiProvider;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICustomerCompanyService _customerCompanyService;

        #endregion

        #region Constructor
       
        #endregion


        public virtual IActionResult Index()
        {

            return View();
        }

        public virtual IActionResult Approve()
        {
            return null;
        }

        public virtual IActionResult Reject()
        {
            return null;
        }
    }
}
