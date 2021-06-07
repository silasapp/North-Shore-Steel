
using System;
using System.Collections.Generic;
using Nop.Services.Customers;
using System.Text;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface IUserRegistrationService
    {
        Task<(CustomerRegistrationResult, UserRegistration)> InsertUserAsync(UserRegistration userRegistration);

        Task UpdateUserAsync(UserRegistration userRegistration);
        Task<UserRegistration> GetByIdAsync(int id);
        Task<CustomerCompany> CreateUserAsync(UserRegistration user, string password, int statusId, int companyId, string companyName, string salesContactEmail, string salesContactName, string salesContactPhone, bool AP, bool Buyer, bool Operations, int wintrixId);
        Task<Nop.Core.Domain.Customers.Customer> CreateCustomerAsync(UserRegistration user, string password, int wintrixId);
        
        Task UpdateRegisteredUserAsync(int regId, int statusId);
    }
}
