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

        void UpdateUser(UserRegistration userRegistration);
        UserRegistration GetUserById(int id);

        /// <summary>
        /// Register customer
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request);
        //Customer GetCustomerByEmail(string email);
    }
}
