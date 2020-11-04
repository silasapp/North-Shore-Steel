﻿using Newtonsoft.Json;
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

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    public class NSSApiProvider
    {
        #region Fields

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private string _baseUrl;
        private string _user;
        private string _pword;

        #endregion

        #region Ctor

        public NSSApiProvider(IHttpClientFactory httpClientFactory,
            ILocalizationService localizationService,
            ILogger logger,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _httpClientFactory = httpClientFactory;
            _localizationService = localizationService;
            _logger = logger;
            _settingService = settingService;
            _storeContext = storeContext;

            // configure settings
            ConfigureNSSApiSettings();

        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a Swift User
        /// </summary>
        /// <param name="request">User request object</param>
        /// <returns>Exchange rates</returns>
        public NSSCreateUserResponse CreateNSSUser(NSSCreateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //initialize
            var retVal = new NSSCreateUserResponse();
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
                    retVal = JsonConvert.DeserializeObject<NSSCreateUserResponse>(respContent);
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

        public NSSCalculateShippingResponse GetShippingRate(NSSCalculateShippingRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (useMock)
            {
                var resp = new NSSCalculateShippingResponse
                {
                    Allowed = true,
                    DeliveryDate = "2020-10-20",
                    DistanceMiles = 100,
                    PickupTime = "4pm"
                };

                return resp;
            }


            //initialize
            var retVal = new NSSCalculateShippingResponse();
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
                        _logger.Warning($"NSS.CalculateShipping -> {request.DestinationAddressLine1}", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

                    // create user resource
                    var resource = "/shipping-charges";

                    //body params
                    var param = request.ToKeyValue();

                    var content = new FormUrlEncodedContent(param);

                    var response = httpClient.PostAsync(resource, content).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    respContent = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<NSSCalculateShippingResponse>(respContent);
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.CalculateShipping -> {request.DestinationAddressLine1}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateUser details => email: {request.DestinationAddressLine1}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");

            return retVal;
        }

        public NSSGetCompanyCreditBalance GetCompanyCreditBalance(int companyId, bool useMock = false)
        {
            //initialize
            var retVal = new NSSGetCompanyCreditBalance();
            var respContent = string.Empty;

            if (useMock)
            {
                var resp = new NSSGetCompanyCreditBalance
                {
                    CreditAmount = (decimal)1500.00
                };

                return resp;
            }

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
                        _logger.Warning($"NSS.GetCompanyAccountCreditBal -> {companyId}", new Exception("NSS token returned empty"));
                        return retVal;
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

                    // acc credit bal resource
                    var resource = $"/{companyId}/credit";

                    var response = httpClient.GetAsync(resource).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    respContent = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<NSSGetCompanyCreditBalance>(respContent);
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.GetCompanyAccountCredBal -> {companyId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.GetAccountCreditBal details => companyid: {companyId}", $"resp content ==> {respContent ?? "empty"}");

            return retVal;
        }

        public void CreateNSSOrder(int companyId, NSSCreateOrderRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (useMock)
            { 
                return;
            }

            //initialize
            var respContent = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - Create user", new Exception("NSS API attributes not configured correctly."));
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
                        _logger.Warning($"NSS.CreateOrder -> {request.OrderId}", new Exception("NSS token returned empty"));
                    }

                    //httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", token);

                    // create user resource
                    var resource = $"/{companyId}/orders";

                    //body params
                    var param = request.ToKeyValue();

                    var content = new FormUrlEncodedContent(param);

                    var response = httpClient.PostAsync(resource, content).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.CreateOrder -> {request.OrderId}", ex);
            }

            // log request & resp
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateOrder details => orderid: {request.OrderId}", $"resp content ==> {respContent ?? "empty"}, request ==> {JsonConvert.SerializeObject(request)}");
        }

        private void ConfigureNSSApiSettings()
        {
            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftCoreSettings>(storeScope);

            _baseUrl = swiftPortalOverrideSettings.NSSApiBaseUrl;
            _user = swiftPortalOverrideSettings.NSSApiAuthUsername;
            _pword = swiftPortalOverrideSettings.NSSApiAuthPassword;
        }

        #endregion
    }
}
