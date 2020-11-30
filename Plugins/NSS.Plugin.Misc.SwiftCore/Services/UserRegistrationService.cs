using Nop.Data;
using Nop.Services.Customers;
using Nop.Core;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using Nop.Services.Localization;
using System.Linq;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IRepository<UserRegistration> _userRegistrationRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly CustomerSettings _customerSettings;


        public UserRegistrationService(
            ILocalizationService localizationService,
            IRepository<UserRegistration> userRegistrationRepository,
            ICustomerService customerService,
            IEncryptionService encryptionService,
            CustomerSettings customerSettings
        )
        {
            _localizationService = localizationService;
            _userRegistrationRepository = userRegistrationRepository;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _customerSettings = customerSettings;
        }

        public virtual UserRegistration GetUserById(int id)
        {
            var user = _userRegistrationRepository.Table.FirstOrDefault(u => u.Id == id);
            return user;
        }

        public virtual (CustomerRegistrationResult, UserRegistration) InsertUser(UserRegistration userRegistration)
        {
            if (userRegistration == null)
                throw new ArgumentNullException(nameof(userRegistration));

            var result = new CustomerRegistrationResult();

            if (!CommonHelper.IsValidEmail(userRegistration.WorkEmail))
            {
                result.AddError(_localizationService.GetResource("Common.WrongEmail"));
                return (result, null);
            }


            //at this point request is valid
            _userRegistrationRepository.Insert(userRegistration);
            return (result, userRegistration);


        }

        public CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request)
        {

            var result = new CustomerRegistrationResult();

            if (_customerService.IsRegistered(request.Customer))
            {
                result.AddError("Current customer is already registered");
                return result;
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailIsNotProvided"));
                return result;
            }

            if (!CommonHelper.IsValidEmail(request.Email))
            {
                result.AddError(_localizationService.GetResource("Common.WrongEmail"));
                return result;
            }

            //validate unique user
            if (_customerService.GetCustomerByEmail(request.Email) != null)
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists"));
                return result;
            }

            //at this point request is valid
            request.Customer.Username = request.Username;
            request.Customer.Email = request.Email;
            request.Password = "pass$$123word";
            var customerPassword = new CustomerPassword
            {
                CustomerId = request.Customer.Id,
                PasswordFormat = request.PasswordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };
            switch (request.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    customerPassword.Password = request.Password;
                    break;
                case PasswordFormat.Encrypted:
                    customerPassword.Password = _encryptionService.EncryptText(request.Password);
                    break;
                case PasswordFormat.Hashed:
                    var saltKey = _encryptionService.CreateSaltKey(NopCustomerServicesDefaults.PasswordSaltKeySize);
                    customerPassword.PasswordSalt = saltKey;
                    customerPassword.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, _customerSettings.HashedPasswordFormat);
                    break;
            }

            _customerService.InsertCustomerPassword(customerPassword);

            request.Customer.Active = request.IsApproved;

            //add to 'Registered' role
            var registeredRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName);
            if (registeredRole == null)
                throw new NopException("'Registered' role could not be loaded");

            _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = request.Customer.Id, CustomerRoleId = registeredRole.Id });

            //remove from 'Guests' role            
            if (_customerService.IsGuest(request.Customer))
            {
                var guestRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.GuestsRoleName);
                _customerService.RemoveCustomerRoleMapping(request.Customer, guestRole);
            }

            _customerService.UpdateCustomer(request.Customer);

            return result;
        }


        public void UpdateUser(UserRegistration userRegistration)
        {
            _userRegistrationRepository.Update(userRegistration);
        }

        //create customer
        //create nopcustomer
        //var user company=new user company
        //buyer operation and ap
        //getbyerpcompanyId :: i have company here
        //need, company, usercompany, userregistrationbyId

    }
}
