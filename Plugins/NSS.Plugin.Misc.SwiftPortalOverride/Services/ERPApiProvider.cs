using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Http;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Requests;
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
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests;
using Newtonsoft.Json.Linq;
using NSS.Plugin.Misc.SwiftPortalOverride.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    public class ERPApiProvider
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

        public ERPApiProvider(IHttpClientFactory httpClientFactory,
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
        public string GetNSSToken(HttpClient httpClient)
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
                _logger.Error("Swift Api provider - Authenticate", ex);
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
        public ERPCreateUserResponse CreateNSSUser(ERPCreateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new ERPCreateUserResponse();
            var respContent = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - Create user", new Exception("NSS API attributes not configured correctly."));
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
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.CreateUser -> {request.WorkEmail}", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

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

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.CreateUser -> {request.WorkEmail}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateUser details => email: {request.WorkEmail}, wintrixId: {retVal.WitnrixId?.ToString() ?? "empty"}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return retVal;
        }


        public void UpdateNSSUser(int erpId, ERPUpdateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var respContent = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - Update user", new Exception("NSS API attributes not configured correctly."));
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);

                    //get token
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.UpdateNSSUser erpId -> {erpId}", new Exception("NSS token returned empty"));
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

                    // create user resource
                    var resource = $"/users/{erpId}";

                    //body params
                    var param = request.ToKeyValue();

                    var content = new FormUrlEncodedContent(param);

                    var response = httpClient.PutAsync(resource, content).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.UpdateUser erpId-> {erpId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.UpdateNSSUser details => erpId: {erpId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request.ToKeyValue())}");
        }

        #endregion

        #region Orders API

        public List<Order> GetRecentOrders(string ERPCompanyId)
        {
            var retVal = new List<Order>();
            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - Get Recent Orders", new Exception("NSS API attributes not configured correctly."));
                return retVal;
            }


            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    //get token
                    var token = GetNSSToken(client);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.GetRecentOrders -> ", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var resource = $"/companies/{ERPCompanyId}/orders/recent";

                    client.DefaultRequestHeaders.Accept.Clear();

                    var response = client.GetAsync(resource).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<List<Order>>(responseBody);

                }
            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.GetRecentOrders ->", ex);
            }

            return retVal;
        }

        public ERPCreateOrderResponse CreateNSSOrder(int companyId, ERPCreateOrderRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (useMock)
            {
                return new ERPCreateOrderResponse { NSSOrderNo = 1462176 };
            }

            //initialize
            var retVal = new ERPCreateOrderResponse();
            var respContent = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - CreateNSSOrder", new Exception("NSS API attributes not configured correctly."));
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
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.CreateNSSOrder -> {request.OrderId}", new Exception("NSS token returned empty"));
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

                    // create user resource
                    var resource = $"companies/{companyId}/orders";

                    //body params
                    var param = request.ToKeyValue();

                    var content = new FormUrlEncodedContent(param);

                    var response = httpClient.PostAsync(resource, content).Result;

                    respContent = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                        retVal = JsonConvert.DeserializeObject<ERPCreateOrderResponse>(respContent);
                    else
                        throw new NopException($"An error ocurred while placing order: {respContent}", $"request => {JsonConvert.SerializeObject(request)}, result: {respContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.CreateOrder -> {request.OrderId}, request ==> {JsonConvert.SerializeObject(request)}", ex);
                throw;
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateOrder details => orderid: {request.OrderId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return retVal;
        }

        public List<ERPSearchOrdersResponse> SearchOpenOrders(int companyId, ERPSearchOrdersRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new List<ERPSearchOrdersResponse>();
            var respContent = string.Empty;

            if (!useMock)
                if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
                {
                    _logger.Warning("Swift Api provider - SearchOpenOrders", new Exception("NSS API attributes not configured correctly."));
                    return retVal;
                }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    if (useMock)
                        httpClient.BaseAddress = new Uri("https://private-anon-bd88ec445e-nssswift.apiary-mock.com");
                    else
                        httpClient.BaseAddress = new Uri(_baseUrl);

                    if (!useMock)
                    {
                        //get token
                        var token = GetNSSToken(httpClient);

                        if (string.IsNullOrEmpty(token))
                        {
                            _logger.Warning($"NSS.SearchOpenOrders -> {companyId}", new Exception("NSS token returned empty"));
                            return retVal;
                        }

                        //httpClient.DefaultRequestHeaders.Authorization =
                        //    new AuthenticationHeaderValue("Bearer", token);
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
            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.SearchOpenOrders -> {companyId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.SearchOpenOrders details => companyId: {companyId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return retVal;
        }

        public List<ERPSearchOrdersResponse> SearchClosedOrders(int companyId, ERPSearchOrdersRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new List<ERPSearchOrdersResponse>();
            var respContent = string.Empty;

            if (!useMock)
                if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
                {
                    _logger.Warning("Swift Api provider - SearchClosedOrders", new Exception("NSS API attributes not configured correctly."));
                    return retVal;
                }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    if (useMock)
                        httpClient.BaseAddress = new Uri("https://private-anon-bd88ec445e-nssswift.apiary-mock.com");
                    else
                        httpClient.BaseAddress = new Uri(_baseUrl);

                    if (!useMock)
                    {
                        //get token
                        var token = GetNSSToken(httpClient);

                        if (string.IsNullOrEmpty(token))
                        {
                            _logger.Warning($"NSS.SearchClosedOrders -> {companyId}", new Exception("NSS token returned empty"));
                            return retVal;
                        }

                        //httpClient.DefaultRequestHeaders.Authorization =
                        //    new AuthenticationHeaderValue("Bearer", token);
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
            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.SearchClosedOrders -> {companyId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.SearchClosedOrders details => companyId: {companyId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return retVal;
        }

        public (string, ERPGetOrderDetailsResponse) GetOrderDetails(int companyId, int erpOrderId)
        {
            //initialize
            ERPGetOrderDetailsResponse retVal = null;
            var respContent = string.Empty;
            string error = string.Empty;
            var token = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - GetOrderDetails", new Exception("NSS API attributes not configured correctly."));
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
                    token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.GetOrderDetails companyId -> {companyId}, orderId -> {erpOrderId}", new Exception("NSS token returned empty"));
                        return ("", retVal);
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);


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
                _logger.Error($"NSS.GetOrderDetails companyId -> {companyId}, orderId -> {erpOrderId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetOrderDetails => companyId: {companyId}, orderId: {erpOrderId}", $"resp content ==> {respContent ?? "empty"}");

            return (token, retVal);
        }

        public (string, List<ERPGetOrderMTRResponse>) GetOrderMTRs(int companyId, int erpOrderId, int? lineItemId = null)
        {
            //initialize
            var retVal = new List<ERPGetOrderMTRResponse>();
            var respContent = string.Empty;
            var error = string.Empty;
            var token = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - GetOrderMTRs", new Exception("NSS API attributes not configured correctly."));
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
                    token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.GetOrderMTRs companyId -> {companyId}, orderId -> {erpOrderId}", new Exception("NSS token returned empty"));
                        return ("", retVal);
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);


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
                _logger.Error($"NSS.GetOrderMTRs companyId -> {companyId}, orderId -> {erpOrderId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetOrderMTRs => companyId: {companyId}, orderId: {erpOrderId}", $"resp content ==> {respContent ?? "empty"}");

            return (token, retVal);
        }

        #endregion

        #region Invoices API

        public List<Invoice> GetRecentInvoices(string ERPCompanyId)
        {
            var retVal = new List<Invoice>();
            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - Get Recent Invoices", new Exception("NSS API attributes not configured correctly."));
                return retVal;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    //get token
                    var token = GetNSSToken(client);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.GetRecentInvoices -> ", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    var resource = $"/companies/{ERPCompanyId}/invoices/recent";

                    client.DefaultRequestHeaders.Accept.Clear();

                    var response = client.GetAsync(resource).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<List<Invoice>>(responseBody);

                }
            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.GetRecentInvoices ->", ex);
            }

            return retVal;
        }

        public (string, List<ERPSearchInvoicesResponse>) SearchOpenInvoices(int companyId, ERPSearchInvoicesRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new List<ERPSearchInvoicesResponse>();
            var respContent = string.Empty;
            var token = string.Empty;

            if (!useMock)
                if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
                {
                    _logger.Warning("Swift Api provider - SearchOpenInvoices", new Exception("NSS API attributes not configured correctly."));
                    return ("", retVal);
                }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    if (useMock)
                        httpClient.BaseAddress = new Uri("https://private-anon-bd88ec445e-nssswift.apiary-mock.com");
                    else
                        httpClient.BaseAddress = new Uri(_baseUrl);

                    if (!useMock)
                    {
                        //get token
                        token = GetNSSToken(httpClient);

                        if (string.IsNullOrEmpty(token))
                        {
                            _logger.Warning($"NSS.SearchOpenInvoices -> {companyId}", new Exception("NSS token returned empty"));
                            return ("", retVal);
                        }

                        //httpClient.DefaultRequestHeaders.Authorization =
                        //    new AuthenticationHeaderValue("Bearer", token);
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

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.SearchOpenInvoices -> {companyId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.SearchOpenInvoices details => email: {companyId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return (token, retVal);
        }

        public (string, List<ERPSearchInvoicesResponse>) SearchClosedInvoices(int companyId, ERPSearchInvoicesRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));


            //initialize
            var retVal = new List<ERPSearchInvoicesResponse>();
            var respContent = string.Empty;
            var token = string.Empty;

            if (!useMock)
                if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
                {
                    _logger.Warning("Swift Api provider - SearchClosedInvoices", new Exception("NSS API attributes not configured correctly."));
                    return ("", retVal);
                }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    if (useMock)
                        httpClient.BaseAddress = new Uri("https://private-anon-bd88ec445e-nssswift.apiary-mock.com");
                    else
                        httpClient.BaseAddress = new Uri(_baseUrl);

                    if (!useMock)
                    {
                        //get token
                        token = GetNSSToken(httpClient);

                        if (string.IsNullOrEmpty(token))
                        {
                            _logger.Warning($"NSS.SearchClosedInvoices -> {companyId}", new Exception("NSS token returned empty"));
                            return ("", retVal);
                        }

                        //httpClient.DefaultRequestHeaders.Authorization =
                        //    new AuthenticationHeaderValue("Bearer", token);
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

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.SearchClosedInvoices -> {companyId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.SearchClosedInvoices => companyId: {companyId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return (token, retVal);
        }

        #endregion

        #region Companies API

        public CompanyInfo GetCompanyInfo(string erpCompanyId)
        {
            var retVal = new CompanyInfo();
            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - Get Recent Invoices", new Exception("NSS API attributes not configured correctly."));
                return retVal;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    //get token
                    var token = GetNSSToken(client);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.GetCompanyInfo -> ", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    var resource = $"/companies/{erpCompanyId}";

                    client.DefaultRequestHeaders.Accept.Clear();

                    var response = client.GetAsync(resource).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<CompanyInfo>(responseBody);

                }
            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.GetCompanyInfo ->", ex);
            }

            return retVal;
        }

        public ERPGetCompanyCreditBalance GetCompanyCreditBalance(int companyId, bool useMock = false)
        {
            //initialize
            var retVal = new ERPGetCompanyCreditBalance();
            var respContent = string.Empty;

            if (useMock)
            {
                var resp = new ERPGetCompanyCreditBalance
                {
                    CreditAmount = (decimal)1500.00
                };

                return resp;
            }

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - GetCompanyCreditBalance", new Exception("NSS API attributes not configured correctly."));
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
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.GetCompanyCreditBalance -> {companyId}", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

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
                _logger.Error($"NSS.GetCompanyCreditBalance -> {companyId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetCompanyCreditBalance details => companyid: {companyId}", $"resp content ==> {respContent ?? "empty"}");

            return retVal;
        }

        public (Dictionary<string, bool>, string) GetCompanyNotificationPreferences(int userId, int companyId)
        {
            string error = string.Empty;
            var result = new Dictionary<string, bool>();

            var respContent = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";
                _logger.Warning("Swift Api provider - GetCompanyNotificationPreferences", new Exception(error));
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
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        error = "NSS token returned empty";
                        _logger.Warning($"NSS.GetCompanyNotificationPreferences companyId-> {companyId}", new Exception(error));
                        return (result, error);
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

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
                _logger.Error($"NSS.GetCompanyNotificationPreferences -> userId => {userId}, companyId -> {companyId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetCompanyNotificationPreferences => userId: {userId}, companyId => {companyId}", $"resp content ==> {respContent ?? "empty"}");

            return (result, error);
        }

        public (Dictionary<string, bool>, string) UpdateCompanyNotificationPreferences(int userId, int companyId, IDictionary<string, bool> preferences)
        {
            string error = string.Empty;
            var result = new Dictionary<string, bool>();

            //initialize
            var respContent = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";
                _logger.Warning("Swift Api provider - UpdateCompanyNotificationPreferences", new Exception(error));
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
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        error = "NSS token returned empty";
                        _logger.Warning($"NSS.UpdateCompanyNotificationPreferences", new Exception(error));
                        return (result, error);
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

                    //  resource
                    var resource = $"users/{userId}/companies/{companyId}/notifications";

                    var param = preferences.ToKeyValue();

                    var response = httpClient.PutAsync(resource, new FormUrlEncodedContent(param)).Result;

                    respContent = response.Content.ReadAsStringAsync().Result;

                    // throw error if not successful
                    if (response.IsSuccessStatusCode)
                        result = ERPGetNotificationPreferencesResponse.FromJson(respContent);
                    else
                        throw new NopException($"An error occured when updating user company notification preferences : api status => {response.StatusCode}, message => {respContent}", respContent);

                }

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.UpdateCompanyNotificationPreferences -> userId => {userId}, companyId -> {companyId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.UpdateCompanyNotificationPreferences => userId: {userId}", $"resp content ==> {respContent ?? "empty"}, companyId  ==> {companyId}");

            return (result, error);
        }

        #endregion

        #region Shipping API

        public ERPCalculateShippingResponse GetShippingRate(ERPCalculateShippingRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (useMock)
            {
                var resp = new ERPCalculateShippingResponse
                {
                    Allowed = true,
                    DeliveryDate = "2020-10-20",
                    DistanceMiles = 100,
                    PickupTime = "4pm",
                    ShippingCost = 20
                };

                return resp;
            }


            //initialize
            var retVal = new ERPCalculateShippingResponse();
            var respContent = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - GetShippingRate", new Exception("NSS API attributes not configured correctly."));
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
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.CalculateShipping -> {request.DestinationAddressLine1}", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

                    //  resource
                    var resource = "/shipping-charges";

                    //body params
                    var param = request.ToKeyValue();

                    var content = new FormUrlEncodedContent(param);

                    var response = httpClient.PostAsync(resource, content).Result;

                    respContent = response.Content.ReadAsStringAsync().Result;

                    // throw error if not successful
                    if (response.IsSuccessStatusCode)
                        retVal = JsonConvert.DeserializeObject<ERPCalculateShippingResponse>(respContent);

                    else
                        throw new NopException($"An error occured when getting shipping rate : {respContent}", respContent);

                }

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.CalculateShipping -> request => {JsonConvert.SerializeObject(request)}", ex);

                throw;
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetShippingRate => email: {request.DestinationAddressLine1}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return retVal;
        }

        #endregion

        #region UserRegistration
        public (ERPRegisterUserResponse, string) CreateUserRegistration(ERPRegisterUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new ERPRegisterUserResponse();
            var respContent = string.Empty;
            string error = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";

                _logger.Warning("Swift Api provider - CreateUserRegistration", new NopException(error));

                return (retVal, error);
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);

                    //get token
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        error = "NSS token returned empty";

                        _logger.Warning($"NSS.CreateUserRegistration -> {request.SwiftRegistrationId}", new Exception(error));

                        return (retVal, error);
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);


                    // create user resource
                    var resource = $"/userregistration";

                    //body params
                    var param = request.ToKeyValue();

                    var content = new FormUrlEncodedContent(param);

                    var response = httpClient.PostAsync(resource, content).Result;

                    //// throw error if not successful
                    //response.EnsureSuccessStatusCode();

                    respContent = response.Content.ReadAsStringAsync().Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        error = respContent;
                        throw new NopException($"Request returned status of {response.StatusCode.ToString()} and message: {respContent}, request ==> {JsonConvert.SerializeObject(request)}");
                    }

                    retVal = ERPRegisterUserResponse.FromJson(respContent) ?? new ERPRegisterUserResponse();
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.CreateUserRegistration -> {request.SwiftRegistrationId}, request ==> {JsonConvert.SerializeObject(request)}", ex);
                return (retVal, error);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateUserRegistration => regId: {request.SwiftRegistrationId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return (retVal, error);
        }

        public string RejectUserRegistration(int regId)
        {
            //initialize
            var respContent = string.Empty;
            var error = string.Empty;


            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";

                _logger.Warning("Swift Api provider - RejectUserRegistration", new NopException(error));

                return error;
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);

                    //get token
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        error = "NSS token returned empty";

                        _logger.Warning($"NSS.RejectUserRegistration -> {regId}", new NopException(error));

                        return error;
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

                    // create user resource
                    var resource = $"/userregistration/{regId}/reject";

                    var response = httpClient.PutAsync(resource, null).Result;

                    respContent = response.Content.ReadAsStringAsync().Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        error = respContent;
                        throw new NopException($"Request returned status of {response.StatusCode.ToString()} and message: {respContent}");
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.RejectUserRegistration -> {regId}", ex);
                return error;
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.RejectUserRegistration => regId: {regId}", $"resp content ==> {respContent ?? "empty"}");

            return error;
        }

        public (ERPApproveUserRegistrationResponse, string) ApproveUserRegistration(int regId)
        {
            //initialize
            var retVal = new ERPApproveUserRegistrationResponse();
            var respContent = string.Empty;
            var error = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                error = "NSS API attributes not configured correctly.";

                _logger.Warning("Swift Api provider - ApproveUserRegistration", new NopException(error));

                return (retVal, error);
            }

            //create swift user
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                {
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.BaseAddress = new Uri(_baseUrl);

                    //get token
                    var token = GetNSSToken(httpClient);

                    if (string.IsNullOrEmpty(token))
                    {
                        error = "NSS token returned empty";

                        _logger.Warning($"NSS.ApproveUserRegistration -> {regId}", new Exception(error));

                        return (retVal, error);
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);


                    // create user resource
                    var resource = $"/userregistration/{regId}/approve";

                    var response = httpClient.PutAsync(resource, null).Result;

                    respContent = response.Content.ReadAsStringAsync().Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        error = respContent;
                        throw new NopException($"Request returned status of {response.StatusCode.ToString()} and message: {respContent}");
                    }

                    retVal = ERPApproveUserRegistrationResponse.FromJson(respContent) ?? new ERPApproveUserRegistrationResponse();
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.ApproveUserRegistration -> {regId}", ex);

                return (retVal, error);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.ApproveUserRegistration => regId: {regId}", $"resp content ==> {respContent ?? "empty"}");

            return (retVal, error);
        }
        #endregion


        #endregion
    }
}
