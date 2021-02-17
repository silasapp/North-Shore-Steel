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
        Beaumont = 1,
        Houston = 2,
        None = 0
    }

    public enum HearAboutUs
    {
        Salesperson = 1,
        Website = 2,
        Email = 3,
        [Display(Name = "Social Media")]
        SocialMedia = 4,
        [Display(Name = "Word of Mouth")]
        WordOfMouth = 5,
        [Display(Name = "Trade Publication")]
        TradePublication = 6,
        Other = 7


    }
}

