using Nop.Data;
using Nop.Services.Events;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IEventPublisher _eventPublisher;

        public CompanyService(
            IRepository<Company> companyRepository, IEventPublisher eventPublisher)
        {
            _companyRepository = companyRepository;
            _eventPublisher = eventPublisher;
        }

        public void DeleteCompany(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));
            
            _companyRepository.Delete(company);

            //event notification
            _eventPublisher.EntityDeleted(company);
        }

        public void UpdateCompany(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            //update
            _companyRepository.Update(company);

            //event notification
            _eventPublisher.EntityUpdated(company);
        }

        public Company GetCompanyEntityByErpEntityId(int id)
        {
            var company = _companyRepository.Table.FirstOrDefault(c => c.ErpCompanyId == id);

            return company;
        }

        public IList<Company> GetCompanyList()
        {
            var query = _companyRepository.Table;

            return query.ToList();
        }

        public void InsertCompany(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            _companyRepository.Insert(company);

            //event notification
            _eventPublisher.EntityInserted(company);
        }
    }
}
