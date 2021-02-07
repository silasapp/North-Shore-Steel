using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Caching.Extensions;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    /// <summary>
    /// Customer service
    /// </summary>
    public partial class CustomCustomerService : CustomerService
    {
        #region Fields

        private readonly CachingSettings _cachingSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<Address> _customerAddressRepository;
        private readonly IRepository<Nop.Core.Domain.Customers.Customer> _customerRepository;
        private readonly IRepository<CustomerAddressMapping> _customerAddressMappingRepository;
        private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
        private readonly IRepository<CustomerPassword> _customerPasswordRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IRepository<Address> _addressRepository;

        #endregion

        #region Ctor
        public CustomCustomerService(
             CachingSettings cachingSettings,
            CustomerSettings customerSettings,
            ICacheKeyService cacheKeyService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IRepository<Address> customerAddressRepository,
            IRepository<Nop.Core.Domain.Customers.Customer> customerRepository,
            IRepository<CustomerAddressMapping> customerAddressMappingRepository,
            IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
            IRepository<CustomerPassword> customerPasswordRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<GenericAttribute> gaRepository,
            IRepository<ShoppingCartItem> shoppingCartRepository,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            ShoppingCartSettings shoppingCartSettings,
            IRepository<Address> addressRepository
            ) : base(cachingSettings, customerSettings, cacheKeyService, eventPublisher, genericAttributeService, customerAddressRepository, customerRepository, customerAddressMappingRepository,
                customerCustomerRoleMappingRepository, customerPasswordRepository, customerRoleRepository, gaRepository, shoppingCartRepository, staticCacheManager, storeContext, shoppingCartSettings)
        {
            _cachingSettings = cachingSettings;
            _customerSettings = customerSettings;
            _cacheKeyService = cacheKeyService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _customerAddressRepository = customerAddressRepository;
            _customerRepository = customerRepository;
            _customerAddressMappingRepository = customerAddressMappingRepository;
            _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
            _customerPasswordRepository = customerPasswordRepository;
            _customerRoleRepository = customerRoleRepository;
            _gaRepository = gaRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _shoppingCartSettings = shoppingCartSettings;
            _addressRepository = addressRepository;
        }

        #endregion

        #region Methods


        #region Customer address mapping

        public override Address GetCustomerAddress(int customerId, int addressId)
        {
            if (customerId == 0 || addressId == 0)
                return null;

            return _addressRepository.ToCachedGetById(addressId, _cachingSettings.ShortTermCacheTime);
        }

        /// <summary>
        /// Gets a customer billing address
        /// </summary>
        /// <param name="customer">Customer identifier</param>
        /// <returns>Result</returns>
        public override Address GetCustomerBillingAddress(Nop.Core.Domain.Customers.Customer customer)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            return GetCustomerAddress(customer.Id, customer.BillingAddressId ?? 0);
        }

        /// <summary>
        /// Gets a customer shipping address
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public override Address GetCustomerShippingAddress(Nop.Core.Domain.Customers.Customer customer)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            return GetCustomerAddress(customer.Id, customer.ShippingAddressId ?? 0);
        }
        #endregion

        #endregion
    }
}