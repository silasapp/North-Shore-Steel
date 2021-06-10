using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Configuration;
using Nop.Web.Controllers;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Linq;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System.Collections.Generic;
using Nop.Services.Common;
using Nop.Core.Domain.Customers;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using NSS.Plugin.Misc.SwiftCore.DTOs;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class HomeOverrideController : HomeController
    {
        #region Fields
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICompanyService _companyService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IApiService _apiService;

        #endregion

        #region Constructor
        public HomeOverrideController(
            IApiService apiService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ICustomerCompanyService customerCompanyService,
            IGenericAttributeService genericAttributeService,
            ICompanyService companyService,
            ICustomerModelFactory customerModelFactory

            )
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _customerCompanyService = customerCompanyService;
            _genericAttributeService = genericAttributeService;
            _companyService = companyService;
            _customerModelFactory = customerModelFactory;
            _apiService = apiService;
        }
        #endregion


        public override IActionResult Index()
        {
            string ERPCId = string.Empty;
            var currentCustomer =  _workContext.GetCurrentCustomerAsync().Result;
            int customerId = currentCustomer.Id;
            Company company = new Company();

            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, customerId);
            TransactionModel model = new TransactionModel();

            // get all companies assigned to customer
            IEnumerable<CustomerCompany> customerCompanies = _customerCompanyService.GetCustomerCompaniesAsync(customerId).Result;

            ERPCId =  _genericAttributeService.GetAttributeAsync<string>(currentCustomer, compIdCookieKey).Result;

            if (ERPCId != null)
            {
                // check if customer still has access to previously selected company
                IEnumerable<CustomerCompany> cc = customerCompanies.Where(x => x.Company.ErpCompanyId.ToString() == ERPCId);
                if (cc.Count() > 0)
                {
                 var (result, updatedModel) = NavigateToPermittedHomeScreen(ERPCId, model).Result;
                    model = updatedModel;
                    return result;
                }

                // remove cookie
                SaveAttributeERPCompanyId(currentCustomer, compIdCookieKey, "");
            }

            if (customerCompanies.Count() == 1)
            {
                ERPCId = customerCompanies.First().Company.ErpCompanyId.ToString();
                SaveAttributeERPCompanyId(currentCustomer, compIdCookieKey, ERPCId);

                var (result, updatedModel) = NavigateToPermittedHomeScreen(ERPCId, model).Result;
                model = updatedModel;
                return result;
            }
            else if (customerCompanies.Count() > 1)
            {
                CustomerSelectAccountModel selectAccountModel = new CustomerSelectAccountModel
                {
                    Companies = customerCompanies.Select(cc => cc.Company),
                    LoggedInCustomerId = customerId
                };

                return View("~/Plugins/Misc.SwiftPortalOverride/Views/SelectAccount.cshtml", selectAccountModel);
            }

            // has no access to company
            return RedirectToAction("Index", "Resource");
        }

        public async Task SelectCompany(string ERPCompanyId)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            int customerId = currentCustomer.Id;
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, customerId);
           await SaveAttributeERPCompanyId(currentCustomer, compIdCookieKey, ERPCompanyId);
        }

        private async Task<(IActionResult, TransactionModel)> NavigateToPermittedHomeScreen(string eRPCId, TransactionModel model)
        {
            model = new TransactionModel();

            var (canViewDashboard, isAPUser) = await CanViewDashboard(eRPCId);

            if (canViewDashboard)
            {
                model = await _customerModelFactory.PrepareCustomerHomeModelAsync(eRPCId);
                return (View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model), model);
            }

            if (isAPUser)
            {
                return (RedirectToAction("CompanyInvoices", "Invoice", model), model);
            }

            // no permission
            return (RedirectToAction("Index", "Resource", model), model);
        }

        private async Task<(bool, bool)> CanViewDashboard(string ERPCId)
        {
            bool isBuyer = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, Convert.ToInt32(ERPCId), ERPRole.Buyer);
            bool isOperations = await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, Convert.ToInt32(ERPCId), ERPRole.Operations);
            bool isAPUser =await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, Convert.ToInt32(ERPCId), ERPRole.AP);

            bool canViewDashboard = (isBuyer || isOperations);

            return (canViewDashboard, isAPUser);
        }

        private async Task SaveAttributeERPCompanyId(Nop.Core.Domain.Customers.Customer currentCustomer, string compIdCookieKey, string ERPCompanyId)
        {
           await _genericAttributeService.SaveAttributeAsync(currentCustomer, compIdCookieKey, ERPCompanyId);
        }
    }
}
