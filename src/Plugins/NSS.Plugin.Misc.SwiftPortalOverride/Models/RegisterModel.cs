using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using NSS.Plugin.Misc.SwiftCore.Helpers;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial record RegisterModel : BaseNopModel
    {
        
        
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email { get; set; }
        
        public bool EnteringEmailTwice { get; set; }
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Fields.ConfirmEmail")]
        public string ConfirmEmail { get; set; }
        public bool CheckUsernameAvailabilityEnabled { get; set; }

        //form fields & properties

        public bool FirstNameEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.FirstName")]
        public string FirstName { get; set; }
        public bool FirstNameRequired { get; set; }
        public bool LastNameEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.LastName")]
        public string LastName { get; set; }
        public bool LastNameRequired { get; set; }

        public HearAboutUs HearAboutUs { get; set; }
        
        public int PreferredPickupLocationId { get; set; }
        
        public string Other { get; set; }

        public string ItemsForNextProject { get; set; }

        public bool APRole { get; set; }

        public bool OperationsRole { get; set; }

        public bool BuyerRole { get; set; }

        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.Company")]
        public string Company { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Account.Fields.Phone")]
        public string CellPhone { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Account.Fields.Phone")]
        public string Phone { get; set; }

        public bool IsExistingCustomer { get; set; }

        public bool AcceptPrivacyPolicyEnabled { get; set; }
        public bool AcceptPrivacyPolicyPopup { get; set; }

        public string MarketingVideoUrl { get; set; }

 
    }
}