using System.Collections.Generic;
using System.Threading.Tasks;
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
       Task<CustomerNavigationModel> PrepareCustomerNavigationModelAsync(bool isABuyer, bool isOperations, int selectedTabId = 0);

       Task<TransactionModel> PrepareCustomerHomeModelAsync(string CompanyId);
        /// <summary>
        /// Prepare the customer register model
        /// </summary>
        /// <param name="model">Customer register model</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomCustomerAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
        /// <param name="setDefaultValues">Whether to populate model properties by default values</param>
        /// <returns>Customer register model</returns>
        Task<RegisterModel> PrepareRegisterModelAsync(RegisterModel model, bool excludeProperties,
            string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false);

        Task<NotificationsModel> PrepareNotificationsModelAsync(int eRPCompanyId, string error, IDictionary<string, bool> notifications);

       Task<Nop.Web.Models.Customer.CustomerAddressListModel> PrepareCustomerAddressListModelAsync(int erpComapanyId);
    }
}
