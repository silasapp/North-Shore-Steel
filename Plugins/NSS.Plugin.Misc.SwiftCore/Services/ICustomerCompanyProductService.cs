using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface ICustomerCompanyProductsService
    {
        void InsertCustomerCompanyProducts(List<CustomerCompanyProduct> customerCompanyProducts);

        IList<CustomerCompanyProduct> GetCustomerCompanyProducts();

        CustomerCompanyProduct GetCustomerCompanyProductById(int customerId, int companyId, int productId);

    }
}
