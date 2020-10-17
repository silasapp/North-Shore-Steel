using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Web.Factories
{
    public partial interface IProductModelFactory
    {
        IEnumerable<ProductOverviewModel> PrepareSwiftProductOverviewmodel(IEnumerable<Product> products);
    }
}
