using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public interface IInvoiceModelFactory
    {
        Task<CompanyInvoiceListModel> PrepareInvoiceListModelAsync(int companyId, CompanyInvoiceListModel.SearchFilter searchFilter);
    }
}
