using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Web.Factories
{
    public partial interface ICatalogModelFactory
    {
        public CatalogModel PrepareSwiftCatalogModel(IList<int> categoryIds, IList<int> specIds);
    }
}
