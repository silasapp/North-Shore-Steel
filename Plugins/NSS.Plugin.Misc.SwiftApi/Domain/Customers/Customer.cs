using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftApi.Domain.Customers
{
    public class Customer : Nop.Core.Domain.Customers.Customer
    {
        /// <summary>
        /// Gets or sets a list of Customer companies
        /// </summary>
        public IList<CustomerCompany> CustomerCompanies { get; set; }
    }
}
