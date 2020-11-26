using Nop.Data;
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

        public virtual UserRegistration GetUserById(int id)
        {
            var user = _userRegistrationRepository.Table.FirstOrDefault(u => u.Id == id);
            return user;
        }

        public virtual CustomerRegistrationResult InsertUser(UserRegistration userRegistration)
        {
            if (userRegistration == null)
                throw new ArgumentNullException(nameof(userRegistration));

            var result = new CustomerRegistrationResult();

            if (!CommonHelper.IsValidEmail(userRegistration.WorkEmail))
            {
                result.AddError(_localizationService.GetResource("Common.WrongEmail"));
                return result;
            }


            //at this point request is valid
            _userRegistrationRepository.Insert(userRegistration);

            return result;


        }

        //create customer
        //create nopcustomer
        //var user company=new user company
        //buyer operation and ap
        //getbyerpcompanyId :: i have company here
        //need, company, usercompany, userregistrationbyId

    }
}
