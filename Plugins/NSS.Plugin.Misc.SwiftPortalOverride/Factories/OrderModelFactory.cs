using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.Common;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public class OrderModelFactory : IOrderModelFactory
    {
        #region Fields

        private readonly ERPApiProvider _nSSApiProvider;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly AddressSettings _addressSettings;
        private readonly ICountryService _countryService;
        private readonly IOrderService _orderService;
        private readonly CustomGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public OrderModelFactory(CustomGenericAttributeService genericAttributeService, IOrderService orderService, ERPApiProvider nSSApiProvider, IAddressModelFactory addressModelFactory, AddressSettings addressSettings, ICountryService countryService, IAddressService addressService)
        {
            _nSSApiProvider = nSSApiProvider;
            _addressModelFactory = addressModelFactory;
            _addressSettings = addressSettings;
            _countryService = countryService;
            _addressService = addressService;
            _orderService = orderService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        public CompanyOrderListModel PrepareOrderListModel(int companyId, CompanyOrderListModel.SearchFilter filter)
        {
            // search nss api
            var response = new List<ERPSearchOrdersResponse>();

            if ((filter.OrderId == null && filter.PONo == null) && (filter.FromDate == null || filter.ToDate == null))
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
                else if (filter.FromDate.HasValue && !filter.ToDate.HasValue)
                {
                    filter.ToDate = filter.FromDate.Value.AddYears(1);
                }
            }

            var request = new ERPSearchOrdersRequest()
            {
                FromDate = filter.FromDate?.ToString("yyyy-MM-dd"),
                ToDate = filter.ToDate?.ToString("yyyy-MM-dd"),
                OrderId = filter.OrderId?.ToString(),
                PONo = filter.PONo,
            };

            if (filter.IsClosed)
                response = _nSSApiProvider.SearchClosedOrders(companyId, request, useMock: false);
            else
                response = _nSSApiProvider.SearchOpenOrders(companyId, request, useMock: false);

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

        public OrderDetailsModel PrepareOrderDetailsModel(int companyId, int erpOrderId, ERPGetOrderDetailsResponse orderDetailsResponse, int mtrCount, List<ERPGetOrderMTRResponse> orderMTRs, string token)
        {
            var model = new OrderDetailsModel();
            Nop.Core.Domain.Orders.Order order = null;

            if (orderDetailsResponse != null)
            {
                model.OrderId = orderDetailsResponse.OrderId;
                model.Weight = orderDetailsResponse.Weight;
                model.PoNo = orderDetailsResponse.PoNo;
                model.OrderDate = orderDetailsResponse.OrderDate;
                model.PromiseDate = orderDetailsResponse.PromiseDate;
                model.ScheduledDate = orderDetailsResponse.ScheduledDate;
                model.DeliveryDate = orderDetailsResponse.DeliveryDate;
                model.Source = orderDetailsResponse.Source;
                model.OrderStatusName = orderDetailsResponse.OrderStatusName;
                model.DeliveryMethodName = orderDetailsResponse.DeliveryMethodName;

                model.DeliveryTicketFile = orderDetailsResponse.DeliveryTicketFile;
                if (!string.IsNullOrEmpty(model.DeliveryTicketFile))
                    model.DeliveryTicketFile = $"{orderDetailsResponse.DeliveryTicketFile}{token}";

                model.InvoiceFile = orderDetailsResponse.InvoiceFile;
                if (!string.IsNullOrEmpty(model.InvoiceFile))
                    model.InvoiceFile = $"{orderDetailsResponse.InvoiceFile}{token}";

                model.OrderFile = orderDetailsResponse.OrderFile;
                if (!string.IsNullOrEmpty(model.OrderFile))
                    model.OrderFile = $"{orderDetailsResponse.OrderFile}{token}";

                model.MtrCount = mtrCount;

                model.LineItemTotal = orderDetailsResponse.LineItemTotal;
                model.TaxTotal = orderDetailsResponse.TaxTotal;
                model.OrderTotal = orderDetailsResponse.OrderTotal;


                // fetch order from db
                var attr = _genericAttributeService.GetAttributeByKeyValue(SwiftCore.Helpers.Constants.ErpOrderNoAttribute, erpOrderId.ToString(), nameof(Order));

                if (attr != null)
                    order = _orderService.GetOrderById(attr.EntityId);

                if (order != null)
                {
                    // nC orderId
                    model.SysOrderId = order.Id;

                    // pickup address
                    if (order.PickupAddressId.HasValue && _addressService.GetAddressById(order.PickupAddressId.Value) is Address pickupAddress)
                    {
                        model.IsPickup = true;
                        model.PickupAddress = new AddressModel
                        {
                            Address1 = pickupAddress.Address1,
                            City = pickupAddress.City,
                            County = pickupAddress.County,
                            CountryName = _countryService.GetCountryByAddress(pickupAddress)?.Name ?? string.Empty,
                            ZipPostalCode = pickupAddress.ZipPostalCode
                        };
                    }
                }

                model.BillingAddress = new AddressModel
                {
                    Address1 = orderDetailsResponse.BillingAddressLine1,
                    Address2 = orderDetailsResponse.BillingAddressLine2,
                    StateProvinceName = orderDetailsResponse.BillingState,
                    City = orderDetailsResponse.BillingCity,
                    ZipPostalCode = orderDetailsResponse.BillingPostalCode
                };

                // shipping address
                model.ShippingAddress = new AddressModel
                {
                    Address1 = orderDetailsResponse.ShippingAddressLine1,
                    Address2 = orderDetailsResponse.ShippingAddressLine2,
                    StateProvinceName = orderDetailsResponse.ShippingState,
                    City = orderDetailsResponse.ShippingCity,
                    ZipPostalCode = orderDetailsResponse.ShippingPostalCode
                };

                // line items
                if (orderDetailsResponse.OrderItems != null)
                    foreach (var item in orderDetailsResponse.OrderItems)
                    {
                        var orderMTR = new OrderDetailsModel.OrderMTRModel();

                        var mtr = orderMTRs.FirstOrDefault(x => x.LineNo == item.LineNo);
                        if (mtr != null)
                            orderMTR = new OrderDetailsModel.OrderMTRModel { LineNo = mtr.LineNo, Description = mtr.Description, HeatNo = mtr.HeatNo, MtrId = mtr.MtrId };

                        var orderItem = new OrderDetailsModel.OrderItemModel
                        {
                            CustomerPartNo = item.CustomerPartNo,
                            Description = item.Description,
                            LineNo = item.LineNo,
                            Quantity = item.Quantity,
                            TotalPrice = item.TotalPrice,
                            TotalWeight = item.TotalWeight,
                            UnitPrice = item.UnitPrice,
                            UOM = item.UOM,
                            WeightPerPiece = item.WeightPerPiece,

                            // mtr
                            MTR = orderMTR
                        };

                        model.OrderItems.Add(orderItem);
                    }

                // mtrs
                foreach (var mtr in orderMTRs)
                {
                    var orderMTR = new OrderDetailsModel.OrderMTRModel
                    {
                        MtrId = mtr.MtrId,
                        LineNo = mtr.LineNo,
                        HeatNo = mtr.HeatNo,
                        Description = mtr.Description,
                        MtrFile = $"{mtr.MtrFile}{token}"
                };

                    model.MTRs.Add(orderMTR);
                }
            }

            return model;
        }
    }
}
