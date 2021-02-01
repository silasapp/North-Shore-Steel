using NSS.Plugin.Misc.SwiftCore.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Order = PayPalCheckoutSdk.Orders.Order;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Logging;
using Microsoft.Net.Http.Headers;
using PayPalCheckoutSdk.Payments;
using System.Globalization;
using PayPal.v1.Webhooks;
using System.IO;
using System.Linq;
using NSS.Plugin.Misc.SwiftCore.Domain.PayPal;
using Nop.Services.Orders;
using Nop.Services.Common;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class PayPalProcessor
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public PayPalProcessor(IWorkContext workContext, ILogger logger)
        {
            _workContext = workContext;
            _logger = logger;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check whether the plugin is configured
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <returns>Result</returns>
        private bool IsConfigured(SwiftCoreSettings settings)
        {
            //client id and secret are required to request services
            return !string.IsNullOrEmpty(settings?.PayPalClientId) &&
                (!string.IsNullOrEmpty(settings?.PayPalSecretKey) || settings.PayPalClientId.Equals("sb", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Handle function and get result
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="settings">Plugin settings</param>
        /// <param name="function">Function</param>
        /// <returns>Result; error message if exists</returns>
        private (TResult Result, string ErrorMessage) HandleFunction<TResult>(SwiftCoreSettings settings, Func<TResult> function)
        {
            try
            {
                //ensure that plugin is configured
                if (!IsConfigured(settings))
                    throw new NopException("Plugin not configured");

                //invoke function
                return (function(), default);
            }
            catch (Exception exception)
            {
                //get a short error message
                var message = exception.Message;
                var detailedException = exception;
                do
                {
                    detailedException = detailedException.InnerException;
                } while (detailedException?.InnerException != null);
                if (detailedException is HttpException httpException)
                {
                    var details = JsonConvert.DeserializeObject<ExceptionDetails>(httpException.Message);
                    message = !string.IsNullOrEmpty(details.Message)
                        ? details.Message
                        : (!string.IsNullOrEmpty(details.Name) ? details.Name : message);
                }

                //log errors
                var logMessage = $"{PaypalDefaults.SystemName} error: {System.Environment.NewLine}{message}";
                _logger.Error(logMessage, exception, _workContext.CurrentCustomer);

                return (default, message);
            }
        }

        /// <summary>
        /// Handle request to checkout services and get result
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="settings">Plugin settings</param>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        private TResult HandleCheckoutRequest<TRequest, TResult>(SwiftCoreSettings settings, TRequest request)
            where TRequest : HttpRequest where TResult : class
        {
            //prepare common request params
            request.Headers.Add(HeaderNames.UserAgent, PaypalDefaults.UserAgent);
            request.Headers.Add("PayPal-Partner-Attribution-Id", PaypalDefaults.PartnerCode);
            request.Headers.Add("Prefer", "return=representation");

            //execute request
            var environment = settings.PayPalUseSandbox
                ? new SandboxEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPalEnvironment
                : new LiveEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPalEnvironment;
            var client = new PayPalHttpClient(environment);
            var response = client.Execute(request)
                ?? throw new NopException("No response from the service.");

            //return the results if necessary
            if (typeof(TResult) == typeof(object))
                return default;

            var result = response.Result?.Result<TResult>()
                ?? throw new NopException("No response from the service.");

            return result;
        }

        /// <summary>
        /// Handle request to core services and get result
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="settings">Plugin settings</param>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        private TResult HandleCoreRequest<TRequest, TResult>(SwiftCoreSettings settings, TRequest request)
            where TRequest : BraintreeHttp.HttpRequest where TResult : class
        {
            //prepare common request params
            request.Headers.Add(HeaderNames.UserAgent, PaypalDefaults.UserAgent);
            request.Headers.Add("PayPal-Partner-Attribution-Id", PaypalDefaults.PartnerCode);
            request.Headers.Add("Prefer", "return=representation");

            //execute request
            var environment = settings.PayPalUseSandbox
                ? new PayPal.Core.SandboxEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPal.Core.PayPalEnvironment
                : new PayPal.Core.LiveEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPal.Core.PayPalEnvironment;
            var client = new PayPal.Core.PayPalHttpClient(environment);
            var response = client.Execute(request)
                ?? throw new NopException("No response from the service.");

            //return the results if necessary
            if (typeof(TResult) == typeof(object))
                return default;

            var result = response.Result?.Result<TResult>()
                ?? throw new NopException("No response from the service.");

            return result;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Authorize a previously created order
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="orderId">Order id</param>
        /// <returns>Authorized order; error message if exists</returns>
        public (Order Order, string ErrorMessage) Authorize(SwiftCoreSettings settings, string orderId)
        {
            return HandleFunction(settings, () =>
            {
                var request = new OrdersAuthorizeRequest(orderId).RequestBody(new AuthorizeRequest());
                return HandleCheckoutRequest<OrdersAuthorizeRequest, Order>(settings, request);
            });
        }

        /// <summary>
        /// Capture a previously created order
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="orderId">Order id</param>
        /// <returns>Captured order; error message if exists</returns>
        public (Order Order, string ErrorMessage) Capture(SwiftCoreSettings settings, string orderId)
        {
            return HandleFunction(settings, () =>
            {
                var request = new OrdersCaptureRequest(orderId).RequestBody(new OrderActionRequest());
                return HandleCheckoutRequest<OrdersCaptureRequest, Order>(settings, request);
            });
        }

        /// <summary>
        /// Capture an authorization
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="authorizationId">Authorization id</param>
        /// <returns>Capture details; error message if exists</returns>
        public (PayPalCheckoutSdk.Payments.Capture Capture, string ErrorMessage) CaptureAuthorization
            (SwiftCoreSettings settings, string authorizationId)
        {
            return HandleFunction(settings, () =>
            {
                var request = new AuthorizationsCaptureRequest(authorizationId).RequestBody(new CaptureRequest());
                return HandleCheckoutRequest<AuthorizationsCaptureRequest, PayPalCheckoutSdk.Payments.Capture>(settings, request);
            });
        }

        /// <summary>
        /// Void an authorization
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="authorizationId">Authorization id</param>
        /// <returns>Voided order; Error message if exists</returns>
        public (object Order, string ErrorMessage) Void(SwiftCoreSettings settings, string authorizationId)
        {
            return HandleFunction(settings, () =>
            {
                var request = new AuthorizationsVoidRequest(authorizationId);
                return HandleCheckoutRequest<AuthorizationsVoidRequest, object>(settings, request);
            });
        }

        /// <summary>
        /// Refund a captured payment
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="captureId">Capture id</param>
        /// <param name="currency">Currency code</param>
        /// <param name="amount">Amount to refund</param>
        /// <returns>Refund details; error message if exists</returns>
        public (PayPalCheckoutSdk.Payments.Refund refund, string errorMessage) Refund
            (SwiftCoreSettings settings, string captureId, string currency, decimal? amount = null)
        {
            return HandleFunction(settings, () =>
            {
                var refundRequest = new RefundRequest();
                if (amount.HasValue)
                    refundRequest.Amount = new PayPalCheckoutSdk.Payments.Money { CurrencyCode = currency, Value = amount.Value.ToString("0.00", CultureInfo.InvariantCulture) };
                var request = new CapturesRefundRequest(captureId).RequestBody(refundRequest);
                return HandleCheckoutRequest<CapturesRefundRequest, PayPalCheckoutSdk.Payments.Refund>(settings, request);
            });
        }

        /// <summary>
        /// Get access token
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <returns>Access token; error message if exists</returns>
        public (AccessToken AccessToken, string ErrorMessage) GetAccessToken(SwiftCoreSettings settings)
        {
            //try to get access token
            return HandleFunction(settings, () =>
            {
                var environment = settings.PayPalUseSandbox
                    ? new SandboxEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPalEnvironment
                    : new LiveEnvironment(settings.PayPalClientId, settings.PayPalSecretKey) as PayPalEnvironment;
                var request = new AccessTokenRequest(environment);
                return HandleCheckoutRequest<AccessTokenRequest, AccessToken>(settings, request);
            });
        }

        ///// <summary>
        ///// Create webhook that receive events for the subscribed event types
        ///// </summary>
        ///// <param name="settings">Plugin settings</param>
        ///// <param name="url">Webhook listener URL</param>
        ///// <returns>Webhook; error message if exists</returns>
        //public (Webhook webhook, string errorMessage) CreateWebHook(SwiftCoreSettings settings, string url)
        //{
        //    return HandleFunction(settings, () =>
        //    {
        //        //check whether webhook already exists
        //        var webhooks = HandleCoreRequest<WebhookListRequest, WebhookList>(settings, new WebhookListRequest());
        //        if (!string.IsNullOrEmpty(settings.WebhookId))
        //        {
        //            var webhookById = webhooks?.Webhooks?.FirstOrDefault(webhook => webhook.Id.Equals(settings.WebhookId));
        //            if (webhookById != null)
        //                return webhookById;
        //        }
        //        var webhookByUrl = webhooks?.Webhooks?.FirstOrDefault(webhook => webhook.Url.Equals(url, StringComparison.InvariantCultureIgnoreCase));
        //        if (webhookByUrl != null)
        //            return webhookByUrl;

        //        //or try to create the new one if doesn't exist
        //        var request = new WebhookCreateRequest().RequestBody(new Webhook
        //        {
        //            EventTypes = PaypalDefaults.WebhookEventNames.Select(name => new EventType { Name = name }).ToList(),
        //            Url = url
        //        });
        //        return HandleCoreRequest<WebhookCreateRequest, Webhook>(settings, request);
        //    });
        //}

        ///// <summary>
        ///// Delete webhook
        ///// </summary>
        ///// <param name="settings">Plugin settings</param>
        //public void DeleteWebhook(SwiftCoreSettings settings)
        //{
        //    HandleFunction(settings, () =>
        //    {
        //        var request = new WebhookDeleteRequest(settings.WebhookId);
        //        return HandleCoreRequest<WebhookDeleteRequest, object>(settings, request);
        //    });
        //}

        ///// <summary>
        ///// Handle webhook request
        ///// </summary>
        ///// <param name="settings">Plugin settings</param>
        ///// <param name="request">HTTP request</param>
        //public void HandleWebhook(SwiftCoreSettings settings, Microsoft.AspNetCore.Http.HttpRequest request)
        //{
        //    HandleFunction(settings, async () =>
        //    {
        //        //get request details
        //        var rawRequestString = string.Empty;
        //        using (var streamReader = new StreamReader(request.Body))
        //            rawRequestString = await streamReader.ReadToEndAsync();

        //        //define a local function to validate the webhook event and get an appropriate resource
        //        object getWebhookResource<TResource>() where TResource : class
        //        {
        //            //verify webhook event data
        //            var webhookEvent = JsonConvert.DeserializeObject<Event<TResource>>(rawRequestString);
        //            var verifyRequest = new WebhookVerifySignatureRequest<TResource>().RequestBody(new VerifyWebhookSignature<TResource>
        //            {
        //                AuthAlgo = request.Headers["PAYPAL-AUTH-ALGO"],
        //                CertUrl = request.Headers["PAYPAL-CERT-URL"],
        //                TransmissionId = request.Headers["PAYPAL-TRANSMISSION-ID"],
        //                TransmissionSig = request.Headers["PAYPAL-TRANSMISSION-SIG"],
        //                TransmissionTime = request.Headers["PAYPAL-TRANSMISSION-TIME"],
        //                WebhookId = settings.WebhookId,
        //                WebhookEvent = webhookEvent
        //            });
        //            var result = HandleCoreRequest<WebhookVerifySignatureRequest<TResource>, VerifyWebhookSignatureResponse>(settings, verifyRequest);

        //            // This would be hard to always get it to success, as the result is dependent on time of webhook sent.
        //            // As long as we get a 200 response, we should be fine.
        //            //see details here https://github.com/paypal/PayPal-NET-SDK/commit/16e5bebfd4021d0888679e526cdf1f4f19527f3e#diff-ee79bcc68a8451d30522e1d9b2c5bc13R36
        //            return result != null ? webhookEvent?.Resource : default;
        //        }

        //        //try to get webhook resource type
        //        var webhookResourceType = JsonConvert.DeserializeObject<Event<Order>>(rawRequestString).ResourceType?.ToLowerInvariant();

        //        //and now get specific webhook resource
        //        var webhookResource = webhookResourceType switch
        //        {
        //            "checkout-order" => getWebhookResource<Order>(),
        //            "authorization" => getWebhookResource<PayPalCheckoutSdk.Payments.Authorization>(),
        //            "capture" => getWebhookResource<PayPalCheckoutSdk.Payments.Capture>(),
        //            "refund" => getWebhookResource<PayPalCheckoutSdk.Payments.Refund>(),
        //            _ => null
        //        };
        //        if (webhookResource == null)
        //            return false;

        //        var orderReference = webhookResource is Order payPalOrder
        //            ? payPalOrder.PurchaseUnits?.FirstOrDefault()?.CustomId
        //            : JsonConvert.DeserializeObject<Event<ExtendedWebhookResource>>(rawRequestString).Resource?.CustomId;
        //        if (!Guid.TryParse(orderReference, out var orderGuid))
        //            return false;

        //        var order = _orderService.GetOrderByGuid(orderGuid);
        //        if (order == null)
        //            return false;

        //        _orderService.InsertOrderNote(new Nop.Core.Domain.Orders.OrderNote()
        //        {
        //            OrderId = order.Id,
        //            Note = $"Webhook details: {System.Environment.NewLine}{rawRequestString}",
        //            DisplayToCustomer = false,
        //            CreatedOnUtc = DateTime.UtcNow
        //        });

        //        //authorization actions
        //        var authorization = webhookResource as PayPalCheckoutSdk.Payments.Authorization;
        //        switch (authorization?.Status?.ToLowerInvariant())
        //        {
        //            case "created":
        //                if (decimal.TryParse(authorization.Amount?.Value, out var authorizedAmount) && authorizedAmount == Math.Round(order.OrderTotal, 2))
        //                {
        //                    //all is ok, so authorize order
        //                    if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
        //                    {
        //                        order.AuthorizationTransactionId = authorization.Id;
        //                        order.AuthorizationTransactionResult = authorization.Status;
        //                        _orderService.UpdateOrder(order);
        //                        _orderProcessingService.MarkAsAuthorized(order);
        //                    }
        //                }
        //                break;

        //            case "denied":
        //            case "expired":
        //            case "pending":
        //                order.CaptureTransactionResult = authorization.Status;
        //                order.OrderStatus = Nop.Core.Domain.Orders.OrderStatus.Pending;
        //                _orderService.UpdateOrder(order);
        //                _orderProcessingService.CheckOrderStatus(order);
        //                break;

        //            case "voided":
        //                if (_orderProcessingService.CanVoidOffline(order))
        //                {
        //                    order.AuthorizationTransactionId = authorization.Id;
        //                    order.AuthorizationTransactionResult = authorization.Status;
        //                    _orderService.UpdateOrder(order);
        //                    _orderProcessingService.VoidOffline(order);
        //                }
        //                break;
        //        }

        //        //capture actions
        //        var capture = webhookResource as PayPalCheckoutSdk.Payments.Capture;
        //        switch (capture?.Status?.ToLowerInvariant())
        //        {
        //            case "completed":
        //                if (decimal.TryParse(capture.Amount?.Value, out var capturedAmount) && capturedAmount == Math.Round(order.OrderTotal, 2))
        //                {
        //                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
        //                    {
        //                        order.CaptureTransactionId = capture.Id;
        //                        order.CaptureTransactionResult = capture.Status;
        //                        _orderService.UpdateOrder(order);
        //                        _orderProcessingService.MarkOrderAsPaid(order);
        //                    }
        //                }
        //                break;

        //            case "pending":
        //            case "declined":
        //                order.CaptureTransactionResult = $"{capture.Status}. {capture.StatusDetails?.Reason}";
        //                order.OrderStatus = Nop.Core.Domain.Orders.OrderStatus.Pending;
        //                _orderService.UpdateOrder(order);
        //                _orderProcessingService.CheckOrderStatus(order);
        //                break;

        //            case "refunded":
        //                if (_orderProcessingService.CanRefundOffline(order))
        //                    _orderProcessingService.RefundOffline(order);
        //                break;
        //        }

        //        //refund actions
        //        var refund = webhookResource as PayPalCheckoutSdk.Payments.Refund;
        //        switch (refund?.Status?.ToLowerInvariant())
        //        {
        //            case "completed":
        //                var refundIds = _genericAttributeService.GetAttribute<List<string>>(order, PaypalDefaults.RefundIdAttributeName)
        //                    ?? new List<string>();
        //                if (!refundIds.Contains(refund.Id))
        //                {
        //                    if (decimal.TryParse(refund.Amount?.Value, out var refundedAmount))
        //                    {
        //                        if (_orderProcessingService.CanPartiallyRefundOffline(order, refundedAmount))
        //                        {
        //                            _orderProcessingService.PartiallyRefundOffline(order, refundedAmount);
        //                            refundIds.Add(refund.Id);
        //                            _genericAttributeService.SaveAttribute(order, PaypalDefaults.RefundIdAttributeName, refundIds);
        //                        }
        //                    }
        //                }
        //                break;
        //        }

        //        return true;
        //    });
        //}


        #endregion
    }
}
