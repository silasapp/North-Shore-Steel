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
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public class HomeOverrideController : HomeController
    {
        #region Fields
        private readonly ERPApiProvider _nSSApiProvider;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICompanyService _companyService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ERPApiProvider _erpApiProvider;

        #endregion

        #region Constructor
        public HomeOverrideController(
            ERPApiProvider erpApiProvider,
            ISettingService settingService,
            IStoreContext storeContext,
            ERPApiProvider nSSApiProvider,
            IWorkContext workContext,
            ICustomerCompanyService customerCompanyService,
            IGenericAttributeService genericAttributeService,
            ICompanyService companyService,
            ICustomerModelFactory customerModelFactory

            )
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _nSSApiProvider = nSSApiProvider;
            _workContext = workContext;
            _customerCompanyService = customerCompanyService;
            _genericAttributeService = genericAttributeService;
            _companyService = companyService;
            _customerModelFactory = customerModelFactory;
            _erpApiProvider = erpApiProvider;
        }
        #endregion


        public override IActionResult Index()
        {
            string ERPCId = string.Empty;
            var currentCustomer = _workContext.CurrentCustomer;
            int customerId = currentCustomer.Id;
            Company company = new Company();

            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, customerId);
            TransactionModel model = new TransactionModel();

            // get all companies assigned to customer
            IEnumerable<CustomerCompany> customerCompanies = _customerCompanyService.GetCustomerCompanies(customerId);

            ERPCId = _genericAttributeService.GetAttribute<string>(currentCustomer, compIdCookieKey);
            if (ERPCId != null)
            {
                // check if customer still has access to previously selected company
                IEnumerable<CustomerCompany> cc = customerCompanies.Where(x => x.Company.ErpCompanyId.ToString() == ERPCId);
                if (cc.Count() > 0)
                {
                    model = _customerModelFactory.PrepareCustomerHomeModel(ERPCId);
                    return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
                }

                // remove cookie
                saveAttributeERPCompanyId(currentCustomer, compIdCookieKey, "");


            }

            if (customerCompanies.Count() == 1)
            {
                ERPCId = customerCompanies.First().Company.ErpCompanyId.ToString();
                saveAttributeERPCompanyId(currentCustomer, compIdCookieKey, ERPCId);
                model = _customerModelFactory.PrepareCustomerHomeModel(ERPCId);
                return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);
            }
            else if (customerCompanies.Count() > 1)
            {
                CustomerSelectAccountModel selectAccountModel = new CustomerSelectAccountModel();
                selectAccountModel.Companies = customerCompanies.Select(cc => cc.Company);
                selectAccountModel.loggedInCustomerId = customerId;
                return View("~/Plugins/Misc.SwiftPortalOverride/Views/SelectAccount.cshtml", selectAccountModel);
            }

            // has access to no company
            return View("~/Plugins/Misc.SwiftPortalOverride/Views/HomeIndex.cshtml", model);

        }

        public void SelectCompany(string ERPCompanyId)
        {
            var currentCustomer = _workContext.CurrentCustomer;
            int customerId = currentCustomer.Id;
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, customerId);
            saveAttributeERPCompanyId(currentCustomer, compIdCookieKey, ERPCompanyId);
        }

        public OrderDetailsModel GetOrderMTRs(int companyId, int orderId)
        {
            var model = new OrderDetailsModel();

            var token = string.Empty;
            var orderMTRs = new List<ERPGetOrderMTRResponse>();
            (token, orderMTRs) = _erpApiProvider.GetOrderMTRs(companyId, orderId);

            var orderedMTRs = orderMTRs?.OrderBy(x => x.LineNo)?.ToList();

            foreach (var mtr in orderedMTRs)
            {
                var orderMTR = new OrderDetailsModel.OrderMTRModel
                {
                    MtrId = mtr.MtrId,
                    LineNo = mtr.LineNo,
                    HeatNo = mtr.HeatNo,
                    Description = mtr.Description,
                    MtrFile = $"{mtr.MtrFile}{token}"
                };

                model.MTRs.Add(orderMTR);
            }
            return model;
        }

        private void saveAttributeERPCompanyId(Nop.Core.Domain.Customers.Customer currentCustomer, string compIdCookieKey, string ERPCompanyId)
        {
            _genericAttributeService.SaveAttribute(currentCustomer, compIdCookieKey, ERPCompanyId);

        }
    }
}
