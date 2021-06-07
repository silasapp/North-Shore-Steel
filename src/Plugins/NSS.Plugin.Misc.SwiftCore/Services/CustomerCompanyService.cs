using Nop.Data;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task DeleteCustomerCompanyAsync(CustomerCompany customerCompany)
        {
            if (customerCompany == null)
                throw new ArgumentNullException(nameof(customerCompany));

            await _customerCompanyRepository.DeleteAsync(customerCompany);
        }

        public async Task InsertCustomerCompanyAsync(CustomerCompany customerCompany)
        {

            if (customerCompany == null)
                throw new ArgumentNullException(nameof(customerCompany));

            await _customerCompanyRepository.InsertAsync(customerCompany);
        }

        public virtual async Task<CustomerCompany> GetCustomerCompanyAsync(int customerId, int companyId)
        {
            if (customerId == 0 || companyId == 0)
                return null;

            return await _customerCompanyRepository.Table.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.CompanyId == companyId);
        }

        public virtual async Task<CustomerCompany> GetCustomerCompanyByErpCompIdAsync(int customerId, int erpCompanyId)
        {
            if (customerId == 0 || erpCompanyId == 0)
                return null;

            CustomerCompany customerCompany = null;

            var companny = await _companyRepository.Table.FirstOrDefaultAsync(c => c.ErpCompanyId == erpCompanyId);

            if (companny == null)
                return null;

            customerCompany = await _customerCompanyRepository.Table.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.CompanyId == companny.Id);
            if (customerCompany != null)
                customerCompany.Company = companny;

            return customerCompany;
        }

        public virtual async Task<IEnumerable<CustomerCompany>> GetCustomerCompaniesAsync(int customerId)
        {
            if (customerId == 0)
                return null;

            var customerCompanies = await _customerCompanyRepository.Table.Where(c => c.CustomerId == customerId).ToListAsync();

            foreach (CustomerCompany customerCompany in customerCompanies)
            {
                customerCompany.Company = await _companyRepository.Table.FirstOrDefaultAsync(c => c.Id == customerCompany.CompanyId);
            }
            return customerCompanies;
        }

        public async Task UpdateCustomerCompanyAsync(CustomerCompany customerCompany)
        {
            var cCompany = await _customerCompanyRepository.Table.FirstOrDefaultAsync(c => c.CustomerId == customerCompany.CustomerId && c.CompanyId == customerCompany.CompanyId);
            cCompany.CanCredit = customerCompany.CanCredit;
            await _customerCompanyRepository.UpdateAsync(cCompany);
        }

        public async Task<bool> AuthorizeAsync(int customerId, int erpCompanyId, ERPRole role)
        {
            if (customerId == 0 || erpCompanyId == 0)
                return false;

            var company = await _companyRepository.Table.FirstOrDefaultAsync(c => c.ErpCompanyId == erpCompanyId);
            if (company == null)
                return false;

            var customerCompany = await _customerCompanyRepository.Table.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.CompanyId == company.Id);
            if (customerCompany == null)
                return false;

            switch (role)
            {
                case ERPRole.AP:
                    return customerCompany.AP;
                case ERPRole.Buyer:
                    return customerCompany.Buyer;
                case ERPRole.Operations:
                    return customerCompany.Operations;
                case ERPRole.CanCredit:
                    return customerCompany.CanCredit;
                default:
                    break;
            }

            return false;

        }
    }
}
