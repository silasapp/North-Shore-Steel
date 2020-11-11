using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface ICustomerCompanyProductService
    {
        void InsertCustomerCompanyProducts(List<CustomerCompanyProduct> customerCompanyProducts);

        IList<CustomerCompanyProduct> GetCustomerCompanyProducts();

        void UpdateCustomerCompanyProducts(List<CustomerCompanyProduct> customerCompanyProducts);

        void UpdateCustomerCompanyProduct(CustomerCompanyProduct customerCompanyProduct);

        CustomerCompanyProduct GetCustomerCompanyProductById(int customerCompanyId, int productId);

        void DeleteCustomerCompanyProduct(CustomerCompanyProduct customerCompanyProduct);

    }
}
