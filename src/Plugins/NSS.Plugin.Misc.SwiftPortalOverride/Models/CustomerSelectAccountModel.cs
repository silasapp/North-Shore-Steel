using Nop.Web.Framework.Models;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public record CustomerSelectAccountModel : BaseNopModel
    {
        public IEnumerable<Company> Companies { get; set; }

        public int LoggedInCustomerId { get; set; }
        public int Count { get; set; }
    }
}
