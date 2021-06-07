using Nop.Web.Framework.Models;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class CustomerSelectAccountModel : BaseNopModel
    {
        public IEnumerable<Company> Companies { get; set; }

        public int loggedInCustomerId { get; set; }
        public int Count { get; set; }
    }
}
