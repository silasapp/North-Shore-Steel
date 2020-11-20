﻿using Nop.Web.Framework.Models;
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
            FilterContext = new SearchFilter();
        }

        public IList<OrderDetailsModel> Orders { get; set; }
        public SearchFilter FilterContext { get; set; }
        public bool IsClosed { get; set; }


        #region Nested Classes
        public partial class OrderDetailsModel : BaseNopEntityModel
        {
            public int OrderId { get; set; }
            public decimal Weight { get; set; }

            public decimal OrderTotal { get; set; }

            public string PoNo { get; set; }

            public DateTimeOffset OrderDate { get; set; }

            public DateTimeOffset PromiseDate { get; set; }

            public DateTimeOffset? ScheduledDate { get; set; }

            public string OrderStatusName { get; set; }

            public DateTimeOffset? DeliveryDate { get; set; }

            public int? DeliveryTicketCount { get; set; }
        }

        public partial class SearchFilter
        {
            public int? OrderId { get; set; }
            public DateTimeOffset? FromDate { get; set; }
            public DateTimeOffset? ToDate { get; set; }
            public string PONo { get; set; }
            public bool IsClosed { get; set; }
        }
        #endregion
    }
}