using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Caching;
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

      //  private readonly CachingSettings _cachingSettings;
        private readonly CustomerSettings _customerSettings;
     //   private readonly ICacheKeyService _cacheKeyService;
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
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly INopDataProvider _dataProvider;
        private readonly IRepository<ForumTopic> _forumTopicRepository;
        private readonly IRepository<BlogComment> _blogCommentRepository;
        private readonly IRepository<NewsComment> _newsCommentRepository;
        private readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        private readonly IRepository<PollVotingRecord> _pollVotingRecordRepository;

        #endregion

        #region Ctor
        public CustomCustomerService(
          //   CachingSettings cachingSettings,
            CustomerSettings customerSettings,
          //  ICacheKeyService cacheKeyService,
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
            IRepository<ProductReview> productReviewRepository,
            ShoppingCartSettings shoppingCartSettings,
            IRepository<Address> addressRepository, IRepository<Order> orderRepository,
            IRepository<BlogComment> blogCommentRepository,
            IRepository<ForumPost> forumPostRepository,
            IRepository<ForumTopic> forumTopicRepository,
            INopDataProvider dataProvider,
            IRepository<NewsComment> newsCommentRepository,
             IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
            IRepository<PollVotingRecord> pollVotingRecordRepository
            ) :base( customerSettings,
             genericAttributeService,
             dataProvider,
             customerAddressRepository,
             blogCommentRepository,
             customerRepository,
             customerAddressMappingRepository,
             customerCustomerRoleMappingRepository,
             customerPasswordRepository,
             customerRoleRepository,
             forumPostRepository,
             forumTopicRepository,
             gaRepository,
             newsCommentRepository,
             orderRepository,
             productReviewRepository,
             productReviewHelpfulnessRepository,
            pollVotingRecordRepository,
             shoppingCartRepository,
             staticCacheManager,
             storeContext,
             shoppingCartSettings)
            
            
       {
         //   _cachingSettings = cachingSettings;
            _customerSettings = customerSettings;
           // _cacheKeyService = cacheKeyService;
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

        public override async Task<Address> GetCustomerAddressAsync(int customerId, int addressId)
        {
            if (customerId == 0 || addressId == 0)
                return null;/*;*/

            return await _addressRepository.GetByIdAsync(addressId);
        }

        /// <summary>
        /// Gets a customer billing address
        /// </summary>
        /// <param name="customer">Customer identifier</param>
        /// <returns>Result</returns>
        public override async Task<Address> GetCustomerBillingAddressAsync(Nop.Core.Domain.Customers.Customer customer)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            return await GetCustomerAddressAsync(customer.Id, customer.BillingAddressId ?? 0);
        }

        /// <summary>
        /// Gets a customer shipping address
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public override async Task<Address> GetCustomerShippingAddressAsync(Nop.Core.Domain.Customers.Customer customer)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            return await GetCustomerAddressAsync(customer.Id, customer.ShippingAddressId ?? 0);
        }
        #endregion

        #endregion
    }
}