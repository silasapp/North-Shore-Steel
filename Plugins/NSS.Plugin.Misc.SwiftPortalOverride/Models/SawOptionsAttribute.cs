using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class SawOptionsAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<ProductAttributeValue> Values { get; set; }
    }
}
