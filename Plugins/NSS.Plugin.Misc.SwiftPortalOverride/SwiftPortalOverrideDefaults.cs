using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride
{
    public static class SwiftPortalOverrideDefaults
    {
        /// <summary>
        /// Gets a plugin system name
        /// </summary>
        public static string SystemName => "Misc.SwiftPortalOverride";

        public static string ApprovalMessageTemplateName => "NewCustomer.Notification";

        public static string ChangePasswordMessageTemplateName => "Customer.ChangePassword";
        public static string NewCustomerPendingApprovalMessageTemplateName => "NewCustomer.PendingApproval";
        public static string NewCustomerRejectionMessageTemplateName => "NewCustomer.Rejection";

        public static string ERPCompanyCookieKey => "ERPCompanyId{0}";

        public static string NewUserEmailForPasswordChange => "CustomerFirstLoginEmail";
    }
}
