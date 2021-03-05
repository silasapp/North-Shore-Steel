using NSS.Plugin.Misc.SwiftCore.DTOs;
using NSS.Plugin.Misc.SwiftCore.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface IApiService
    {
        #region User API

        public ERPCreateUserResponse CreateNSSUser(ERPCreateUserRequest request);
        public void UpdateNSSUser(int erpId, ERPUpdateUserRequest request);

        #endregion User API

        #region Orders API

        public ERPCreateOrderResponse CreateNSSOrder(int companyId, ERPCreateOrderRequest request);
        public List<ERPSearchOrdersResponse> SearchOpenOrders(int companyId, ERPSearchOrdersRequest request);

        public (string, List<ERPSearchOrdersResponse>) SearchClosedOrders(int companyId, ERPSearchOrdersRequest request);

        public (string, DTOs.Responses.ERPGetOrderDetailsResponse) GetOrderDetails(int companyId, int erpOrderId);

        public (string, List<ERPGetOrderMTRResponse>) GetOrderMTRs(int companyId, int erpOrderId, int? lineItemId = null);

        public (string, ERPGetOrderShippingDetailsResponse) GetOrderShippingDetails(int companyId, int erpOrderId);

        #endregion

        #region Invoices API

        public (string, List<ERPSearchInvoicesResponse>) SearchOpenInvoices(int companyId, ERPSearchInvoicesRequest request);

        public (string, List<ERPSearchInvoicesResponse>) SearchClosedInvoices(int companyId, ERPSearchInvoicesRequest request);

        #endregion

        #region Companies API

        public ERPCompanyInfoResponse GetCompanyInfo(string erpCompanyId);
        public ERPGetCompanyCreditBalance GetCompanyCreditBalance(int companyId);
        public (Dictionary<string, bool>, string) GetCompanyNotificationPreferences(int userId, int companyId);
        public (Dictionary<string, bool>, string) UpdateCompanyNotificationPreferences(int userId, int companyId, IDictionary<string, bool> preferences);
        public List<ERPGetCompanyStats> GetCompanyStats(string companyId);

        #endregion

        #region Shipping API

        public ERPCalculateShippingResponse GetShippingRate(ERPCalculateShippingRequest request);

        #endregion

        #region UserRegistration
        public (ERPRegisterUserResponse, string) CreateUserRegistration(ERPRegisterUserRequest request);
        public string RejectUserRegistration(int regId);
        public (ERPApproveUserRegistrationResponse, string) ApproveUserRegistration(int regId);

        #endregion
    }
}
