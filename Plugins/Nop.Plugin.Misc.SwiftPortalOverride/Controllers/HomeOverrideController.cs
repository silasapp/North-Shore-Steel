using Microsoft.AspNetCore.Mvc;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class HomeCustomController : HomeController
    {
      
        public override IActionResult Index()
        {
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml");
        }
    }
}
