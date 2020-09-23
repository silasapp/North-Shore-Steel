using Nop.Data;
using Nop.Plugin.Swift.Api.Domain.Customers;
using System;
using System.Linq;

namespace Nop.Plugin.Swift.Api.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _companyRepository;

        public CompanyService(
            IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public Company GetCompanyEntityById(int id)
        {
            var company = _companyRepository.Table.FirstOrDefault(c => c.Id == id);

            return company;
        }

        public void InsertCompany(Company company)
        {

            if (company == null)
                throw new ArgumentNullException(nameof(company));

            _companyRepository.Insert(company);
        }
    }
}
