using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.SwiftPortalOverride
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
    }
}
