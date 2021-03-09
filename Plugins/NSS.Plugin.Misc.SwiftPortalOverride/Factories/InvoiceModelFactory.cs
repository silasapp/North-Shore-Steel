﻿using NSS.Plugin.Misc.SwiftCore.DTOs;
using NSS.Plugin.Misc.SwiftCore.Services;
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
        private readonly IApiService _apiService;
        public InvoiceModelFactory(IApiService apiService)
        {
            _apiService = apiService;
        }

        public CompanyInvoiceListModel PrepareInvoiceListModel(int companyId, CompanyInvoiceListModel.SearchFilter filter)
        {
            // search nss api
            var response = new List<ERPSearchInvoicesResponse>();

            if ((!filter.InvoiceId.HasValue && !filter.OrderId.HasValue && filter.PONo == null) && (filter.FromDate == null || filter.ToDate == null))
            {
                // set 1 year range
                if (!filter.FromDate.HasValue && !filter.ToDate.HasValue)
                {
                    filter.FromDate = DateTimeOffset.UtcNow.AddYears(-1);
                    filter.ToDate = DateTimeOffset.UtcNow;
                }
                else if (!filter.FromDate.HasValue && filter.ToDate.HasValue)
                {
                    filter.FromDate = filter.ToDate.Value.AddYears(-1);
                }
                else if(filter.FromDate.HasValue && !filter.ToDate.HasValue)
                {
                    filter.ToDate = filter.FromDate.Value.AddYears(1);
                }
            }

            var request = new ERPSearchInvoicesRequest()
            {
                InvoiceId = filter.InvoiceId?.ToString(),
                FromDate = filter.FromDate?.ToString("yyyy-MM-dd"),
                ToDate = filter.ToDate?.ToString("yyyy-MM-dd"),
                OrderId = filter.OrderId?.ToString(),
                PONo = filter.PONo
            };

            if (filter.IsClosed)
                (_, response) = _apiService.SearchClosedInvoices(companyId, request);
            else
                (_, response) = _apiService.SearchOpenInvoices(companyId, request);

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
                PoNo = invoice.PoNo,
                InvoiceFile = $"{invoice.InvoiceFile}"

        }).ToList();

            var model = new CompanyInvoiceListModel
            {
                FilterContext = filter,
                Invoices = invoices?.OrderByDescending(x => x.InvoiceId)?.ToList()
            };

            return model;
        }
    }
}
