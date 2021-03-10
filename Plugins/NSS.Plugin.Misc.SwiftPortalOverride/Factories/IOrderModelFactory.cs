using NSS.Plugin.Misc.SwiftCore.DTOs;
using NSS.Plugin.Misc.SwiftCore.DTOs.Responses;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public interface IOrderModelFactory
    {
        CompanyOrderListModel PrepareOrderListModel(int companyId, CompanyOrderListModel.SearchFilter filter);
        OrderDetailsModel PrepareOrderDetailsModel(int companyId, int erpOrderId, ERPGetOrderDetailsResponse orderDetailsResponse, int mtrCount, List<ERPGetOrderMTRResponse> orderMTRs);
        OrderShippingDetailsModel PrepareOrderShippingDetailsModel(ERPGetOrderShippingDetailsResponse orderShippingDetailsResponse);
    }
}
