using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface ICompanyService
    {
        Company GetCompanyEntityByErpEntityId(int id);

        void InsertCompany(Company company);
    }
}
