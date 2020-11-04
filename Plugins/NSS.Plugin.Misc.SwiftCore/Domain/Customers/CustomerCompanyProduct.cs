using Nop.Core;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftCore.Domain.Customers
{
    public class CustomerCompanyProduct : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer company id
        /// </summary>
        public int CustomerCompanyId { get; set; }

        /// <summary>
        /// Gets or sets the product id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the Customer Part No
        /// </summary>
        public string CustomerPartNo { get; set; }
    }
}
