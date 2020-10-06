using NSS.Plugin.Misc.SwiftApi.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Services
{
    public interface ICompanyService
    {
        Company GetCompanyEntityByErpEntityId(int id);

        void InsertCompany(Company company);
    }
}
