using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public partial interface ICatalogModelFactory
    {
        public  Task<CatalogModel> PrepareSwiftCatalogModelAsync(IList<int> categoryIds, IList<int> specIds, string searchKeyword = null, bool isPageLoad = false);
    }
}
