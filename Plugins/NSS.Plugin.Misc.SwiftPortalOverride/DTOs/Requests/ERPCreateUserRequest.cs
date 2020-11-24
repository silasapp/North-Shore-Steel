using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Requests
{
    public class ERPCreateUserRequest
    {
        public string SwiftUserId { get; set; }
        public string Firstname { get; set; }
        public string LastName { get; set; }
        public string WorkEmail { get; set; }
        public string Phone { get; set; }
        //public string CellPhone { get; set; }
        //public string WorkPhone { get; set; }

        public string CompanyName { get; set; }
        public string IsExistingCustomer { get; set; }
        public string PreferredLocationid { get; set; }
        public string HearAboutUs { get; set; }
        public string Other { get; set; }
        public string ItemsForNextProject { get; set; }

    }
}
