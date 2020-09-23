using Nop.Core;

namespace Nop.Plugin.Swift.Api.Domain.Customers
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
        /// Gets or sets the Company entity
        /// </summary>
        public Company Company { get; set; }

    }
}
