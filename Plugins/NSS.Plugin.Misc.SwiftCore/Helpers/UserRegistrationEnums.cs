using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Helpers
{
    public enum UserRegistrationStatus
    {
        Rejected = -1,
        Pending = 0,
        Approved = 1
    }

    public enum PreferredLocation
    {
        Houston = 1,
        Beaumont = 2
    }

    public enum HearAboutUs
    {
        [Display(Name = "Social Media")]
        SocialMedia = 1,
        Website = 2,
        Other = 3


    }
}

