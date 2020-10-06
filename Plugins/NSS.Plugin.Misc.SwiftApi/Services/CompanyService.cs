using Nop.Data;
using NSS.Plugin.Misc.SwiftApi.Domain.Customers;
using System;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftApi.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _companyRepository;

        public CompanyService(
            IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public Company GetCompanyEntityByErpEntityId(int id)
        {
            var company = _companyRepository.Table.FirstOrDefault(c => c.ErpCompanyId == id);

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
