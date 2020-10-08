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
        public static string PreferredLocationIdAttribute => "Preferred Location";
        public static string HearAboutUsAttribute => "Hear About Us";
        public static string OtherAttribute => "Other";
        public static string ItemsForNextProjectAttribute => "Items For Next Project";
        public static string NSSApprovedAttribute => "NSSApproved";
        #endregion
    }
}
