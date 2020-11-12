using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial class CompanyOrderListModel : BaseNopModel
    {
        public CompanyOrderListModel()
        {
            Orders = new List<OrderDetailsModel>();
        }

        public IList<OrderDetailsModel> Orders { get; set; }


        #region Nested Classes
        public partial class OrderDetailsModel : BaseNopEntityModel
        {
            public int OrderNo { get; set; }
            public decimal Weight { get; set; }
            public decimal OrderTotal { get; set; }
            public string PONo { get; set; }
            public DateTimeOffset OrderDate { get; set; }
            public DateTimeOffset? PromisedDate { get; set; }
            public DateTimeOffset? ShippedDate { get; set; }
            public DateTimeOffset? ScheduledDate { get; set; }
            public string OrderStatusName { get; set; }
            public string OntimeStatusName { get; set; }
            public int DaysLate { get; set; }
        }
        #endregion
    }
}
