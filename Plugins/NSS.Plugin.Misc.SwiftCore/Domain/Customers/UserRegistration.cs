using LinqToDB.Mapping;
using Nop.Core;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Domain.Customers
{
    [Table("UserRegistration")]
    public class UserRegistration : BaseEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string WorkEmail { get; set; }

        public string Cell { get; set; }

        public string Phone { get; set; }

        public string CompanyName { get; set; }

        public bool IsExistingCustomer { get; set; }

        public string Roles { get; set; }

        public string HearAboutUs { get; set; }

        public string Other { get; set; }

        public string ItemsForNextProject { get; set; }

        public int StatusId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime ModifiedOnUtc { get; set; }


        // not mapped
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string[] RoleArray 
        { 
            get => Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries);  
            set => Roles = string.Join(',', value);  
        }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public UserRegistrationStatus Status 
        { 
            get => (UserRegistrationStatus)StatusId;  
            set => StatusId = (int)value;  
        }
    }
}
