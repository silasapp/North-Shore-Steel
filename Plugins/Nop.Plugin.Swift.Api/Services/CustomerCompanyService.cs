using Nop.Data;
using Nop.Plugin.Swift.Api.Domain.Customers;
using System;

namespace Nop.Plugin.Swift.Api.Services
{
    public class CustomerCompanyService : ICustomerCompanyService
    {
        private readonly IRepository<CustomerCompany> _customerCompanyRepository;

        public CustomerCompanyService(
            IRepository<CustomerCompany> customerCompanyRepository)
        {
            _customerCompanyRepository = customerCompanyRepository;
        }

        public void InsertCustomerCompany(CustomerCompany customerCompany)
        {
            
            if (customerCompany == null)
                throw new ArgumentNullException(nameof(customerCompany));

            _customerCompanyRepository.Insert(customerCompany);
        }
    }
}
