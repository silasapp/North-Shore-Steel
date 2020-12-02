
using System;
using System.Collections.Generic;
using Nop.Services.Customers;
using System.Text;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface IUserRegistrationService
    {
        (CustomerRegistrationResult, UserRegistration) InsertUser(UserRegistration userRegistration);

        void UpdateUser(UserRegistration userRegistration);
        UserRegistration GetById(int id);
        CustomerCompany CreateUser(UserRegistration user, string password, int statusId, int companyId, string companyName, string salesContactEmail, string salesContactName, string salesContactPhone, bool AP, bool Buyer, bool Operations);
        
        void UpdateRegisteredUser(int regId, int statusId);
    }
}
