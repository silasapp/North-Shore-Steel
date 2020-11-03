using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class SwiftCheckoutModel
    {
        public SwiftCheckoutBillingAddress BillingAddress { get; set; }
        public SwiftCheckoutShippingAddress ShippingAddress { get; set; }
        public SwiftCheckoutPaymentMethodModel PaymentMethodModel { get; set; }
    }

    public class SwiftCheckoutBillingAddress
    {
        public AddressModel BillingNewAddress { get; set; }
        public int? BillingAddressId { get; set; }
        public bool SaveToAddressBook { get; set; }
        public bool ShipToSameAddress { get; set; }
    }

    public class SwiftCheckoutShippingAddress
    {
        public AddressModel ShippingNewAddress { get; set; }
        public int? ShippingAddressId { get; set; }
        public bool SaveToAddressBook { get; set; }
        public CheckoutPickupPointModel PickupPoint { get; set; }
        public bool IsPickupInStore { get; set; }
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
