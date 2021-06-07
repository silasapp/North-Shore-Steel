using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface ICustomerCompanyProductService
    {
         Task InsertCustomerCompanyProducts(List<CustomerCompanyProduct> customerCompanyProducts);

        IList<CustomerCompanyProduct> GetCustomerCompanyProducts();

         Task UpdateCustomerCompanyProducts(List<CustomerCompanyProduct> customerCompanyProducts);

         Task UpdateCustomerCompanyProduct(CustomerCompanyProduct customerCompanyProduct);

        CustomerCompanyProduct GetCustomerCompanyProductById(int customerCompanyId, int productId);

         Task DeleteCustomerCompanyProduct(CustomerCompanyProduct customerCompanyProduct);

    }
}
