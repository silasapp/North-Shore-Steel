using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
//using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using System;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class OrderOverrideController : OrderController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly Nop.Web.Factories.IOrderModelFactory _orderNopModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IShipmentService _shipmentService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly ERPApiProvider _erpApiProvider;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public OrderOverrideController(ERPApiProvider erpApiProvider, 
            ICustomerService customerService,
            Nop.Web.Factories.IOrderModelFactory orderNopModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IShipmentService shipmentService,
            IWebHelper webHelper,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings,
            IOrderModelFactory orderModelFactory,
            ICustomerCompanyService customerCompanyService,
            IGenericAttributeService genericAttributeService) : base(customerService, orderNopModelFactory, orderProcessingService, orderService, paymentService, pdfService, shipmentService, webHelper, workContext, rewardPointsSettings)
        {
            _customerService = customerService;
            _orderNopModelFactory = orderNopModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _shipmentService = shipmentService;
            _webHelper = webHelper;
            _workContext = workContext;
            _rewardPointsSettings = rewardPointsSettings;
            _orderModelFactory = orderModelFactory;
            _erpApiProvider = erpApiProvider;
            _customerCompanyService = customerCompanyService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        //My account / Order details page
        [HttpsRequirement]
        public override IActionResult Details(int orderId)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));
            bool isAp = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.AP);
            bool isBuyer = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer);
            bool isOperations = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Operations);

            if (!isBuyer && !isOperations)
                return AccessDeniedView();


            ERPGetOrderDetailsResponse orderDetailsResponse = null;

            // call api
            if (eRPCompanyId > 0 && orderId > 0)
                (_, orderDetailsResponse) = _erpApiProvider.GetOrderDetails(eRPCompanyId, orderId);

            if (orderDetailsResponse == null)
                return Challenge();

            // get mtrs if count > 0
            var orderMTRs = new List<ERPGetOrderMTRResponse>();
            if (int.TryParse(orderDetailsResponse.MtrCount, out int mtrCount) && mtrCount > 0)
            {
                (_, orderMTRs) = _erpApiProvider.GetOrderMTRs(eRPCompanyId, orderId);
            }
            

            var model = _orderModelFactory.PrepareOrderDetailsModel(eRPCompanyId, orderId, orderDetailsResponse, mtrCount, orderMTRs);
            model.CanBuy = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer);
            
            return View(model);
        }



        //My account / Orders
        [HttpsRequirement]
        public virtual IActionResult CompanyOrders()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));
            bool isAp = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.AP);
            bool isBuyer = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer);
            bool isOperations = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Operations);

            if (!isBuyer && !isOperations)
                return AccessDeniedView();

            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new CompanyOrderListModel();

            return View(model);
        }

        [IgnoreAntiforgeryToken]
        public PartialViewResult SearchCompanyOrders([FromBody]CompanyOrderListModel.SearchFilter filter)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));
            bool isAp = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.AP);
            bool isBuyer = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer);
            bool isOperations = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Operations);

            if (!isBuyer && !isOperations)
                return (PartialViewResult)AccessDeniedView();

            var model = new CompanyOrderListModel();

            if(eRPCompanyId > 0)
                model = _orderModelFactory.PrepareOrderListModel(eRPCompanyId, filter);
            model.IsClosed = filter.IsClosed;

            return PartialView("_OrderGrid", model);
        }

        #endregion
    }
}