using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin")
            {
                viewLocations = new[] { $"/Plugins/Misc.SwiftPortalOverride/Areas/Admin/Views/{context.ControllerName}/{context.ViewName}.cshtml" }.Concat(viewLocations);
            }
            else
            {
                if (context.ViewName == "Components/TopMenu/Default")
                {
                    viewLocations = new string[] { 
                        "~/Plugins/Misc.SwiftPortalOverride/Views/CustomTopMenu/CustomTopMenu.cshtml"
                    }.Concat(viewLocations);

                }
                else
                {
                    viewLocations = new[] { 
                        $"/Plugins/Misc.SwiftPortalOverride/Views/{context.ControllerName}/{context.ViewName}.cshtml", 
                        $"/Plugins/Misc.SwiftPortalOverride/Views/Shared/{context.ViewName}.cshtml", 
                    }.Concat(viewLocations);
                }
            }

            return viewLocations;
        }
    }
}
