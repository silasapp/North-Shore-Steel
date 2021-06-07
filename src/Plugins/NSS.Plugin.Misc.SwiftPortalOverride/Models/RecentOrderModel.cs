using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class RecentOrderModel : BaseNopModel
    {
        public int OrderId { get; set; }
        public double Weight { get; set; }
        public string OrderTotal { get; set; }
        public string PONo { get; set; }
        public string EstDeliveryDate { get; set; }
        public string OrderStatusName { get; set; }
    }
}
