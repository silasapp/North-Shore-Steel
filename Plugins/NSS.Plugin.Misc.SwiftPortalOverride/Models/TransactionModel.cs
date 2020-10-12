using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class TransactionModel : BaseNopModel
    {
        public List<Order> RecentOrders { get; set; }
        public List<Invoice> RecentInvoices { get; set; }

        public CompanyInfo CompanyInfo { get; set; }
    }

    public class CompanyInfo
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public SalesContact SalesContact { get; set; }
    }

    public class SalesContact
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public double Weight { get; set; }
        public double OrderTotal { get; set; }
        public string PONo { get; set; }
        public DateTime EstDeliveryDate { get; set; }
        public string OrderStatusName { get; set; }
    }

    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int OrderNo { get; set; }
        public double InvoiceAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime InvoiceDueDate { get; set; }
        public string InvoiceStatusName { get; set; }
    }
}
