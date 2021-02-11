using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftApi.Attributes;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.JSON.ActionResults;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class OrderFundsCaptureController : BaseApiController
    {
        private readonly CustomGenericAttributeService _genericAttributeService;
        private readonly PayPalProcessor _payPalProcessor;
        private readonly SwiftCoreSettings _swiftCoreSettings;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;

        public OrderFundsCaptureController(ILogger logger, IOrderProcessingService orderProcessingService, IOrderService orderService, SwiftCoreSettings swiftCoreSettings, PayPalProcessor payPalProcessor, CustomGenericAttributeService genericAttributeService, IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService, IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _genericAttributeService = genericAttributeService;
            _payPalProcessor = payPalProcessor;
            _swiftCoreSettings = swiftCoreSettings;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _logger = logger;
        }


        [HttpPost]
        [Route("/api/orders/{orderId}/capture")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult CaptureOrder(int orderId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - CaptureOrder - orderId = {orderId}");

            // get entity order
            var sysOrderId = _genericAttributeService.GetAttributeByKeyValue(SwiftCore.Helpers.Constants.ErpOrderNoAttribute, orderId.ToString(), nameof(Order))?.EntityId;

            if (sysOrderId == null)
                return Error(HttpStatusCode.BadRequest, "order", "not found.");

            // get authID
            var order = _orderService.GetOrderById(sysOrderId.Value);

            if (order == null)
                return Error(HttpStatusCode.BadRequest, "order", "not found.");

            // validate thats its paypal and has authid
            if (string.IsNullOrEmpty(order.AuthorizationTransactionId))
                return Error(HttpStatusCode.BadRequest, "order", "not valid for capture.");

            if (!string.IsNullOrEmpty(order.CaptureTransactionId) && order.CaptureTransactionResult?.ToUpperInvariant() == "COMPLETED")
                return Error(HttpStatusCode.BadRequest, "order", "order has already been captured.");

            // capture request from paypal
            var (capture, error) = _payPalProcessor.CaptureAuthorization(_swiftCoreSettings, order.AuthorizationTransactionId);

            if(!string.IsNullOrEmpty(error))
                return Error(HttpStatusCode.BadRequest, "paypal", error);

            //capture actions
            switch (capture?.Status?.ToLowerInvariant())
            {
                case "completed":
                    if (decimal.TryParse(capture.Amount?.Value, out var capturedAmount) && capturedAmount == Math.Round(order.OrderTotal, 2))
                    {
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.CaptureTransactionId = capture.Id;
                            order.CaptureTransactionResult = capture.Status;
                            _orderService.UpdateOrder(order);
                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    }
                    break;

                case "pending":
                case "declined":
                    order.CaptureTransactionResult = $"{capture.Status}. {capture.StatusDetails?.Reason}";
                    order.OrderStatus = Nop.Core.Domain.Orders.OrderStatus.Pending;
                    _orderService.UpdateOrder(order);
                    _orderProcessingService.CheckOrderStatus(order);
                    break;

                case "refunded":
                    if (_orderProcessingService.CanRefundOffline(order))
                        _orderProcessingService.RefundOffline(order);
                    break;
            }

            return new RawJsonActionResult(JsonConvert.SerializeObject(capture));
        }
    }
}
