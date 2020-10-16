using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Web.Factories
{
    public partial interface ICatalogModelFactory
    {
        public void PrepareSwiftCatalogModel(IList<int> categoryIds, IList<int> specIds);
    }
}
