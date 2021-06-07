using Nop.Data;
using Nop.Services.Customers;
using Nop.Core;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using Nop.Services.Localization;
using System.Linq;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;
using Nop.Services.Common;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IRepository<UserRegistration> _userRegistrationRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICompanyService _companyService;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;


        public UserRegistrationService(
            ILocalizationService localizationService,
            IRepository<UserRegistration> userRegistrationRepository,
            ICustomerService customerService,
            IEncryptionService encryptionService,
            CustomerSettings customerSettings,
            ICompanyService companyService,
            ICustomerCompanyService customerCompanyService,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService
        )
        {
            _localizationService = localizationService;
            _userRegistrationRepository = userRegistrationRepository;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _customerSettings = customerSettings;
            _companyService = companyService;
            _customerCompanyService = customerCompanyService;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
        }

        public async Task<UserRegistration> GetByIdAsync(int id)
        {
            return await _userRegistrationRepository.Table.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async  Task<CustomerCompany> InsertCustomerCompanyAsync(Nop.Core.Domain.Customers.Customer customer, int companyId, bool aP, bool buyer, bool operations)
        {
            var company = await _companyService.GetCompanyEntityByErpEntityIdAsync(companyId);

            var customerCompany = new CustomerCompany
            {
                CustomerId = customer.Id,
                CompanyId = company.Id,
                CanCredit = false,
                AP = aP,
                Buyer = buyer,
                Operations = operations
            };

         await   _customerCompanyService.InsertCustomerCompanyAsync(customerCompany);
            return customerCompany;
        }


        public async Task<CustomerCompany> CreateUserAsync(UserRegistration userReg, string password, int statusId, int companyId, string companyName, string salesContactEmail, string salesContactName, string salesContactPhone, bool ap, bool buyer, bool operations, int wintrixId)
        {
            var customer = new Nop.Core.Domain.Customers.Customer
            {
                RegisteredInStoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                CustomerGuid = Guid.NewGuid(),
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                Active = true,
                Username = userReg.WorkEmail,
                Email = userReg.WorkEmail
            };

            //insert user
            await InsertCustomer(userReg, customer, password, wintrixId);

            //insert company
            await InsertCompanyAsync(companyId, companyName, salesContactEmail, salesContactName, salesContactPhone);

            //insert customer company
            var cc = await InsertCustomerCompanyAsync(customer, companyId, ap, buyer, operations);

            // update user state and modified state 
            await UpdateRegisteredUserAsync(userReg.Id, statusId);

            return cc;
        }

        public async Task<Nop.Core.Domain.Customers.Customer> CreateCustomerAsync(UserRegistration userReg, string password, int wintrixId)
        {
            var customer = new Nop.Core.Domain.Customers.Customer
            {
                RegisteredInStoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                CustomerGuid = Guid.NewGuid(),
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                Active = true,
                Username = userReg.WorkEmail,
                Email = userReg.WorkEmail
            };

            //insert user
            await InsertCustomer(userReg, customer, password, wintrixId);

            return customer;
        }

        private async Task InsertCustomer(UserRegistration reg, Nop.Core.Domain.Customers.Customer customer, string password, int wintrixId)
        {
            await _customerService.InsertCustomerAsync(customer);

            await InsertCustomGenericAttributesAsync(reg, customer, wintrixId);

            //password
            if (!string.IsNullOrWhiteSpace(password))
            {
                await AddPasswordAsync(password, customer);
            }

            //add to 'Registered' role
            var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
            if (registeredRole == null)
                throw new NopException("'Registered' role could not be loaded");

            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = registeredRole.Id });

            //remove from 'Guests' role            
            if (await _customerService.IsGuestAsync(customer))
            {
                var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
                await _customerService.RemoveCustomerRoleMappingAsync(customer, guestRole);
            }

            await _customerService.UpdateCustomerAsync(customer);
        }

        private async Task InsertCompanyAsync(int companyId, string companyName, string salesContactEmail, string salesContactName, string salesContactPhone)
        {
            Company company = await _companyService.GetCompanyEntityByErpEntityIdAsync(companyId);
            if (company == null)
            {
                company = new Company
                {
                    ErpCompanyId = companyId,
                    Name = companyName,
                    SalesContactEmail = salesContactEmail,
                    SalesContactName = salesContactName,
                    SalesContactPhone = salesContactPhone,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _companyService.InsertCompanyAsync(company);
            }
        }

        private async Task InsertCustomGenericAttributesAsync(UserRegistration reg, Nop.Core.Domain.Customers.Customer newCustomer, int wintrixId)
        {
            // save custom fields

            await _genericAttributeService.SaveAttributeAsync(newCustomer, NopCustomerDefaults.FirstNameAttribute, reg.FirstName);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, NopCustomerDefaults.LastNameAttribute, reg.LastName);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, NopCustomerDefaults.PhoneAttribute, reg.Phone);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, NopCustomerDefaults.CompanyAttribute, reg.CompanyName);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, Helpers.Constants.CellAttribute, reg.Cell);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, Helpers.Constants.HearAboutUsAttribute, reg.HearAboutUs);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, Helpers.Constants.OtherAttribute, reg.Other);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, Helpers.Constants.PreferredLocationIdAttribute, reg.PreferredLocationId);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, Helpers.Constants.ItemsForNextProjectAttribute, reg.ItemsForNextProject);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, Helpers.Constants.IsExistingCustomerAttribute, reg.IsExistingCustomer);
            await _genericAttributeService.SaveAttributeAsync(newCustomer, Helpers.Constants.ErpKeyAttribute, wintrixId);
            
        }

        private async Task AddPasswordAsync(string newPassword, Nop.Core.Domain.Customers.Customer customer)
        {
            var customerPassword = new CustomerPassword
            {
                CustomerId = customer.Id,
                PasswordFormat = _customerSettings.DefaultPasswordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };

            switch (_customerSettings.DefaultPasswordFormat)
            {
                case PasswordFormat.Clear:
                    {
                        customerPassword.Password = newPassword;
                    }
                    break;
                case PasswordFormat.Encrypted:
                    {
                        customerPassword.Password = _encryptionService.EncryptText(newPassword);
                    }
                    break;
                case PasswordFormat.Hashed:
                    {
                        var saltKey = _encryptionService.CreateSaltKey(5);
                        customerPassword.PasswordSalt = saltKey;
                        customerPassword.Password = _encryptionService.CreatePasswordHash(newPassword, saltKey, _customerSettings.HashedPasswordFormat);
                    }
                    break;
            }
            await _customerService.InsertCustomerPasswordAsync(customerPassword);

            await _customerService.UpdateCustomerAsync(customer);
        }

        public virtual async Task<(CustomerRegistrationResult, UserRegistration)> InsertUserAsync(UserRegistration userRegistration)
        {
            if (userRegistration == null)
                throw new ArgumentNullException(nameof(userRegistration));

            var result = new CustomerRegistrationResult();

            if (!CommonHelper.IsValidEmail(userRegistration.WorkEmail))
            {
                result.AddError(await _localizationService.GetResourceAsync("Common.WrongEmail"));
                return (result, null);
            }


            //at this point request is valid
            await _userRegistrationRepository.InsertAsync(userRegistration);
            return (result, userRegistration);


        }

        public async Task UpdateUserAsync (UserRegistration userRegistration)
        {
            await _userRegistrationRepository.UpdateAsync(userRegistration);
        }

        public async Task UpdateRegisteredUserAsync(int regId, int statusId)
        {
            var user = await GetByIdAsync(regId);
            user.StatusId = statusId;
            user.ModifiedOnUtc = DateTime.UtcNow;
            await UpdateUserAsync(user);
        }
    }
}
