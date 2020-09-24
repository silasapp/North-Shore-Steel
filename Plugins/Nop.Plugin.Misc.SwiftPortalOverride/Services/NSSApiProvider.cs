using Nop.Core;
using Nop.Core.Http;
using Nop.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;
using Nop.Plugin.Misc.SwiftPortalOverride.Models;
using Nop.Plugin.Misc.SwiftPortalOverride.Requests;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Nop.Plugin.Misc.SwiftPortalOverride.Services
{
    public class NSSApiProvider
    {
        #region Fields

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

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
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a Swift User
        /// </summary>
        /// <param name="request">User request object</param>
        /// <returns>Exchange rates</returns>
        public SwiftCreateUserResponse CreateSwiftUser(SwiftCreateUserRequest request, bool useMock = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (useMock)
            {
                request = new SwiftCreateUserRequest
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
                    WorkEmail = "jessicajones@test.com",
                };
            }

            // log request
            _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Create NSS user request -> ", JsonSerializer.Serialize(request));

            //initialize
            var retVal = new SwiftCreateUserResponse();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftPortalOverrideSettings>(storeScope);

            string baseUrl = swiftPortalOverrideSettings.NSSApiBaseUrl;
            string user = swiftPortalOverrideSettings.NSSApiAuthUsername;
            string pword = swiftPortalOverrideSettings.NSSApiAuthPassword;

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pword))
            {
                _logger.Error("Swift Api provider - Create user", new Exception("NSS API attributes not configured correctly."));
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
                var token = GetSwiftToken(httpClient, baseUrl, user, pword);
                _logger.Error("Swift Api provider - Create user", new Exception($"NSS token => {token}"));

                if (string.IsNullOrEmpty(token))
                {
                    _logger.Error("Swift Api provider - Create user", new Exception("NSS token returned empty")); 
                    return retVal;
                }

                // create user resource
                var requestUrl = $"{ baseUrl}users";
                if (!baseUrl.EndsWith('/')) requestUrl = $"{ baseUrl}/users";

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

                using var responseStream = response.Content.ReadAsStreamAsync().Result;
                retVal = JsonSerializer.DeserializeAsync<SwiftCreateUserResponse>(responseStream).Result;
            }
            catch (Exception ex)
            {
                _logger.Error("Swift Api provider - Create user", ex);
            }

            // log request
            _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Create NSS user response -> wintrixId==>", retVal.WintrixId?.ToString() ?? "empty");

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
        public string GetSwiftToken(HttpClient httpClient, string baseUrl, string user, string pword)
        {
            var retVal = string.Empty;
            var requestUrl = $"{ baseUrl}authenticate";
            if (!baseUrl.EndsWith('/')) requestUrl = $"{ baseUrl}/authenticate";

            //body params
            var param = new Dictionary<string, string>
            {
                { "username", user },
                { "password", pword }
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


        #endregion
    }
}
