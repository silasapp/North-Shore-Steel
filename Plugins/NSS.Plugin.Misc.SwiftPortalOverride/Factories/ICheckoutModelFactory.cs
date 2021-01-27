using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Web.Models.Checkout;


namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public partial interface ICheckoutModelFactory
    {
        /// <summary>
        /// Prepare billing address model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="selectedCountryId">Selected country identifier</param>
        /// <param name="prePopulateNewAddressWithCustomerFields">Pre populate new address with customer fields</param>
        /// <param name="overrideAttributesXml">Override attributes xml</param>
        /// <returns>Billing address model</returns>
        CheckoutBillingAddressModel PrepareBillingAddressModel(IList<ShoppingCartItem> cart,
            int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false,
            string overrideAttributesXml = "");

        /// <summary>
        /// Prepare shipping address model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="selectedCountryId">Selected country identifier</param>
        /// <param name="prePopulateNewAddressWithCustomerFields">Pre populate new address with customer fields</param>
        /// <param name="overrideAttributesXml">Override attributes xml</param>
        /// <param name="cart">Cart</param>
        /// <returns>Shipping address model</returns>
        CheckoutShippingAddressModel PrepareShippingAddressModel(IList<ShoppingCartItem> cart, int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "");

        /// <summary>
        /// Prepare one page checkout model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>One page checkout model</returns>
        OnePageCheckoutModel PrepareOnePageCheckoutModel(IList<ShoppingCartItem> cart);
    }
}
