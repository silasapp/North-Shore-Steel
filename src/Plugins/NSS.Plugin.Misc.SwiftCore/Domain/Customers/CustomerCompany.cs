using Nop.Core;

namespace NSS.Plugin.Misc.SwiftCore.Domain.Customers
{
    public class CustomerCompany : BaseEntity
    {
        /// <summary>
        /// Gets or sets the Customer entity identifier
        /// </summary>
        public int CustomerId { get; set;}

        /// <summary>
        /// Gets or sets the Customer entity
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the Company entity identifier
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the can credit
        /// </summary>
        public bool CanCredit { get; set; }

        /// <summary>
        /// Gets or sets the Company entity
        /// </summary>
        public Company Company { get; set; }

        /// <summary>
        /// Gets or sets the AP entity
        /// </summary>
        public bool AP { get; set; }

        /// <summary>
        /// Gets or sets the Buyer entity
        /// </summary>
        public bool Buyer { get; set; }

        /// <summary>
        /// Gets or sets the Operations entity
        /// </summary>
        public bool Operations { get; set; }

    }
}
