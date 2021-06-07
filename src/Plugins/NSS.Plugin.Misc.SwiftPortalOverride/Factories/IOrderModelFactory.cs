using NSS.Plugin.Misc.SwiftCore.DTOs;
using NSS.Plugin.Misc.SwiftCore.DTOs.Responses;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public interface IOrderModelFactory
    {
        Task<CompanyOrderListModel> PrepareOrderListModelAsync(int companyId, CompanyOrderListModel.SearchFilter filter);
        Task<OrderDetailsModel> PrepareOrderDetailsModelAsync(int companyId, int erpOrderId, ERPGetOrderDetailsResponse orderDetailsResponse, int mtrCount, List<ERPGetOrderMTRResponse> orderMTRs);
        Task<OrderShippingDetailsModel> PrepareOrderShippingDetailsModelAsync(ERPGetOrderShippingDetailsResponse orderShippingDetailsResponse);
    }
}
