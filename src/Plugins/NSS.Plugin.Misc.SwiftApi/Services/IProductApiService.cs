using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using static NSS.Plugin.Misc.SwiftApi.Infrastructure.Constants;

namespace NSS.Plugin.Misc.SwiftApiApi.Services
{
    public interface IProductApiService
    {
        IList<Product> GetProducts(IList<int> ids = null,
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
           int limit = Configurations.DefaultLimit, int page = Configurations.DefaultPageValue, int sinceId = Configurations.DefaultSinceId, 
           int? categoryId = null, string vendorName = null, bool? publishedStatus = null);

        Task<int> GetProductsCount(DateTime? createdAtMin = null, DateTime? createdAtMax = null, 
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, bool? publishedStatus = null, 
            string vendorName = null, int? categoryId = null);

        Product GetProductById(int productId);

        Product GetProductByIdNoTracking(int productId);
    }
}