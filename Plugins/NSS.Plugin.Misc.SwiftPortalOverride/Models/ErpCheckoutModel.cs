using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class ErpCheckoutModel
    {
        public ErpCheckoutBillingAddress BillingAddress { get; set; }
        public ErpCheckoutShippingAddress ShippingAddress { get; set; }
        public ErpCheckoutPaymentMethodModel PaymentMethodModel { get; set; }
        public bool HasError { get; set; }
    }

    public class ErpCheckoutBillingAddress
    {
        public AddressModel BillingNewAddress { get; set; }
        public int BillingAddressId { get; set; }
        public bool SaveToAddressBook { get; set; }
        public bool ShipToSameAddress { get; set; }
    }

    public class ErpCheckoutShippingAddress
    {
        public AddressModel ShippingNewAddress { get; set; }
        public int ShippingAddressId { get; set; }
        public bool SaveToAddressBook { get; set; }
        public CheckoutPickupPointModel PickupPoint { get; set; }
        public bool IsPickupInStore { get; set; }
    }

    public class ErpCheckoutPaymentMethodModel
    {
        public int CheckoutPaymentMethodType { get; set; }
    }

    public enum CheckoutPaymentMethodType
    {
        CreditCard = 1, Paypal, LineOfCredit
    }
}
