﻿using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Http;
using NSS.Plugin.Misc.SwiftCore.DTOs;
using NSS.Plugin.Misc.SwiftCore.DTOs.Responses;
using NSS.Plugin.Misc.SwiftCore.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using Newtonsoft.Json.Linq;
using NSS.Plugin.Misc.SwiftCore.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using NSS.Plugin.Misc.SwiftCore.Helpers;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class NSSApiService : IApiService
    {
        #region Fields

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly SwiftCoreSettings _settings;
        private readonly IStoreContext _storeContext;
        private string _baseUrl;
        private string _user;
        private string _pword;

        #endregion

        #region Ctor

        public NSSApiService(IHttpClientFactory httpClientFactory,
            ILocalizationService localizationService,
            ILogger logger,
            SwiftCoreSettings settings,
            IStoreContext storeContext)
        {
            _httpClientFactory = httpClientFactory;
            _localizationService = localizationService;
            _logger = logger;
            _settings = settings;
            _storeContext = storeContext;

            // configure settings
            _baseUrl = settings.NSSApiBaseUrl;
            _user = settings.NSSApiAuthUsername;
            _pword = settings.NSSApiAuthPassword;        
        }

        #endregion

        #region Methods


        #region Auth

        /// <summary>
        /// Get Request Token
        /// </summary>
        /// <param name="httpClient">Http client instance</param>
        /// <returns>token</returns>
        public async Task<string> GetNSSTokenAsync(HttpClient httpClient)
        {
            var retVal = string.Empty;
            var resource = "/authenticate";

            //body params
            var param = new Dictionary<string, string>
            {
                { "username", _user },
                { "password", _pword }
            };
            var content = new FormUrlEncodedContent(param);

            //get token
            try
            {
                var response = httpClient.PostAsync(resource, content).Result;

                // throw error if not successful
                response.EnsureSuccessStatusCode();

                var token = response.Content.ReadAsStringAsync().Result;
                if (token != null)
                    retVal = token;
            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync("Swift Api provider - Authenticate", ex);
            }

            return retVal;
        }

        #endregion

        #region User API

        /// <summary>
        /// Create a Swift User
        /// </summary>
        /// <param name="request">User request object</param>
        /// <returns>Exchange rates</returns>
        public async Task<ERPCreateUserResponse> CreateNSSUserAsync(ERPCreateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new ERPCreateUserResponse();
            var respContent = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
               await _logger.WarningAsync("Swift Api provider - Create user", new Exception("NSS API attributes not configured correctly."));
                return retVal;
            }

            //create swift user
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_baseUrl);

                var client = new CustomRestClient(new RestSharp.RestClient(_baseUrl));

                //get token
                var token = await GetNSSTokenAsync(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                  await  _logger.WarningAsync($"NSS.CreateUser -> {request.WorkEmail}", new Exception("NSS token returned empty"));
                    return retVal;
                }

                // create user resource
                var resource = "/users";

                //body params
                var param = new KeyValuePair<string, string>[]
                {
                        new KeyValuePair<string, string>("swiftUserId", request.SwiftUserId),
                        new KeyValuePair<string, string>("firstName", request.Firstname),
                        new KeyValuePair<string, string>("lastName", request.LastName),
                        new KeyValuePair<string, string>("workEmail", request.WorkEmail),
                        new KeyValuePair<string, string>("phone", request.Phone),
                        new KeyValuePair<string, string>("companyName", request.CompanyName),
                        new KeyValuePair<string, string>("isExistingCustomer", request.IsExistingCustomer),
                        new KeyValuePair<string, string>("preferredLocationId", request.PreferredLocationid),
                        new KeyValuePair<string, string>("hearAboutUs", request.HearAboutUs),
                        new KeyValuePair<string, string>("other", request.Other),
                        new KeyValuePair<string, string>("itemsForNextProject", request.ItemsForNextProject)
                };

                var content = new FormUrlEncodedContent(param);

                var response = httpClient.PostAsync(resource, content).Result;

                // throw error if not successful
                response.EnsureSuccessStatusCode();

                respContent = response.Content.ReadAsStringAsync().Result;
                retVal = JsonConvert.DeserializeObject<ERPCreateUserResponse>(respContent);


            }
            catch (Exception ex)
            {
              await  _logger.ErrorAsync($"NSS.CreateUser -> {request.WorkEmail}", ex);
            }

            // log request & resp
           await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateUser details => email: {request.WorkEmail}, wintrixId: {retVal.WitnrixId?.ToString() ?? "empty"}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return retVal;
        }


        public async Task UpdateNSSUserAsync(int erpId, ERPUpdateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var error = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
               await _logger.WarningAsync("Swift Api provider - Update user", new Exception("NSS API attributes not configured correctly."));
            }

            //create swift user
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_baseUrl);

                var client = new CustomRestClient(new RestSharp.RestClient(_baseUrl));

                //get token
                var token =await GetNSSTokenAsync(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                  await  _logger.WarningAsync($"NSS.UpdateNSSUser erpId -> {erpId}", new Exception("NSS token returned empty"));
                }

                // create user resource
                var resource = $"/users/{erpId}";

                (error, _) = await client.PutAsync<ERPUpdateUserRequest, string>(resource, request, token);

                // throw error if not successful
                if (!string.IsNullOrEmpty(error))
                {
                    error = $"An error has occured: {error}";
                }
            }
            catch (Exception ex)
            {
              await  _logger.ErrorAsync($"NSS.UpdateUser erpId-> {erpId}", ex);
            }

            // log request & resp
            var msg = string.IsNullOrEmpty(error) ? "empty" : error;
           await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.UpdateNSSUser details => erpId: {erpId}", $"resp content ==> {msg}, request ==> {JsonConvert.SerializeObject(request.ToKeyValue())}");
        }

        #endregion

        #region Orders API

        public async Task<ERPCreateOrderResponse> CreateNSSOrderAsync(int companyId, ERPCreateOrderRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new ERPCreateOrderResponse();
            string error = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
               await _logger.WarningAsync("Swift Api provider - CreateNSSOrder", new Exception("NSS API attributes not configured correctly."));
                return retVal;
            }

            //create swift user
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_baseUrl);

                var client = new CustomRestClient(new RestSharp.RestClient(_baseUrl));

                //get token
                var token = await GetNSSTokenAsync(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                   await _logger.WarningAsync($"NSS.CreateNSSOrder -> {request.OrderId}", new Exception("NSS token returned empty"));
                }

                // create user resource
                var resource = $"companies/{companyId}/orders";

                (error, retVal) = await client.PostAsync<ERPCreateOrderRequest, ERPCreateOrderResponse>(resource, request, token);

                if (string.IsNullOrEmpty(error))
                    retVal ??= new ERPCreateOrderResponse();
                else
                    throw new NopException($"An error ocurred while placing order: message = {error}", $"request => {JsonConvert.SerializeObject(request)}");

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync
                    ($"NSS.CreateOrder -> {request.OrderId}, request ==> {JsonConvert.SerializeObject(request)}", ex);
                throw;
            }

            // log request & resp
           await  _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateOrder details => orderid: {request.OrderId}", $"resp content ==> {JsonConvert.SerializeObject(retVal) ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return retVal;
        }

        public async Task<List<ERPSearchOrdersResponse>> SearchOpenOrdersAsync(int companyId, ERPSearchOrdersRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new List<ERPSearchOrdersResponse>();
            var respContent = string.Empty;

                if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
                {
                  await  _logger.WarningAsync("Swift Api provider - SearchOpenOrders", new Exception("NSS API attributes not configured correctly."));
                    return retVal;
                }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();


                    httpClient.BaseAddress = new Uri(_baseUrl);


                    //get token
                    var token = await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                       await _logger.WarningAsync($"NSS.SearchOpenOrders -> {companyId}", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                // create user resource
                var resource = $"/companies/{companyId}/orders/open";

                //query params
                var param = request.ToKeyValue();

                var response = httpClient.GetAsync(QueryHelpers.AddQueryString(resource, param)).Result;

                // throw error if not successful
                response.EnsureSuccessStatusCode();

                respContent = response.Content.ReadAsStringAsync().Result;

                retVal = ERPSearchOrdersResponse.FromJson(respContent) ?? new List<ERPSearchOrdersResponse>();

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.SearchOpenOrders -> {companyId}", ex);
            }

            // log request & resp
            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.SearchOpenOrders details => companyId: {companyId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return retVal;
        }

        public async Task<(string, List<ERPSearchOrdersResponse>)> SearchClosedOrdersAsync(int companyId, ERPSearchOrdersRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new List<ERPSearchOrdersResponse>();
            var respContent = string.Empty;
            var token = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
              await  _logger.WarningAsync("Swift Api provider - SearchClosedOrders", new Exception("NSS API attributes not configured correctly."));
                return ("", retVal);
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);


                    //get token
                    token =await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                       await _logger.WarningAsync($"NSS.SearchClosedOrders -> {companyId}", new Exception("NSS token returned empty"));
                        return ("", retVal);
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                // create user resource
                var resource = $"/companies/{companyId}/orders/closed";

                //body params
                var param = request.ToKeyValue();

                var response = httpClient.GetAsync(QueryHelpers.AddQueryString(resource, param)).Result;

                // throw error if not successful
                response.EnsureSuccessStatusCode();

                respContent = response.Content.ReadAsStringAsync().Result;

                retVal = ERPSearchOrdersResponse.FromJson(respContent) ?? new List<ERPSearchOrdersResponse>();

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.SearchClosedOrders -> {companyId}", ex);
            }

            // log request & resp
            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.SearchClosedOrders details => companyId: {companyId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return (token, retVal);
        }

        public async Task<(string, DTOs.Responses.ERPGetOrderDetailsResponse)> GetOrderDetailsAsync(int companyId, int erpOrderId)
        {
            //initialize
            ERPGetOrderDetailsResponse retVal = null;
            var respContent = string.Empty;
            string error = string.Empty;
            var token = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                await  _logger.WarningAsync("Swift Api provider - GetOrderDetails", new Exception("NSS API attributes not configured correctly."));
                return ("", retVal);

            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);


                    //get token
                    token = await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                      await  _logger.WarningAsync($"NSS.GetOrderDetails companyId -> {companyId}, orderId -> {erpOrderId}", new Exception("NSS token returned empty"));
                        return ("", retVal);
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);


                    // create user resource
                    var resource = $"/companies/{companyId}/orders/{erpOrderId}";

                    var response = httpClient.GetAsync(resource).Result;

                    respContent = response.Content.ReadAsStringAsync().Result;

                    // throw error if not successful
                    if (!response.IsSuccessStatusCode)
                    {
                        error = respContent;
                        throw new NopException($"NSS.GetOrderDetails Request returned status of {response.StatusCode.ToString()} and content: {respContent}");
                    }

                    retVal = ERPGetOrderDetailsResponse.FromJson(respContent);
                }

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.GetOrderDetails companyId -> {companyId}, orderId -> {erpOrderId}", ex);
            }

            // log request & resp
                await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetOrderDetails => companyId: {companyId}, orderId: {erpOrderId}", $"resp content ==> {respContent ?? "empty"}");

            return (token, retVal);
        }

        public async Task<(string, List<ERPGetOrderMTRResponse>)> GetOrderMTRsAsync(int companyId, int erpOrderId, int? lineItemId = null)
        {
            //initialize
            var retVal = new List<ERPGetOrderMTRResponse>();
            var respContent = string.Empty;
            var error = string.Empty;
            var token = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
               await _logger.WarningAsync("Swift Api provider - GetOrderMTRs", new Exception("NSS API attributes not configured correctly."));
                return ("", retVal);
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);


                    //get token
                    token = await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                       await _logger.WarningAsync($"NSS.GetOrderMTRs companyId -> {companyId}, orderId -> {erpOrderId}", new Exception("NSS token returned empty"));
                        return ("", retVal);
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);


                    // create user resource
                    var resource = $"/companies/{companyId}/orders/{erpOrderId}/mtrs";

                    HttpResponseMessage response;

                    if (lineItemId != null)
                        response = httpClient.GetAsync($"{resource}?lineItemId={lineItemId}").Result;
                    else
                        response = httpClient.GetAsync(resource).Result;

                    respContent = response.Content.ReadAsStringAsync().Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        error = respContent;
                        throw new NopException($"Request returned status of {response.StatusCode.ToString()} and message: {respContent}");
                    }

                    retVal = ERPGetOrderMTRResponse.FromJson(respContent) ?? new List<ERPGetOrderMTRResponse>();
                }

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.GetOrderMTRs companyId -> {companyId}, orderId -> {erpOrderId}", ex);
            }

            // log request & resp
           await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetOrderMTRs => companyId: {companyId}, orderId: {erpOrderId}", $"resp content ==> {respContent ?? "empty"}");

            return (token, retVal);
        }

        public async Task<(string, ERPGetOrderShippingDetailsResponse)> GetOrderShippingDetailsAsync(int companyId, int erpOrderId)
        {
            //initialize
            var retVal = new ERPGetOrderShippingDetailsResponse();
            var respContent = string.Empty;
            var error = string.Empty;
            var token = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
               await _logger.WarningAsync("Swift Api provider - GetOrderShippingDetails", new Exception("NSS API attributes not configured correctly."));
                return ("", retVal);
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);

                    //get token
                    token = await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                       await _logger.WarningAsync($"NSS.GetOrderShippingDetails companyId -> {companyId}, orderId -> {erpOrderId}", new Exception("NSS token returned empty"));
                        return ("NSS auth token returned empty", retVal);
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    // create user resource
                    var resource = $"/companies/{companyId}/orders/{erpOrderId}/shipments";

                    HttpResponseMessage response;

                    response = httpClient.GetAsync(resource).Result;

                    respContent = response.Content.ReadAsStringAsync().Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        error = respContent;
                        throw new NopException($"Request returned status of {response.StatusCode.ToString()} and message: {respContent}");
                    }

                    retVal = ERPGetOrderShippingDetailsResponse.FromJson(respContent) ?? new ERPGetOrderShippingDetailsResponse();
                }

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.GetOrderShippingDetails companyId -> {companyId}, orderId -> {erpOrderId}", ex);
            }

            // log request & resp
           await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetOrderShippingDetails => companyId: {companyId}, orderId: {erpOrderId}", $"resp content ==> {respContent ?? "empty"}");

            return (error, retVal);
        }

        #endregion

        #region Invoices API

        public async Task<(string, List<ERPSearchInvoicesResponse>)> SearchOpenInvoicesAsync(int companyId, ERPSearchInvoicesRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new List<ERPSearchInvoicesResponse>();
            var respContent = string.Empty;
            var token = string.Empty;

        
                if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
                {
                   await _logger.WarningAsync("Swift Api provider - SearchOpenInvoices", new Exception("NSS API attributes not configured correctly."));
                    return ("", retVal);
                }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();


                    httpClient.BaseAddress = new Uri(_baseUrl);

                    //get token
                    token =await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                       await _logger.WarningAsync($"NSS.SearchOpenInvoices -> {companyId}", new Exception("NSS token returned empty"));
                        return ("", retVal);
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                // create user resource
                var resource = $"/companies/{companyId}/invoices/open";

                //body params
                var param = request.ToKeyValue();

                var response = httpClient.GetAsync(QueryHelpers.AddQueryString(resource, param)).Result;

                // throw error if not successful
                response.EnsureSuccessStatusCode();

                respContent = response.Content.ReadAsStringAsync().Result;

                retVal = ERPSearchInvoicesResponse.FromJson(respContent) ?? new List<ERPSearchInvoicesResponse>();


            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.SearchOpenInvoices -> {companyId}", ex);
            }

            // log request & resp
            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.SearchOpenInvoices details => email: {companyId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return (token, retVal);
        }

        public async Task<(string, List<ERPSearchInvoicesResponse>)> SearchClosedInvoicesAsync(int companyId, ERPSearchInvoicesRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));


            //initialize
            var retVal = new List<ERPSearchInvoicesResponse>();
            var respContent = string.Empty;
            var token = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
               await _logger.WarningAsync("Swift Api provider - SearchClosedInvoices", new Exception("NSS API attributes not configured correctly."));
                return ("", retVal);
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);


                    //get token
                    token = await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                       await _logger.WarningAsync($"NSS.SearchClosedInvoices -> {companyId}", new Exception("NSS token returned empty"));
                        return ("", retVal);
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                // create user resource
                var resource = $"/companies/{companyId}/invoices/closed";

                //body params
                var param = request.ToKeyValue();

                var response = httpClient.GetAsync(QueryHelpers.AddQueryString(resource, param)).Result;

                // throw error if not successful
                response.EnsureSuccessStatusCode();

                respContent = response.Content.ReadAsStringAsync().Result;
                retVal = ERPSearchInvoicesResponse.FromJson(respContent) ?? new List<ERPSearchInvoicesResponse>();
            

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.SearchClosedInvoices -> {companyId}", ex);
            }

            // log request & resp
             await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.SearchClosedInvoices => companyId: {companyId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return (token, retVal);
        }

        #endregion

        #region Companies API

        public async Task<ERPCompanyInfoResponse> GetCompanyInfoAsync(string erpCompanyId)
        {
            var retVal = new ERPCompanyInfoResponse();
            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
               await _logger.WarningAsync("Swift Api provider - GetCompanyInfo", new Exception("NSS API attributes not configured correctly."));
                return retVal;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    //get token
                    var token = await GetNSSTokenAsync(client);

                    if (string.IsNullOrEmpty(token))
                    {
                       await _logger.WarningAsync($"NSS.GetCompanyInfo -> ", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var resource = $"/companies/{erpCompanyId}";

                    client.DefaultRequestHeaders.Accept.Clear();

                    var response = client.GetAsync(resource).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<ERPCompanyInfoResponse>(responseBody);

                }
            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.GetCompanyInfo ->", ex);
            }

            return retVal;
        }

        public async Task<ERPGetCompanyCreditBalance> GetCompanyCreditBalanceAsync(int companyId)
        {
            //initialize
            var retVal = new ERPGetCompanyCreditBalance();
            var respContent = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
               await _logger.WarningAsync("Swift Api provider - GetCompanyCreditBalance", new Exception("NSS API attributes not configured correctly."));
                return retVal;
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);

                    //get token
                    var token = await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                      await _logger.WarningAsync($"NSS.GetCompanyCreditBalance -> {companyId}", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    // acc credit bal resource
                    var resource = $"companies/{companyId}/credit";

                    var response = httpClient.GetAsync(resource).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    respContent = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<ERPGetCompanyCreditBalance>(respContent);
                }

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.GetCompanyCreditBalance -> {companyId}", ex);
            }

            // log request & resp
           await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetCompanyCreditBalance details => companyid: {companyId}", $"resp content ==> {respContent ?? "empty"}");

            return retVal;
        }

        public async Task<(Dictionary<string, bool>, string)> GetCompanyNotificationPreferencesAsync(int userId, int companyId)
        {
            string error = string.Empty;
            var result = new Dictionary<string, bool>();

            var respContent = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";
               await _logger.WarningAsync("Swift Api provider - GetCompanyNotificationPreferences", new Exception(error));
                return (result, error);
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);

                    //get token
                    var token = await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        error = "NSS token returned empty";
                       await _logger.WarningAsync($"NSS.GetCompanyNotificationPreferences companyId-> {companyId}", new Exception(error));
                        return (result, error);
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    //  resource
                    var resource = $"users/{userId}/companies/{companyId}/notifications";

                    var response = httpClient.GetAsync(resource).Result;

                    respContent = response.Content.ReadAsStringAsync().Result;

                    // throw error if not successful
                    if (response.IsSuccessStatusCode)
                        result = ERPGetNotificationPreferencesResponse.FromJson(respContent);
                    else
                        throw new NopException($"An error occured when getting user company notification preferences : {respContent}", respContent);

                }

            }
            catch (Exception ex)
            {
              await _logger.ErrorAsync($"NSS.GetCompanyNotificationPreferences -> userId => {userId}, companyId -> {companyId}", ex);
            }

            // log request & resp
           await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetCompanyNotificationPreferences => userId: {userId}, companyId => {companyId}", $"resp content ==> {respContent ?? "empty"}");

            return (result, error);
        }

        public async Task<(Dictionary<string, bool>, string)> UpdateCompanyNotificationPreferencesAsync(int userId, int companyId, IDictionary<string, bool> preferences)
        {
            string error = string.Empty;
            var result = new Dictionary<string, bool>();

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";
               await _logger.WarningAsync("Swift Api provider - UpdateCompanyNotificationPreferences", new Exception(error));
                return (result, error);
            }

            //create swift user
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_baseUrl);

                var client = new CustomRestClient(new RestSharp.RestClient(_baseUrl));

                //get token
                var token = await GetNSSTokenAsync(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                    error = "NSS token returned empty";
                   await _logger.WarningAsync($"NSS.UpdateCompanyNotificationPreferences", new Exception(error));
                    return (result, error);
                }

                //  resource
                var resource = $"users/{userId}/companies/{companyId}/notifications";

                (error, result) = await client.PutAsync<IDictionary<string, bool>, Dictionary<string, bool>>(resource, preferences, token);

                // throw error if not successful
                if (string.IsNullOrEmpty(error))
                    result ??= new Dictionary<string, bool>();
                else
                    error = $"An error occured when updating user company notification preferences : message = {error}";

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.UpdateCompanyNotificationPreferences -> userId => {userId}, companyId -> {companyId}", ex);
            }

            // log request & resp
            var msg = string.IsNullOrEmpty(error) ? JsonConvert.SerializeObject(result) : error;
           await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.UpdateCompanyNotificationPreferences => userId: {userId}", $"resp content ==> {msg}, companyId  ==> {companyId}");

            return (result, error);
        }

        public async Task<List<ERPGetCompanyStats>> GetCompanyStatsAsync(string companyId)
        {
            //initialize
            var retVal = new List<ERPGetCompanyStats>();
            var respContent = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
              await  _logger.WarningAsync("Swift Api provider - GetCompanyStats", new Exception("NSS API attributes not configured correctly."));
                return retVal;
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);

                    //get token
                    var token =await GetNSSTokenAsync(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                      await _logger.WarningAsync($"NSS.GetCompanyStats -> {companyId}", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var resource = $"companies/{companyId}/stats";

                    var response = httpClient.GetAsync(resource).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    respContent = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<List<ERPGetCompanyStats>>(respContent);
                }

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.GetCompanyStats -> {companyId}", ex);
            }

            // log request & resp
           await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetCompanyStats details => companyid: {companyId}", $"resp content ==> {respContent ?? "empty"}");

            return retVal;
        }

        #endregion

        #region Shipping API

        public async Task<(string, ERPCalculateShippingResponse)> GetShippingRateAsync(ERPCalculateShippingRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new ERPCalculateShippingResponse();
            string error = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";
                await _logger.WarningAsync("Swift Api provider - GetShippingRate", new Exception(error));
                return (error, retVal);
            }

            //calculate shipping rate
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_baseUrl);

                var client = new CustomRestClient(new RestSharp.RestClient(_baseUrl));

                //get token
                var token = await GetNSSTokenAsync(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                    error = "NSS token returned empty";
                  await  _logger.WarningAsync($"NSS.CalculateShipping -> {request.DestinationAddressLine1}", new Exception(error));
                    return (error, retVal);
                }

                //  resource
                var resource = "/shipping-charges";

                (error, retVal) = await client.PostAsync<ERPCalculateShippingRequest, ERPCalculateShippingResponse>(resource, request, token);

                // throw error if not successful
                if (string.IsNullOrEmpty(error))
                    retVal ??= new ERPCalculateShippingResponse();

                else
                    throw new NopException($"An error occured when getting shipping rate : {error}", error);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"NSS.CalculateShipping -> request => {JsonConvert.SerializeObject(request)}", ex);

                //throw;
            }

            // log request & resp
            var msg = string.IsNullOrEmpty(error) ? JsonConvert.SerializeObject(retVal) : error;
               await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetShippingRate => email: {request.DestinationAddressLine1}", $"resp content ==> {msg}, request ==> {JsonConvert.SerializeObject(request)}");

            return (error, retVal);
        }

        #endregion

        #region UserRegistration
        public async Task<(ERPRegisterUserResponse, string)> CreateUserRegistrationAsync(ERPRegisterUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new ERPRegisterUserResponse();
            string error = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";

                await _logger.WarningAsync("Swift Api provider - CreateUserRegistration", new NopException(error));

                return (retVal, error);
            }

            //create swift user
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_baseUrl);

                var client = new CustomRestClient(new RestSharp.RestClient(_baseUrl));

                //get token
                var token = await GetNSSTokenAsync(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                    error = "NSS token returned empty";

                   await _logger.WarningAsync($"NSS.CreateUserRegistration -> {request.SwiftRegistrationId}", new Exception(error));

                    return (retVal, error);
                }

                // create user resource
                var resource = $"/userregistration";

                //body params
                (error, retVal) = await client.PostAsync<ERPRegisterUserRequest, ERPRegisterUserResponse>(resource, request, token);

                if (string.IsNullOrEmpty(error))
                    retVal ??= new ERPRegisterUserResponse();

                else
                    error = $"An error occured registring user : {error}";
            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.CreateUserRegistration -> {request.SwiftRegistrationId}, request ==> {JsonConvert.SerializeObject(request)}", ex);
            }

            // log request & resp
            var msg = string.IsNullOrEmpty(error) ? JsonConvert.SerializeObject(retVal) : error;
             await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateUserRegistration => regId: {request.SwiftRegistrationId}", $"resp content ==> {msg}, request ==> {JsonConvert.SerializeObject(request)}");

            return (retVal, error);
        }

        public async Task<string> RejectUserRegistrationAsync(int regId)
        {
            //initialize
            var error = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";

               await _logger.WarningAsync("Swift Api provider - RejectUserRegistration", new NopException(error));

                return error;
            }

            //create swift user
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_baseUrl);

                var client = new CustomRestClient(new RestSharp.RestClient(_baseUrl));

                //get token
                var token = await GetNSSTokenAsync(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                    error = "NSS token returned empty";

                    await _logger.WarningAsync($"NSS.RejectUserRegistration -> {regId}", new NopException(error));

                    return error;
                }

                // create user resource
                var resource = $"/userregistration/{regId}/reject";

                (error, _) = await client.PutAsync<string, string>(resource, null, token);

                if (!string.IsNullOrEmpty(error))
                {
                    error = $"An error has occured: {error}";
                }
            }
            catch (Exception ex)
            {
                   await _logger.ErrorAsync($"NSS.RejectUserRegistration -> {regId}", ex);
            }

            // log request & resp
            var msg = string.IsNullOrEmpty(error) ? "empty" : error;
            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.RejectUserRegistration => regId: {regId}", $"resp content ==> {msg}");

            return error;
        }

        public async Task<(ERPApproveUserRegistrationResponse, string)> ApproveUserRegistrationAsync(int regId)
        {
            //initialize
            var retVal = new ERPApproveUserRegistrationResponse();
            var error = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";

                await _logger.WarningAsync("Swift Api provider - ApproveUserRegistration", new NopException(error));

                return (retVal, error);
            }

            //create swift user
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_baseUrl);

                var client = new CustomRestClient(new RestSharp.RestClient(_baseUrl));

                //get token
                var token = await GetNSSTokenAsync(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                    error = "NSS token returned empty";

                   await _logger.WarningAsync($"NSS.ApproveUserRegistration -> {regId}", new Exception(error));

                    return (retVal, error);
                }

                // create user resource
                var resource = $"/userregistration/{regId}/approve";

                (error, retVal) = await client .PutAsync<string, ERPApproveUserRegistrationResponse>(resource, null, token);

                if (!string.IsNullOrEmpty(error))
                {
                    error = $"An error has occured: {error}";
                }
                else
                {
                    retVal ??= new ERPApproveUserRegistrationResponse();
                }

            }
            catch (Exception ex)
            {
               await _logger.ErrorAsync($"NSS.ApproveUserRegistration -> {regId}", ex);
            }

            // log request & resp
            var msg = string.IsNullOrEmpty(error) ? JsonConvert.SerializeObject(retVal) : error;
           await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.ApproveUserRegistration => regId: {regId}", $"resp content ==> {msg}");

            return (retVal, error);
        }


        #endregion


        #endregion
    }
}