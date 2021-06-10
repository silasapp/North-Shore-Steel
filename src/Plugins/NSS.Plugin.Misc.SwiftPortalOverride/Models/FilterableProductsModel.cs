using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public record FilterableProductsModel : BaseNopModel
    {
        public IEnumerable<IGrouping<string, SpecificationFilter>> ProductsGroupedBySpecTitle { get; set; }

        public List<SpecificationFilter> SpecificationFilters { get; set; }
        public string rr { get; set; }

    }

    public class SpecificationFilter
    {
        public int Id { get; set; }
        public string SpecTitle { get; set; }
        public string Name { get; set; }
        public int ProductCount { get; set; }
    }
}
