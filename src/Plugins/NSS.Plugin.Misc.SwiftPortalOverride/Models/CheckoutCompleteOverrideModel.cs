using Nop.Web.Factories;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Directory;
using Nop.Web.Models.ShoppingCart;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using NSS.Plugin.Misc.SwiftPortalOverride.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial record CheckoutCompleteOverrideModel : BaseNopEntityModel
    {
        public OnePageCheckoutModel BillingAddressModel { get; set; }
        public CheckoutShippingAddressModel ShippingAddressModel { get; set; }
        public CheckoutShippingMethodModel ShippingMethodModel { get; set; }
        public CheckoutConfirmModel ConfirmModel { get; set; }
        public ShoppingCartModel ShoppingCartModel { get; set; }
        public CheckoutPaymentMethodModel PaymentMethodModel { get; set; }
        public IList<StateProvinceModel> StateProvinces { get; set; }
        public AccountCreditModel AccountCreditModel { get; set; }
        public OrderTotalsModel OrderTotals { get; set; }
        public string PaypalScript { get; set; }
    }

    public class AccountCreditModel
    {
        public bool CanCredit { get; set; }
        public decimal CreditAmount { get; set; }
    }
}
