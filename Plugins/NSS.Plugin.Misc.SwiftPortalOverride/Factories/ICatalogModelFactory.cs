using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public partial interface ICatalogModelFactory
    {
        public CatalogModel PrepareSwiftCatalogModel(IList<int> categoryIds, IList<int> specIds, string searchKeyword = null, bool isPageLoad = false);
    }
}
