using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;

namespace NSS.Plugin.DiscountRules.FortyThousandLBS
{
    public partial class FortyThousandLBSDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public FortyThousandLBSDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
            IDiscountService discountService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IShoppingCartService shoppingCartService,
            IProductService productService)
        {
            _actionContextAccessor = actionContextAccessor;
            _discountService = discountService;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            //check if rule is applied
            var isFirstOrderRuleApplied = _settingService.GetSettingByKey<bool>(string.Format(DiscountRequirementDefaults.SettingsKey, request.DiscountRequirementId));
            if (!isFirstOrderRuleApplied)
                return result;

            //result is valid if customer has no order
            var cartItems = _shoppingCartService.GetShoppingCart(request.Customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart);

            int totalWeight = cartItems.Select(item => 
            {
                var product = _productService.GetProductById(item.ProductId);
                return (int)Math.Round(product?.Weight ?? decimal.Zero * item.Quantity);
            }).Sum();

            result.IsValid = totalWeight >= 40000;

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesTenThousandLBS",
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.CurrentRequestProtocol);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //discount requirements
            var discountRequirements = _discountService.GetAllDiscountRequirements()
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SystemName);
            foreach (var discountRequirement in discountRequirements)
            {
                _discountService.DeleteDiscountRequirement(discountRequirement, false);
            }

            base.Uninstall();
        }

        #endregion
    }
}