using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Data;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSS.Plugin.Misc.SwiftCore.Configuration;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    public class QueuedEmailServiceOverride : QueuedEmailService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<QueuedEmail> _queuedEmailRepository;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public QueuedEmailServiceOverride(IEventPublisher eventPublisher,
            IRepository<QueuedEmail> queuedEmailRepository, 
            ISettingService settingService,
            IStoreContext storeContext) : base(eventPublisher, queuedEmailRepository)
        {
            _eventPublisher = eventPublisher;
            _queuedEmailRepository = queuedEmailRepository;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>        
        public override void InsertQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException(nameof(queuedEmail));

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftCoreSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandBox = swiftPortalOverrideSettings.UseSandBox,
                TestEmailAddress = swiftPortalOverrideSettings.TestEmailAddress,
                ApproverMailBox = swiftPortalOverrideSettings.ApproverMailBox,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (model.UseSandBox)
            {
                if (!string.IsNullOrEmpty(model.TestEmailAddress))
                {
                    // use test email
                    var testEmails = model.TestEmailAddress.Trim().Split(';');

                    // add sandbox email header
                    queuedEmail.Body = GetSandBoxEmailHeader(queuedEmail.To, queuedEmail.CC, queuedEmail.Bcc) + queuedEmail.Body;

                    // assign first email as to address
                    queuedEmail.To = testEmails[0];

                    // cc the rest
                    queuedEmail.CC = model.TestEmailAddress;
                }

            }

            _queuedEmailRepository.Insert(queuedEmail);

            //event notification
            _eventPublisher.EntityInserted(queuedEmail);
        }

        private string GetSandBoxEmailHeader(string toAddress, string ccAddress, string bccAddress)
        {
            var retval = new StringBuilder();

            var bcc = string.IsNullOrWhiteSpace(bccAddress)
                           ? null
                           : bccAddress.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var cc = string.IsNullOrWhiteSpace(ccAddress)
                        ? null
                        : ccAddress.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            retval.AppendLine("<br>FOR TEST PURPOSES ONLY:");
            retval.AppendLine("<br>-------------------------------------------------");
            retval.AppendLine("<br>TO:");


            retval.AppendFormat("<br/>{0}", toAddress);

            retval.AppendLine("<br>CC:");

            foreach (var address in cc ?? Enumerable.Empty<string>())
            {
                retval.AppendFormat("<br/>{0}", address);
            }

            retval.AppendLine("<br>BCC:");

            foreach (var address in bcc ?? Enumerable.Empty<string>())
            {
                retval.AppendFormat("<br/>{0}", address);
            }

            retval.AppendLine("<br>-------------------------------------------------<br>");

            return retval.ToString();
        }

        #endregion
    }
}
