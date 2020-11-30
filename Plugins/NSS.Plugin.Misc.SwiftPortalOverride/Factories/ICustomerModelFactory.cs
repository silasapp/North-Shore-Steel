using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    /// <summary>
    /// Represents the interface of the customer model factory
    /// </summary>
    public partial interface ICustomerModelFactory
    {

        /// <summary>
        /// Prepare the customer navigation model
        /// </summary>
        /// <param name="selectedTabId">Identifier of the selected tab</param>
        /// <returns>Customer navigation model</returns>
        CustomerNavigationModel PrepareCustomerNavigationModel(int selectedTabId = 0);

    }
}
