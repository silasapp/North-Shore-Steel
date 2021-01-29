using Nop.Core.Domain.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using NSS.Plugin.Misc.SwiftPortalOverride.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class NewAddress
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int? StateProvinceId { get; set; }
        public string City { get; set; }
        public string ZipPostalCode { get; set; }
        public string AddressType { get; set; }
    }
}
