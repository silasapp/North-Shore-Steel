using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface ICompanyService
    {
        Task<Company> GetCompanyEntityByErpEntityIdAsync(int id);

        Task<Company> GetCompanyByIdAsync(int id);

        Task<IList<Company>> GetCompanyListAsync();

        Task InsertCompanyAsync(Company company);

        Task UpdateCompanyAsync(Company company);

        Task DeleteCompanyAsync(Company company);
    }
}
