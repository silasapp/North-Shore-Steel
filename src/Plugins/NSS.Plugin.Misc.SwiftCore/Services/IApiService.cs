using NSS.Plugin.Misc.SwiftCore.DTOs;
using NSS.Plugin.Misc.SwiftCore.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface IApiService
    {
        #region User API

        public  Task<ERPCreateUserResponse> CreateNSSUserAsync(ERPCreateUserRequest request);
        public Task UpdateNSSUserAsync(int erpId, ERPUpdateUserRequest request);

        #endregion User API

        #region Orders API

        public  Task<ERPCreateOrderResponse> CreateNSSOrderAsync(int companyId, ERPCreateOrderRequest request);
        public Task<List<ERPSearchOrdersResponse>> SearchOpenOrdersAsync(int companyId, ERPSearchOrdersRequest request);

        public Task<(string, List<ERPSearchOrdersResponse>)> SearchClosedOrdersAsync(int companyId, ERPSearchOrdersRequest request);

        public Task<(string, DTOs.Responses.ERPGetOrderDetailsResponse)> GetOrderDetailsAsync(int companyId, int erpOrderId);

        public Task<(string, List<ERPGetOrderMTRResponse>)> GetOrderMTRsAsync(int companyId, int erpOrderId, int? lineItemId = null);

        public Task<(string, ERPGetOrderShippingDetailsResponse)> GetOrderShippingDetailsAsync(int companyId, int erpOrderId);

        #endregion

        #region Invoices API

        public Task<(string, List<ERPSearchInvoicesResponse>)> SearchOpenInvoicesAsync(int companyId, ERPSearchInvoicesRequest request);

        public Task<(string, List<ERPSearchInvoicesResponse>)> SearchClosedInvoicesAsync(int companyId, ERPSearchInvoicesRequest request);

        #endregion

        #region Companies API

        public Task<ERPCompanyInfoResponse> GetCompanyInfoAsync(string erpCompanyId);
        public Task<ERPGetCompanyCreditBalance> GetCompanyCreditBalanceAsync(int companyId);
        public Task<(Dictionary<string, bool>, string)> GetCompanyNotificationPreferencesAsync(int userId, int companyId);
        public Task<(Dictionary<string, bool>, string)> UpdateCompanyNotificationPreferencesAsync(int userId, int companyId, IDictionary<string, bool> preferences);
        public Task<List<ERPGetCompanyStats>> GetCompanyStatsAsync(string companyId);

        #endregion

        #region Shipping API

        public Task<(string, ERPCalculateShippingResponse)> GetShippingRateAsync(ERPCalculateShippingRequest request);

        #endregion

        #region UserRegistration
        public Task<(ERPRegisterUserResponse, string)> CreateUserRegistrationAsync(ERPRegisterUserRequest request);
        public  Task<string> RejectUserRegistrationAsync(int regId);
        public Task<(ERPApproveUserRegistrationResponse, string)> ApproveUserRegistrationAsync(int regId);

        #endregion
    }
}
