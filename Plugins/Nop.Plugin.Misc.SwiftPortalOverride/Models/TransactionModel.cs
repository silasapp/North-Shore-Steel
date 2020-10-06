using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.SwiftPortalOverride.Models
{
    public class TransactionModel : BaseNopModel
    {
        public List<Order> RecentOrders { get; set; }
        public List<Invoice> RecentInvoices { get; set; }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public double Weight { get; set; }
        public string OrderTotal { get; set; }
        public string PONo { get; set; }
        public string EstDeliveryDate { get; set; }
        public string OrderStatusName { get; set; }
    }

    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int OrderNo { get; set; }
        public double InvoiceAmount { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceDueDate { get; set; }
        public string InvoiceStatusName { get; set; }
    }
}
