using Nop.Core.Domain.Catalog;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public partial interface IProductModelFactory
    {
        Task<IEnumerable<ProductOverviewModel>> PrepareSwiftProductOverviewmodelAsync(IEnumerable<Product> products);
    }
}
