using Nop.Data;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class CustomerCompanyService : ICustomerCompanyService
    {
        private readonly IRepository<CustomerCompany> _customerCompanyRepository;
        private readonly IRepository<Company> _companyRepository;

        public CustomerCompanyService(
            IRepository<CustomerCompany> customerCompanyRepository,
            IRepository<Company> companyRepository)
        {
            _customerCompanyRepository = customerCompanyRepository;
            _companyRepository = companyRepository;
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

        public virtual CustomerCompany GetCustomerCompanyByErpCompId(int customerId, int erpCompanyId)
        {
            if (customerId == 0 || erpCompanyId == 0)
                return null;

            var companny = _companyRepository.Table.FirstOrDefault(c => c.ErpCompanyId == erpCompanyId);

            if (companny == null)
                return null;

            return _customerCompanyRepository.Table.FirstOrDefault(c => c.CustomerId == customerId && c.CompanyId == companny.Id);
        }

        public virtual IEnumerable<CustomerCompany> GetCustomerCompanies(int customerId)
        {
            if (customerId == 0)
                return null;

            var customerCompanies = _customerCompanyRepository.Table.Where(c => c.CustomerId == customerId).ToList();

            foreach (CustomerCompany customerCompany in customerCompanies)
            {
                customerCompany.Company = _companyRepository.Table.FirstOrDefault(c => c.Id == customerCompany.CompanyId);
            }
            return customerCompanies;
        }

        public void UpdateCustomerCompany(CustomerCompany customerCompany)
        {
            var cCompany = _customerCompanyRepository.Table.FirstOrDefault(c => c.CustomerId == customerCompany.CustomerId && c.CompanyId == customerCompany.CompanyId);
            cCompany.CanCredit = customerCompany.CanCredit;
            _customerCompanyRepository.Update(cCompany);
        }

    }
}
