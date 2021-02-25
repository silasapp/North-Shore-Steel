using Nop.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftCore.Domain.Customers
{
    public class Company : BaseEntity
    {
        /// <summary>
        /// Gets or sets a name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets ERP company identifier
        /// </summary>
        public int ErpCompanyId { get; set; }

        /// <summary>
        /// Gets or sets a sales contact name
        /// </summary>
        public string SalesContactName { get; set; }

        /// <summary>
        /// Gets or sets a sales contact email
        /// </summary>
        public string SalesContactEmail { get; set; }

        /// <summary>
        /// Gets or sets a sales contact phone
        /// </summary>
        public string SalesContactPhone { get; set; }

        public string SalesContactImageUrl { get; set; }

        //public bool Deleted { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets a list of Customer companies
        /// </summary>
        public IList<CustomerCompany> CustomerCompanies { get; set; }
    }
}
