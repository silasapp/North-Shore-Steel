using LinqToDB;
using System.Linq;
using LinqToDB.Common;
using Nop.Data;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NUglify.Helpers;
using System;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class CustomerCompanyProductService : ICustomerCompanyProductsService
    {
        private readonly IRepository<CustomerCompanyProduct> _customerCompanyProductRepository;

        public CustomerCompanyProductService(
            IRepository<CustomerCompanyProduct> customerCompanyProductRepository)
        {
            _customerCompanyProductRepository = customerCompanyProductRepository;
        }

        public void InsertCustomerCompanyProducts(List<CustomerCompanyProduct> customerCompanyProducts)
        {
            if (customerCompanyProducts.IsNullOrEmpty())
            {
                return;
            }

            // insert
            foreach (CustomerCompanyProduct customerCompanyProduct in customerCompanyProducts)
            {
                _customerCompanyProductRepository.Insert(customerCompanyProduct);
                
            }
        }

        public CustomerCompanyProduct GetCustomerCompanyProductById(int customerId, int companyId, int productId)
        {
            var company = _customerCompanyProductRepository.Table.FirstOrDefault(c => c.CompanyId == companyId && c.CustomerId == customerId && c.ProductId == productId);

            return company;
        }

        public IList<CustomerCompanyProduct> GetCustomerCompanyProducts()
        {
            throw new NotImplementedException();
        }
    }
}
