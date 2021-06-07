using LinqToDB;
using System.Linq;
using LinqToDB.Common;
using Nop.Data;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class CustomerCompanyProductService : ICustomerCompanyProductService
    {
        private readonly IRepository<CustomerCompanyProduct> _customerCompanyProductRepository;

        public CustomerCompanyProductService(
            IRepository<CustomerCompanyProduct> customerCompanyProductRepository)
        {
            _customerCompanyProductRepository = customerCompanyProductRepository;
        }

        public async Task InsertCustomerCompanyProducts(List<CustomerCompanyProduct> customerCompanyProducts)
        {
            if (customerCompanyProducts.IsNullOrEmpty())
            {
                return;
            }

            // insert
            foreach (CustomerCompanyProduct customerCompanyProduct in customerCompanyProducts)
            {
             await   _customerCompanyProductRepository.InsertAsync(customerCompanyProduct);
                
            }
        }

        public CustomerCompanyProduct GetCustomerCompanyProductById(int customerCompanyId, int productId)
        {
            var company = _customerCompanyProductRepository.Table.FirstOrDefault(c => c.CustomerCompanyId == customerCompanyId && c.ProductId == productId);

            return company;
        }

        public IList<CustomerCompanyProduct> GetCustomerCompanyProducts()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateCustomerCompanyProducts(List<CustomerCompanyProduct> customerCompanyProducts)
        {
            foreach (CustomerCompanyProduct customerCompanyProduct in customerCompanyProducts)
            {
                var company = _customerCompanyProductRepository.Table.FirstOrDefault(c => c.CustomerCompanyId == customerCompanyProduct.CustomerCompanyId && c.ProductId == customerCompanyProduct.ProductId);
                if (company == null)
                {
                  await  _customerCompanyProductRepository.InsertAsync(customerCompanyProduct);
                } 
                else {
                    company.CustomerPartNo = customerCompanyProduct.CustomerPartNo;
                   await _customerCompanyProductRepository.UpdateAsync(company);
                }
            }

        }

        public async Task UpdateCustomerCompanyProduct(CustomerCompanyProduct customerCompanyProduct)
        {
            var company = _customerCompanyProductRepository.Table.FirstOrDefault(c => c.CustomerCompanyId == customerCompanyProduct.CustomerCompanyId && c.ProductId == customerCompanyProduct.ProductId);
            if (company == null)
            {
              await  _customerCompanyProductRepository.InsertAsync(customerCompanyProduct);
            }
            else
            {
                company.CustomerPartNo = customerCompanyProduct.CustomerPartNo;
             await   _customerCompanyProductRepository.UpdateAsync(company);
            }
        }

        public async Task DeleteCustomerCompanyProduct(CustomerCompanyProduct customerCompanyProduct)
        {
            if (customerCompanyProduct == null)
                throw new ArgumentNullException(nameof(customerCompanyProduct));

          await  _customerCompanyProductRepository.DeleteAsync(customerCompanyProduct);
        }
    }
}
