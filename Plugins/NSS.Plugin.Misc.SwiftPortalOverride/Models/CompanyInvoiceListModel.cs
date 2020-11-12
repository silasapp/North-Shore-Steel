using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial class CompanyInvoiceListModel
    {
        public int InvoiceId { get; set; }
        public int OrderNo { get; set; }
        public string PoNo { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public DateTimeOffset InvoiceDueDate { get; set; }
        public string InvoiceStatusName { get; set; }
        public DateTimeOffset? InvoicePaidDate { get; set; }
    }
}
