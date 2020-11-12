using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public class InvoiceModelFactory : IInvoiceModelFactory
    {
        private readonly NSSApiProvider _nSSApiProvider;
        public InvoiceModelFactory(NSSApiProvider nSSApiProvider)
        {
            _nSSApiProvider = nSSApiProvider;
        }

        public CompanyInvoiceListModel PrepareOrderListModel(int companyId, CompanyInvoiceListModel.SearchFilter filter)
        {
            // search nss api
            var response = new List<ERPSearchInvoicesResponse>();

            var request = new ERPSearchInvoicesRequest()
            {
                InvoiceId = filter.invoiceId?.ToString(),
                FromDate = filter?.FromDate?.ToString("yyyyMMdd"),
                ToDate = filter?.ToDate?.ToString("yyyyMMdd"),
                OrderId = filter?.OrderId?.ToString(),
                PONo = filter?.PONo
            };

            if (filter.IsClosed)
                response = _nSSApiProvider.SearchClosedInvoices(companyId, request);
            else
                response = _nSSApiProvider.SearchOpenInvoices(companyId, request);

            // map response
            var invoices = response.Select(invoice => new CompanyInvoiceListModel.InvoiceDetailsModel
            {
                BalanceAmount = invoice.BalanceAmount,
                InvoiceAmount = invoice.InvoiceAmount,
                InvoiceDate = invoice.InvoiceDate,
                InvoiceDueDate = invoice.InvoiceDueDate,
                InvoiceId = invoice.InvoiceId,
                InvoicePaidDate = invoice.InvoicePaidDate,
                InvoiceStatusName = invoice.InvoiceStatusName,
                OrderNo = invoice.OrderNo,
                PoNo = invoice.PoNo
            }).ToList();

            var model = new CompanyInvoiceListModel
            {
                FilterContext = filter,
                Invoices = invoices
            };

            return model;
        }
    }
}
