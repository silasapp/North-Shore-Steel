using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Helpers
{
    public static class Constants
    {
        /// <summary>
        /// Gets NSS Wintrix key name
        /// </summary>
        public static string WintrixKeyAttribute => "ErpId";

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
