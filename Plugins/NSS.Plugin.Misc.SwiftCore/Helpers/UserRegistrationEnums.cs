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
        Salesperson = 1,
        Website = 2,
        Email = 3,
        [Display(Name = "Social media")]
        SocialMedia = 4,
        Word_of_mouth = 5,
        [Display(Name = "Trade publication")]
        TradePublication = 6,
        Other = 7


    }
}

