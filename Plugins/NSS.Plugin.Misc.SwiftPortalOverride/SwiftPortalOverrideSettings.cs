using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride
{
    public class SwiftPortalOverrideSettings : ISettings
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
    }
}
