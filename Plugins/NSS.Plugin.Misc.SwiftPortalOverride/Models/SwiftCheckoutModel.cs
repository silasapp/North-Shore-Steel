using Nop.Web.Models.Checkout;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class SwiftCheckoutModel
    {
        public CheckoutBillingAddressModel BillingAddress { get; set; }
        public CheckoutShippingAddressModel ShippingAddress { get; set; }
        public SwiftCheckoutPaymentMethodModel PaymentMethodModel { get; set; }
    }

    public class SwiftCheckoutPaymentMethodModel
    {
        public int CheckoutPaymentMethodType { get; set; }
    }

    public enum CheckoutPaymentMethodType
    {
        CreditCard = 1, Paypal, LineOfCredit
    }
}
