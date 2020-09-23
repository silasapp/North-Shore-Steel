using Nop.Plugin.Swift.Api.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Swift.Api.Services
{
    public interface ICompanyService
    {
        Company GetCompanyEntityById(int id);

        void InsertCompany(Company company);
    }
}
