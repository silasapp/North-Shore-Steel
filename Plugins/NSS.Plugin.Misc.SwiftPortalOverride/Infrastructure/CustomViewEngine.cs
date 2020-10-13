using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Infrastructure
{
    public class CustomViewEngine : IViewLocationExpander
    {

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {


            if (context.AreaName == null && context.ViewName == "Components/TopMenu/Default")
            {
                viewLocations = new string[] { "~/Plugins/Misc.SwiftPortalOverride/Views/CustomTopMenu.cshtml"
                }.Concat(viewLocations);

            }

            return viewLocations;

        }
    }
}
