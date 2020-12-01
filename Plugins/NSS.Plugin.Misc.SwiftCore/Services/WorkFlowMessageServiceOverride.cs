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
        public IList<int> SendNSSCustomerRegisteredNotificationMessage(Nop.Core.Domain.Customers.Customer customer, int languageId, int? erpId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            // config
            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftCoreSettings>(storeScope);

            var messageTemplates = GetActiveMessageTemplates(Helpers.Constants.ApprovalMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                // include erp id as token
                tokens.Add(new Token("Customer.ErpId", erpId?.ToString()));

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = string.IsNullOrEmpty(swiftPortalOverrideSettings.ApproverMailBox) ? emailAccount.Email : swiftPortalOverrideSettings.ApproverMailBox;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }


        /// <summary>
        /// Sends 'New customer' notification message to a NSS
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public IList<int> SendNSSCustomerRegisteredNotificationMessage(int regId, string email, string fullName, bool existingCustomer, int languageId)
        {
            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            // config
            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftCoreSettings>(storeScope);

            var messageTemplates = GetActiveMessageTemplates(Helpers.Constants.ApprovalMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            //_messageTokenProvider.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                // include erp id as token
                tokens.Add(new Token("Customer.FullName", fullName));
                tokens.Add(new Token("Customer.Email", email));
                tokens.Add(new Token("Customer.ExistingCustomer", existingCustomer));
                tokens.Add(new Token("Customer.ApprovalUrl", string.Format(Helpers.Constants.StoreRegistrationConfirmationUrl, store.Url, regId)));

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = string.IsNullOrEmpty(swiftPortalOverrideSettings.ApproverMailBox) ? emailAccount.Email : swiftPortalOverrideSettings.ApproverMailBox;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }


        public IList<int> SendChangePasswordEmailNotificationMessage(Nop.Core.Domain.Customers.Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            // config
            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftCoreSettings>(storeScope);

            var messageTemplates = GetActiveMessageTemplates(Helpers.Constants.ChangePasswordMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                // include erp id as token
                //tokens.Add(new Token("Customer.ErpId", erpId?.ToString()));

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = _customerService.GetCustomerFullName(customer);

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public IList<int> SendNewCustomerPendingApprovalEmailNotificationMessage(string email, string fullName, bool existingCustomer, int languageId)
        {
            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplates = GetActiveMessageTemplates(Helpers.Constants.NewCustomerPendingApprovalMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            //_messageTokenProvider.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                // include erp id as token
                //tokens.Add(new Token("Customer.ErpId", erpId?.ToString()));

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = email;
                var toName = fullName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public IList<int> SendNewCustomerRejectionEmailNotificationMessage(string email, string fullName, int languageId)
        {
            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplates = GetActiveMessageTemplates(Helpers.Constants.NewCustomerRejectionMessageTemplateName, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            //_messageTokenProvider.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                // include erp id as token
                //tokens.Add(new Token("Customer.ErpId", erpId?.ToString()));

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = email;
                var toName = fullName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        #endregion
    }
}
