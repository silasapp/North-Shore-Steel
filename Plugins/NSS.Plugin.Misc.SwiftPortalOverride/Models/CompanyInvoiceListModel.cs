using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial class CompanyInvoiceListModel : BaseNopModel
    {
        public CompanyInvoiceListModel()
        {
            Invoices = new List<InvoiceDetailsModel>();
            FilterContext = new SearchFilter();
            CreditSummary = new CreditSummaryModel();
            CustomerRoles = new CustomerRolesModel();
        }

        public IList<InvoiceDetailsModel> Invoices { get; set; }

        public CreditSummaryModel CreditSummary { get; set; }

        public CustomerRolesModel CustomerRoles { get; set; }

        public SearchFilter FilterContext { get; set; }

        public bool IsClosed { get; set; }


        #region Nested Classes

        public partial class CreditSummaryModel
        {
            public bool CanCredit { get; set; }
            public bool CompanyHasCreditTerms { get; set; }
            public string ApplyForCreditUrl { get; set; }
            public decimal CreditAmount { get; set; }
            public decimal CreditLimit { get; set; }
            public decimal OpenInvoiceAmount { get; set; }
            public decimal PastDueAmount { get; set; }
        }

        public partial class CustomerRolesModel
        {
            public bool IsAP { get; set; }
            public bool IsBuyer { get; set; }
            public bool IsOperations { get; set; }
        }
        public partial class InvoiceDetailsModel : BaseNopEntityModel
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
            public string InvoiceFile { get; set; }
        }

        public partial class SearchFilter
        {
            public int? InvoiceId { get; set; }
            public int? OrderId { get; set; }
            public DateTimeOffset? FromDate { get; set; }
            public DateTimeOffset? ToDate { get; set; }
            public string PONo { get; set; }
            public bool IsClosed { get; set; }
        }

        #endregion
    }
}
