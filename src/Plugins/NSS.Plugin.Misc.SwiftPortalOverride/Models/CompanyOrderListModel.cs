﻿using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial record CompanyOrderListModel : BaseNopModel
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
        public partial record OrderDetailsModel : BaseNopEntityModel
        {
            public int OrderId { get; set; }
            public decimal Weight { get; set; }

            public decimal OrderTotal { get; set; }

            public string PoNo { get; set; }

            public bool IsMultipleShipment { get; set; }

            public DateTimeOffset? OrderDate { get; set; }

            public DateTimeOffset? PromiseDate { get; set; }

            public DateTimeOffset? ScheduledDate { get; set; }

            public string OrderStatusName { get; set; }
            public string DeliveryStatus { get; set; }

            public DateTimeOffset? DeliveryDate { get; set; }

            public string DeliveryTicketFile { get; set; }

            public int? DeliveryTicketCount { get; set; }

            // computed
            public DateTimeOffset? OrderDateUTC { get => OrderDate.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(OrderDate.Value.UtcDateTime, DateTimeKind.Unspecified), TimeSpan.FromHours(-7)).ToUniversalTime() : OrderDate; }
            public DateTimeOffset? PromiseDateUTC { get => PromiseDate.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(PromiseDate.Value.UtcDateTime, DateTimeKind.Unspecified), TimeSpan.FromHours(-7)).ToUniversalTime() : PromiseDate; }
            public DateTimeOffset? ScheduledDateUTC { get => ScheduledDate.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(ScheduledDate.Value.UtcDateTime, DateTimeKind.Unspecified), TimeSpan.FromHours(-7)).ToUniversalTime() : ScheduledDate; }
            public string DeliveryDateUTC { get => DeliveryDate.HasValue ? (new DateTimeOffset(DateTime.SpecifyKind(DeliveryDate.Value.UtcDateTime, DateTimeKind.Unspecified), TimeSpan.FromHours(-7)).ToUniversalTime()).ToString("MM/dd/yy") : "N/A"; }
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