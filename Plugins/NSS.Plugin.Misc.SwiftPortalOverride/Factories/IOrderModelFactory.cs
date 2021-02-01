using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public interface IOrderModelFactory
    {
        CompanyOrderListModel PrepareOrderListModel(int companyId, CompanyOrderListModel.SearchFilter filter);
        OrderDetailsModel PrepareOrderDetailsModel(int companyId, int erpOrderId, DTOs.Responses.ERPGetOrderDetailsResponse orderDetailsResponse, int mtrCount, List<DTOs.Responses.ERPGetOrderMTRResponse> orderMTRs, string token);
    }
}
