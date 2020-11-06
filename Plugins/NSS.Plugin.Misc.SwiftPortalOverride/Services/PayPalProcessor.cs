using NSS.Plugin.Misc.SwiftCore.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Order = PayPalCheckoutSdk.Orders.Order;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftPortalOverride.Domains.PayPal;
using Nop.Core;
using Nop.Services.Logging;
using Microsoft.Net.Http.Headers;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    public class PayPalProcessor
    {
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        public PayPalProcessor(IWorkContext workContext, ILogger logger)
        {
            _workContext = workContext;
            _logger = logger;
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
                ////ensure that plugin is configured
                //if (!IsConfigured(settings))
                //    throw new NopException("Plugin not configured");

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
    }
}
