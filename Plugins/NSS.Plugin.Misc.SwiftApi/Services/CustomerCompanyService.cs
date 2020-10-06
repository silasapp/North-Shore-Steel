using Nop.Data;
using NSS.Plugin.Misc.SwiftApi.Domain.Customers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftApi.Services
{
    public class CustomerCompanyService : ICustomerCompanyService
    {
        private readonly IRepository<CustomerCompany> _customerCompanyRepository;

        public CustomerCompanyService(
            IRepository<CustomerCompany> customerCompanyRepository)
        {
            _customerCompanyRepository = customerCompanyRepository;
        }

        public void DeleteCustomerCompany(CustomerCompany customerCompany)
        {
            if (customerCompany == null)
                throw new ArgumentNullException(nameof(customerCompany));

            _customerCompanyRepository.Delete(customerCompany);
        }

        public void InsertCustomerCompany(CustomerCompany customerCompany)
        {
            
            if (customerCompany == null)
                throw new ArgumentNullException(nameof(customerCompany));

            _customerCompanyRepository.Insert(customerCompany);
        }

        public virtual CustomerCompany GetCustomerCompany(int customerId, int companyId)
        {
            if (customerId == 0 || companyId == 0)
                return null;

            return _customerCompanyRepository.Table.FirstOrDefault(c => c.CustomerId == customerId && c.CompanyId == companyId);
        }

        public virtual IEnumerable<CustomerCompany> GetCustomerCompanies(int customerId)
        {
            if (customerId == 0)
                return null;

            return _customerCompanyRepository.Table.Where(c => c.CustomerId == customerId).ToList();
        }
    }
}
