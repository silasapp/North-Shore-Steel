using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Configuration;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class WorkFlowMessageServiceOverride : WorkflowMessageService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public WorkFlowMessageServiceOverride(CommonSettings commonSettings,
            EmailAccountSettings emailAccountSettings,
            IAddressService addressService,
            IAffiliateService affiliateService,
            ICustomerService customerService,
            IEmailAccountService emailAccountService,
            IEventPublisher eventPublisher,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IOrderService orderService,
            IProductService productService,
            IQueuedEmailService queuedEmailService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer,
            ISettingService settingService) : base(commonSettings,
             emailAccountSettings,
             addressService,
             affiliateService,
             customerService,
             emailAccountService,
             eventPublisher,
             languageService,
             localizationService,
             messageTemplateService,
             messageTokenProvider,
             orderService,
             productService,
             queuedEmailService,
             storeContext,
             storeService,
             tokenizer)
        {

            _eventPublisher = eventPublisher;
            _messageTokenProvider = messageTokenProvider;
            _storeContext = storeContext;
            _settingService = settingService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends 'New customer' notification message to a NSS
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public async Task<IList<int>> SendNSSCustomerRegisteredNotificationMessageAsync(Nop.Core.Domain.Customers.Customer customer, int languageId, int? erpId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await  _storeContext.GetCurrentStoreAsync();
            languageId =  await EnsureLanguageIsActiveAsync(languageId, store.Id);

            // config
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            
            var swiftPortalOverrideSettings = await _settingService.LoadSettingAsync<SwiftCoreSettings>(storeScope);

            var messageTemplates = await GetActiveMessageTemplatesAsync(Helpers.Constants.ApprovalMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>(); 
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount =  GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId).Result;

                var tokens = new List<Token>(commonTokens);
                 _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                // include erp id as token
                tokens.Add(new Token("Customer.ErpId", erpId?.ToString()));

                //event notification
                 _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = string.IsNullOrEmpty(swiftPortalOverrideSettings.ApproverMailBox) ? emailAccount.Email : swiftPortalOverrideSettings.ApproverMailBox;
                var toName = emailAccount.DisplayName;

                return  SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName).Result;
            }).ToList();
        }



        /// <summary>
        /// Sends 'New customer' notification message to a NSS
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public async Task<IList<int>> SendNSSCustomerRegisteredNotificationMessageAsync(int regId, string email, string fullName, bool existingCustomer, int languageId)
        {
            var store =await _storeContext.GetCurrentStoreAsync();
            languageId =await EnsureLanguageIsActiveAsync(languageId, store.Id);

            // config
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var swiftPortalOverrideSettings = await _settingService.LoadSettingAsync<SwiftCoreSettings>(storeScope);

            var messageTemplates = await GetActiveMessageTemplatesAsync(Helpers.Constants.ApprovalMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            //_messageTokenProvider.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount =  GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId).Result;

                var tokens = new List<Token>(commonTokens);
               _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                // include erp id as token
                tokens.Add(new Token("Customer.FullName", fullName));
                tokens.Add(new Token("Customer.Email", email));
                tokens.Add(new Token("Customer.ExistingCustomer", existingCustomer));
                tokens.Add(new Token("Customer.RegistrationConfirmationUrl", string.Format(Helpers.Constants.StoreRegistrationConfirmationUrl, store.Url, regId)));

                //event notification
                _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = string.IsNullOrEmpty(swiftPortalOverrideSettings.ApproverMailBox) ? emailAccount.Email : swiftPortalOverrideSettings.ApproverMailBox;
                var toName = emailAccount.DisplayName;

                return  SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName).Result;
            }).ToList();
        }


        public async  Task<IList<int>> SendChangePasswordEmailNotificationMessageAsync(Nop.Core.Domain.Customers.Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store =  await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            // config
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var swiftPortalOverrideSettings =await _settingService.LoadSettingAsync<SwiftCoreSettings>(storeScope);

            var messageTemplates = await GetActiveMessageTemplatesAsync(Helpers.Constants.ChangePasswordMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
                await  _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return messageTemplates.Select( messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId).Result;

                var tokens = new List<Token>(commonTokens);
                     _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                // include erp id as token
                //tokens.Add(new Token("Customer.ErpId", erpId?.ToString()));

                //event notification
                _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName =  _customerService.GetCustomerFullNameAsync(customer).Result;

                return  SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName).Result;
            }).ToList();
        }

        public async Task<IList<int>> SendNewCustomerPendingApprovalEmailNotificationMessageAsync(string email, string fullName, bool existingCustomer, int languageId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(Helpers.Constants.NewCustomerPendingApprovalMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            //_messageTokenProvider.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount =  GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId).Result;

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                // include erp id as token
                //tokens.Add(new Token("Customer.ErpId", erpId?.ToString()));

                //event notification
                  _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = email;
                var toName = fullName;

                return  SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName).Result;
            }).ToList();
        }

        public async Task<IList<int>> SendNewCustomerRejectionEmailNotificationMessageAsync(string email, string fullName, int languageId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(Helpers.Constants.NewCustomerRejectionMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            //_messageTokenProvider.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId).Result;

                var tokens = new List<Token>(commonTokens);
                 _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                // include erp id as token
                //tokens.Add(new Token("Customer.ErpId", erpId?.ToString()));

                //event notification
                 _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = email;
                var toName = fullName;

                return  SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName).Result;
            }).ToList();
        }

        public async Task<IList<int>> SendCustomerWelcomeMessageAsync(Customer customer, string password, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates =await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.CustomerWelcomeMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
           await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount =  GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId).Result;

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                tokens.Add(new Token("Customer.TempPassword", password));

                //event notification
                _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName =  _customerService.GetCustomerFullNameAsync(customer).Result;

                return  SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName).Result;
            }).ToList();
        }

        #endregion
    }
}
