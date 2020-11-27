using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using Nop.Services.Customers;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface IUserRegistrationService
    {
        (CustomerRegistrationResult, UserRegistration) InsertUser(UserRegistration userRegistration);

        UserRegistration GetUserById(int id);
        //Customer GetCustomerByEmail(string email);
    }
}
