﻿using Nop.Data;
using Nop.Plugin.Swift.Api.Domain.Customers;
using System;
using System.Linq;

namespace Nop.Plugin.Swift.Api.Services
{
    public class CustomerCompanyService : ICustomerCompanyService
    {
        private readonly IRepository<CustomerCompany> _customerCompanyRepository;

        public CustomerCompanyService(
            IRepository<CustomerCompany> customerCompanyRepository)
        {
            _customerCompanyRepository = customerCompanyRepository;
        }

        public void DeleteCustomerCompany(CustomerCompany customerCompany)
        {
            if (customerCompany == null)
                throw new ArgumentNullException(nameof(customerCompany));

            _customerCompanyRepository.Delete(customerCompany);
        }

        public void InsertCustomerCompany(CustomerCompany customerCompany)
        {
            
            if (customerCompany == null)
                throw new ArgumentNullException(nameof(customerCompany));

            _customerCompanyRepository.Insert(customerCompany);
        }

        public virtual CustomerCompany GetCustomerCompany(int customerId, int companyId)
        {
            if (customerId == 0 || companyId == 0)
                return null;

            return _customerCompanyRepository.Table.FirstOrDefault(c => c.CustomerId == customerId && c.CompanyId == companyId);
        }
    }
}