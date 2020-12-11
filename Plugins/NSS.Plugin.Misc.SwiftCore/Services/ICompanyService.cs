using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface ICompanyService
    {
        Company GetCompanyEntityByErpEntityId(int id);

        Company GetCompanyById(int id);

        IList<Company> GetCompanyList();

        void InsertCompany(Company company);

        void UpdateCompany(Company company);

        void DeleteCompany(Company company);
    }
}
