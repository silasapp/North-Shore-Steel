﻿using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public interface IInvoiceModelFactory
    {
        CompanyInvoiceListModel PrepareOrderListModel(int companyId, CompanyInvoiceListModel.SearchFilter searchFilter);
    }
}
