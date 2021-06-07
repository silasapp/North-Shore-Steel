using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial class OrderShippingDetailsModel
    {

        public OrderShippingDetailsModel()
        {
            Shipments = new List<Shipment>();
            Items = new List<Item>();
        }

        public string PoNo { get; set; }
        public List<Shipment> Shipments { get; set; }
        public List<Item> Items { get; set; }

        public partial class Shipment
        {
            public Shipment()
            {
                Items = new List<Item>();
            }
            public long ShipmentId { get; set; }
            public string Status { get; set; }
            public string ScheduledDate { get; set; }
            public long TotalWeight { get; set; }
            public List<Item> Items { get; set; }
        }

        public partial class Item
        {
            public string Description { get; set; }
            public int Quantity { get; set; }
            public long Weight { get; set; }
        }
    }
}
