using Nop.Core.Events;
using Nop.Data;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task DeleteCompanyAsync(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));
            
            await _companyRepository.DeleteAsync(company);

            //event notification
            await _eventPublisher.EntityDeletedAsync(company);
        }

        public async Task UpdateCompanyAsync(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            //update
            await _companyRepository.UpdateAsync(company);

            //event notification
            await _eventPublisher.EntityUpdatedAsync(company);
        }

        public async Task<Company> GetCompanyEntityByErpEntityIdAsync(int id)
        {
            var company = await _companyRepository.Table.FirstOrDefaultAsync(c => c.ErpCompanyId == id);

            return company;
        }

        public async Task<IList<Company>> GetCompanyListAsync()
        {
            var query = _companyRepository.Table;

            return await query.ToListAsync();
        }

        public async Task InsertCompanyAsync(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            await _companyRepository.InsertAsync(company);

            //event notification
            await _eventPublisher.EntityInsertedAsync(company);
        }

        public async Task<Company> GetCompanyByIdAsync(int id)
        {
            var company = await _companyRepository.Table.FirstOrDefaultAsync(c => c.Id == id);

            return company;
        }
    }
}
