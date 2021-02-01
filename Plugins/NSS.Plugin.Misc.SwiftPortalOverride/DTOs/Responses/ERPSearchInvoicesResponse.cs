using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public partial class ERPSearchInvoicesResponse
    {
        [JsonProperty("invoiceId")]
        public int InvoiceId { get; set; }

        [JsonProperty("orderNo")]
        public int OrderNo { get; set; }

        [JsonProperty("poNo")]
        public string PoNo { get; set; }

        [JsonProperty("invoiceAmount")]
        public decimal InvoiceAmount { get; set; }

        [JsonProperty("balanceAmount")]
        public decimal BalanceAmount { get; set; }

        [JsonProperty("invoiceDate")]
        public DateTimeOffset InvoiceDate { get; set; }

        [JsonProperty("invoiceDueDate")]
        public DateTimeOffset InvoiceDueDate { get; set; }


        [JsonProperty("invoiceFile")]
        public string InvoiceFile { get; set; }

        [JsonProperty("invoiceStatusName", NullValueHandling = NullValueHandling.Ignore)]
        public string InvoiceStatusName { get; set; }

        [JsonProperty("invoicePaidDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? InvoicePaidDate { get; set; }
    }

    public partial class ERPSearchInvoicesResponse
    {
        public static List<ERPSearchInvoicesResponse> FromJson(string json) => JsonConvert.DeserializeObject<List<ERPSearchInvoicesResponse>>(json, Converter.Settings);
    }
}
