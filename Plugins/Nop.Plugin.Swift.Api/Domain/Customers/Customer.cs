using System.Collections.Generic;

namespace Nop.Plugin.Swift.Api.Domain.Customers
{
    public class Customer : Core.Domain.Customers.Customer
    {
        /// <summary>
        /// Gets or sets a list of Customer companies
        /// </summary>
        public IList<CustomerCompany> CustomerCompanies { get; set; }
    }
}
