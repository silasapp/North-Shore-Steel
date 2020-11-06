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
    public class CustomerCompanyProductService : ICustomerCompanyProductService
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

        public CustomerCompanyProduct GetCustomerCompanyProductById(int customerCompanyId, int productId)
        {
            var company = _customerCompanyProductRepository.Table.FirstOrDefault(c => c.CustomerCompanyId == customerCompanyId && c.ProductId == productId);

            return company;
        }

        public IList<CustomerCompanyProduct> GetCustomerCompanyProducts()
        {
            throw new NotImplementedException();
        }

        public void UpdateCustomerCompanyProducts(List<CustomerCompanyProduct> customerCompanyProducts)
        {
            foreach (CustomerCompanyProduct customerCompanyProduct in customerCompanyProducts)
            {
                var company = _customerCompanyProductRepository.Table.FirstOrDefault(c => c.CustomerCompanyId == customerCompanyProduct.CustomerCompanyId && c.ProductId == customerCompanyProduct.ProductId);
                if (company == null)
                {
                    _customerCompanyProductRepository.Insert(customerCompanyProduct);
                } 
                else {
                    company.CustomerPartNo = customerCompanyProduct.CustomerPartNo;
                    _customerCompanyProductRepository.Update(company);
                }
            }

        }
    }
}
