using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Configuration
{
    public class SwiftCoreSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the UseSandbox attribute
        /// </summary>
        public bool UseSandBox { get; set; }

        /// <summary>
        /// Gets or sets the email address for Swift Approver role
        /// </summary>
        public string ApproverMailBox { get; set; }

        /// <summary>
        /// Gets or sets the email address to be used in sandbox mode
        /// </summary>
        public string TestEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the NSS API base url
        /// </summary>
        public string NSSApiBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the NSS API authentication username
        /// </summary>
        public string NSSApiAuthUsername { get; set; }

        /// <summary>
        /// Gets or sets the NSS API authentication password
        /// </summary>
        public string NSSApiAuthPassword { get; set; }

        /// <summary>
        /// Gets or sets the Storage Account Name
        /// </summary>
        public string StorageAccountName { get; set; }

        /// <summary>
        /// Gets or sets the Storage Account Key
        /// </summary>
        public string StorageAccountKey { get; set; }

        /// <summary>
        /// Gets or sets the Storage Container Name
        /// </summary>
        public string StorageContainerName { get; set; }

        #region PayPal
        /// <summary>
        /// Gets or sets PayPal environment
        /// </summary>
        public bool PayPalUseSandbox { get; set; }

        /// <summary>
        /// Gets or sets the PayPal ClientId
        /// </summary>
        public string PayPalClientId { get; set; }

        /// <summary>
        /// Gets or sets the PayPal Secret Key
        /// </summary>
        public string PayPalSecretKey { get; set; }
        #endregion

        public string MarketingVideoUrl { get; set; }
        public string ApplyForCreditUrl { get; set; }
    }
}
