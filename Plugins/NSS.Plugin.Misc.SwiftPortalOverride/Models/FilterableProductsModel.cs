using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class FilterableProductsModel : BaseNopModel
    {
        public List<SpecificationFilter> SpecificationFilters { get; set; }
        public IEnumerable<string> UniqueSpecTitles { get; set; }

    }

    public class SpecificationFilter
    {
        public int Id { get; set; }
        public string SpecTitle { get; set; }
        public string Name { get; set; }
        public int ProductCount { get; set; }
    }
}
