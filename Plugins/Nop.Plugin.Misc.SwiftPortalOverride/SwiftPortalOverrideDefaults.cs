using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.SwiftPortalOverride
{
    public static class SwiftPortalOverrideDefaults
    {
        /// <summary>
        /// Gets a plugin system name
        /// </summary>
        public static string SystemName => "Misc.SwiftPortalOverride";

        /// <summary>
        /// Gets NSS Wintrix key name
        /// </summary>
        public static string WintrixKeyAttribute => "WintrixId";

        #region NSSCustomerAttributes           
        public static string IsExistingCustomerAttribute => "Existing Customer";        
        public static string PreferredLocationIdAttribute => "PreferredLocation";        
        public static string HearAboutUsAttribute => "HearAboutUs";        
        public static string OtherAttribute => "Other";        
        public static string ItemsForNextProjectAttribute => "ItemsForNextProject"; 
        public static string NSSApprovedAttribute => "NSSApproved"; 
        #endregion
    }
}
