using Nop.Data;
using Nop.Services.Customers;
using Nop.Core;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using Nop.Services.Localization;
using System.Linq;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;
using Nop.Services.Authentication;
using Nop.Services.Events;
using Nop.Services.Common;
using Nop.Services.Messages;
using System.Collections.Generic;

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
        private readonly IWorkContext _workContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkflowMessageService _workflowMessageService;


        public UserRegistrationService(
            ILocalizationService localizationService,
            IRepository<UserRegistration> userRegistrationRepository,
            ICustomerService customerService,
            IEncryptionService encryptionService,
            CustomerSettings customerSettings,
            ICompanyService companyService,
            IWorkContext workContext,
            ICustomerCompanyService customerCompanyService,
            IAuthenticationService authenticationService,
            IEventPublisher eventPublisher,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            IWorkflowMessageService workflowMessageService
        )
        {
            _localizationService = localizationService;
            _userRegistrationRepository = userRegistrationRepository;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _customerSettings = customerSettings;
            _companyService = companyService;
            _customerCompanyService = customerCompanyService;
            _workContext = workContext;
            _authenticationService = authenticationService;
            _eventPublisher = eventPublisher;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _workflowMessageService = workflowMessageService;
        }

        public virtual UserRegistration GetUserById(int id)
        {
            var user = _userRegistrationRepository.Table.FirstOrDefault(u => u.Id == id);
            return user;
        }

        private void InsertCompany(int companyId, string companyName, string salesContactEmail, string salesContactName, string salesContactPhone)
        {
            Company company = _companyService.GetCompanyEntityByErpEntityId(companyId);
            if (company == null)
            {
                company = new Company
                {
                    ErpCompanyId = companyId,
                    Name = companyName,
                    SalesContactEmail = salesContactEmail,
                    SalesContactName = salesContactName,
                    SalesContactPhone = salesContactPhone
                };

                _companyService.InsertCompany(company);
            }
        }

        public CustomerCompany InsertCustomerCompany(Nop.Core.Domain.Customers.Customer customer, int companyId, bool AP, bool Buyer, bool Operations)
        {
            Company company = _companyService.GetCompanyEntityByErpEntityId(companyId);

            CustomerCompany customerCompany = new CustomerCompany
            {
                CustomerId = customer.Id,
                CompanyId = company.Id,
                CanCredit = false,
                AP = AP,
                Buyer = Buyer,
                Operations = Operations
            };

            _customerCompanyService.InsertCustomerCompany(customerCompany);
            return customerCompany;
        }


        public CustomerCompany CreateUser(UserRegistration userReg, string password, int statusId, int companyId, string companyName, string salesContactEmail, string salesContactName, string salesContactPhone, bool Ap, bool Buyer, bool Operations)
        {
            var customer = new Nop.Core.Domain.Customers.Customer
            {
                RegisteredInStoreId = _storeContext.CurrentStore.Id,
                CustomerGuid = Guid.NewGuid(),
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                Active = true,
                Username = userReg.WorkEmail,
                Email = userReg.WorkEmail
            };

            //insert user
            InsertUser(userReg, customer, password);

            //insert company
            InsertCompany(companyId, companyName, salesContactEmail, salesContactName, salesContactPhone);

            //insert customer company
            var cc = InsertCustomerCompany(customer, companyId, Ap, Buyer, Operations);

            // update user state and modified state 
            UpdateRegisteredUser(userReg.Id, statusId);

            return cc;
        }

        private void InsertUser(UserRegistration user, Nop.Core.Domain.Customers.Customer customer, string password)
        {
            _customerService.InsertCustomer(customer);

            InsertFirstAndLastNameGenericAttributes(user.FirstName, user.LastName, customer);

            //password
            if (!string.IsNullOrWhiteSpace(password))
            {
                AddPassword(password, customer);
            }
        }

        private void InsertFirstAndLastNameGenericAttributes(string firstName, string lastName, Nop.Core.Domain.Customers.Customer newCustomer)
        {
            // we assume that if the first name is not sent then it will be null and in this case we don't want to update it
            if (firstName != null)
            {
                _genericAttributeService.SaveAttribute(newCustomer, NopCustomerDefaults.FirstNameAttribute, firstName);
            }

            if (lastName != null)
            {
                _genericAttributeService.SaveAttribute(newCustomer, NopCustomerDefaults.LastNameAttribute, lastName);
            }
        }

        private void AddPassword(string newPassword, Nop.Core.Domain.Customers.Customer customer)
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
            _customerService.InsertCustomerPassword(customerPassword);

            _customerService.UpdateCustomer(customer);
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

        public void UpdateUser(UserRegistration userRegistration)
        {
            _userRegistrationRepository.Update(userRegistration);
        }

        public void UpdateRegisteredUser(int regId, int statusId)
        {
            var user = GetUserById(regId);
            user.StatusId = statusId;
            user.ModifiedOnUtc = DateTime.UtcNow;
            UpdateUser(user);
        }
    }
}
