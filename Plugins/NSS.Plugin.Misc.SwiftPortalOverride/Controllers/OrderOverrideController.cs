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
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;

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

        #endregion

        #region Ctor

        public OrderOverrideController(ICustomerService customerService,
            Nop.Web.Factories.IOrderModelFactory orderNopModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IShipmentService shipmentService,
            IWebHelper webHelper,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings,
            IOrderModelFactory orderModelFactory) : base(customerService, orderNopModelFactory, orderProcessingService, orderService, paymentService, pdfService, shipmentService, webHelper, workContext, rewardPointsSettings)
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
        }

        #endregion

        #region Methods

        //My account / Orders
        [HttpsRequirement]
        public override IActionResult CustomerOrders()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = _orderNopModelFactory.PrepareCustomerOrderListModel();
            return View(model);
        }

        //My account / Order details page
        [HttpsRequirement]
        public override IActionResult Details(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var model = _orderNopModelFactory.PrepareOrderDetailsModel(order);
            //return View("~/Plugins/Misc.SwiftPortalOverride/Views/CustomOrder/Details.cshtml", model);
            return View(model);
        }

        [HttpsRequirement]
        public virtual IActionResult OpenOrders()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new CompanyOrderListModel();

            return View(model);
        }

        [HttpsRequirement]
        public virtual IActionResult ClosedOrders()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new CompanyOrderListModel();
            model.FilterContext.IsClosed = true;

            return View(model);
        }


        [IgnoreAntiforgeryToken]
        public PartialViewResult SearchCompanyOrders(CompanyOrderListModel.SearchFilter filter)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);

            int.TryParse(Request.Cookies[compIdCookieKey], out int eRPCompanyId);

            var model = new CompanyOrderListModel();

            if(eRPCompanyId > 0)
                model = _orderModelFactory.PrepareOrderListModel(eRPCompanyId, filter);

            return PartialView("_", model);
        }

        #endregion
    }
}