﻿using Nop.Data;
using Nop.Services.Customers;
using Nop.Core;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using Nop.Services.Localization;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IRepository<UserRegistration> _userRegistrationRepository;
        private readonly ILocalizationService _localizationService;


        public UserRegistrationService(
            ILocalizationService localizationService,
            IRepository<UserRegistration> userRegistrationRepository
        )
        {
            _localizationService = localizationService;
            _userRegistrationRepository = userRegistrationRepository;
        }

        

        public virtual CustomerRegistrationResult InsertUser(UserRegistration userRegistration)
        {
            if (userRegistration == null)
                throw new ArgumentNullException(nameof(userRegistration));

            //if (request.Customer == null)
            //    throw new ArgumentException("Can't load current customer");

            var result = new CustomerRegistrationResult();

            if (!CommonHelper.IsValidEmail(userRegistration.WorkEmail))
            {
                result.AddError(_localizationService.GetResource("Common.WrongEmail"));
                return result;
            }


            //at this point request is valid
            //userRegistration.Customer.Email = request.Email;
            _userRegistrationRepository.Insert(userRegistration);

            return result;


        }
    }
}
