using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.SwiftPortalOverride.Services.Customer
{
    public class CustomerRegistrationServiceOverride : CustomerRegistrationService
    {
        CustomerSettings _customerSettings;
        #region constructor
        public CustomerRegistrationServiceOverride(CustomerSettings customerSettings, ICustomerService customerService, IEncryptionService encryptionService, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, ILocalizationService localizationService, INewsLetterSubscriptionService newsLetterSubscriptionService, IRewardPointService rewardPointService, IStoreService storeService, IWorkContext workContext, IWorkflowMessageService workflowMessageService, RewardPointsSettings rewardPointsSettings) : base(customerSettings, customerService, encryptionService, eventPublisher, genericAttributeService, localizationService, newsLetterSubscriptionService, rewardPointService, storeService, workContext, workflowMessageService, rewardPointsSettings)
        {
            _customerSettings = customerSettings;
        }

        #endregion
    }
}
