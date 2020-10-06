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
            ConfigureUserSettings();

        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a Swift User
        /// </summary>
        /// <param name="request">User request object</param>
        /// <returns>Exchange rates</returns>
        public NSSCreateUserResponse CreateSwiftUser(NSSCreateUserRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (useMock)
            {
                request = new NSSCreateUserRequest
                {
                    CompanyName = "ACME",
                    Firstname = "Jessica",
                    LastName = "Jones",
                    HearAboutUs = "WebSite",
                    IsExistingCustomer = "0",
                    ItemsForNextProject = "Test Something",
                    Other = "",
                    Phone = "+12463775637",
                    PreferredLocationid = "1",
                    SwiftUserId = "3",
                    WorkEmail = "jessicajones@test11.com",
                };
            }

            // log request
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateUser -> {request.WorkEmail}", JsonConvert.SerializeObject(request));

            //initialize
            var retVal = new NSSCreateUserResponse();
            var content = string.Empty;

            if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pword))
            {
                _logger.Warning("Swift Api provider - Create user", new Exception("NSS API attributes not configured correctly."));
                return retVal;
            }

            //create swift user
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                //get token
                var token = GetNSSToken(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                    _logger.Warning($"NSS.CreateUser -> {request.WorkEmail}", new Exception("NSS token returned empty"));
                    return retVal;
                }

                // create user resource
                var requestUrl = $"{ _baseUrl}users";
                if (!_baseUrl.EndsWith('/')) requestUrl = $"{ _baseUrl}/users";

                //body params
                var param = new Dictionary<string, string>
                {
                    { "swiftUserId", request.SwiftUserId },
                    { "firstName", request.Firstname },
                    { "lastName", request.LastName },
                    { "workEmail", request.WorkEmail },
                    { "phone", request.Phone },
                    { "companyName", request.CompanyName },
                    { "isExistingCustomer", request.IsExistingCustomer },
                    { "preferredLocationId", request.PreferredLocationid },
                    { "hearAboutUs", request.HearAboutUs },
                    { "other", request.Other },
                    { "itemsForNextProject", request.ItemsForNextProject },
                };

                var req = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = new FormUrlEncodedContent(param) };
                req.Headers.Add("Authorization", $"Bearer {token}");

                var response = httpClient.SendAsync(req).Result;

                // throw error if not successful
                response.EnsureSuccessStatusCode();

                content = response.Content.ReadAsStringAsync().Result;
                retVal = JsonConvert.DeserializeObject<NSSCreateUserResponse>(content);
            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.CreateUser -> {request.WorkEmail}", ex);
            }

            // log request
            _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Debug, $"NSS.CreateUser details => email: {request.WorkEmail}, wintrixId: {retVal.WitnrixId?.ToString() ?? "empty"}", $"resp content ==>{content ?? "empty"}");

            return retVal;
        }

        /// <summary>
        /// Get Request Token
        /// </summary>
        /// <param name="httpClient">Http client instance</param>
        /// <param name="baseUrl">NSS API base url</param>
        /// <param name="user">NSS API auth username</param>
        /// <param name="pword">NSS API auth password</param>
        /// <returns>Exchange rates</returns>
        public string GetNSSToken(HttpClient httpClient)
        {
            var retVal = string.Empty;
            var requestUrl = $"{ _baseUrl}authenticate";
            if (!_baseUrl.EndsWith('/')) requestUrl = $"{ _baseUrl}/authenticate";

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
                var response = httpClient.PostAsync(requestUrl, content).Result;

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


        public List<Order> GetRecentOrders(int companyId)
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
                    //get token
                    var token = GetNSSToken(client);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.GetRecentOrders -> ", new Exception("NSS token returned empty"));
                    }

                    var requestUrl = $"{ _baseUrl}companies/{companyId}/orders/recent";
                    if (!_baseUrl.EndsWith('/')) requestUrl = $"{ _baseUrl}/companies/{companyId}/orders/recent";


                    client.DefaultRequestHeaders.Accept.Clear();
                    var req = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    req.Headers.Add("Authorization", $"Bearer {token}");
                    var response = client.SendAsync(req).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<List<Order>>(responseBody);

                }
                return retVal;
            }
            catch (Exception ex)
            {
                _logger.Error($"NSS.GetRecentOrders ->", ex);
                return retVal;
            }
        }

        public List<Invoice> GetRecentInvoices(int companyId)
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
                    //get token
                    var token = GetNSSToken(client);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.Warning($"NSS.GetRecentInvoices -> ", new Exception("NSS token returned empty"));
                    }

                    var requestUrl = $"{ _baseUrl}companies/{companyId}/invoices/recent";
                    if (!_baseUrl.EndsWith('/')) requestUrl = $"{ _baseUrl}/companies/{companyId}/invoices/recent";


                    client.DefaultRequestHeaders.Accept.Clear();
                    var req = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    req.Headers.Add("Authorization", $"Bearer {token}");
                    var response = client.SendAsync(req).Result;

                    // throw error if not successful
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    retVal = JsonConvert.DeserializeObject<List<Invoice>>(responseBody);

                }
                return retVal;
            }
            catch (Exception ex)
            {

                _logger.Error($"NSS.GetRecentInvoices ->", ex);
                return retVal;
            }
        }


        private void ConfigureUserSettings ()
        {
            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftPortalOverrideSettings>(storeScope);

            _baseUrl = swiftPortalOverrideSettings.NSSApiBaseUrl;
            _user = swiftPortalOverrideSettings.NSSApiAuthUsername;
            _pword = swiftPortalOverrideSettings.NSSApiAuthPassword;


        }


        #endregion
    }
}
