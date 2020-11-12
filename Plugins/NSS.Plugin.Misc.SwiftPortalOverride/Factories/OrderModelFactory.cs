using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
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
    public class OrderModelFactory : IOrderModelFactory
    {
        #region Fields
        private readonly NSSApiProvider _nSSApiProvider;

        #endregion
        public OrderModelFactory(NSSApiProvider nSSApiProvider)
        {
            _nSSApiProvider = nSSApiProvider;
        }
        #region Ctor

        #endregion
        public CompanyOrderListModel PrepareOrderListModel(int companyId, CompanyOrderListModel.SearchFilter filter)
        {
            // search nss api
            var response = new List<ERPSearchOrdersResponse>();

            var request = new ERPSearchOrdersRequest()
            {
                FromDate = filter?.FromDate?.ToString("yyyyMMdd"),
                ToDate = filter?.ToDate?.ToString("yyyyMMdd"),
                OrderId = filter?.OrderId?.ToString(),
                PONo = filter?.PONo,
            };

            if (filter.IsClosed)
                response = _nSSApiProvider.SearchClosedOrders(companyId, request, useMock: true);
            else
                response = _nSSApiProvider.SearchOpenOrders(companyId, request, useMock: true);

            var orders = response.Select(order => new CompanyOrderListModel.OrderDetailsModel 
            {
                DeliveryDate = order.DeliveryDate,
                DeliveryTicketCount = order.DeliveryTicketCount,
                OrderDate = order.OrderDate,
                OrderId = order.OrderId,
                OrderStatusName = order.OrderStatusName,
                OrderTotal = order.OrderTotal,
                PoNo = order.PoNo,
                PromiseDate = order.PromiseDate,
                ScheduledDate = order.ScheduledDate,
                Weight = order.Weight
            }).ToList();

            var model = new CompanyOrderListModel 
            {
                FilterContext = filter,
                Orders = orders
            };

            return model;
        }

        public OrderDetailsModel PrepareOrderDetailsModel()
        {
            throw new NotImplementedException();
        }
    }
}
