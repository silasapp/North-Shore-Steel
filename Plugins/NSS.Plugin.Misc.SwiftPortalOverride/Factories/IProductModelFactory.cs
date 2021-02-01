using Nop.Core.Domain.Catalog;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public partial interface IProductModelFactory
    {
        IEnumerable<ProductOverviewModel> PrepareSwiftProductOverviewmodel(IEnumerable<Product> products);
    }
}
