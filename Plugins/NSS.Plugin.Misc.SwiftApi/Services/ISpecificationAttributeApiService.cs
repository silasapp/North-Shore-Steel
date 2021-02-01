using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using NSS.Plugin.Misc.SwiftApi.Infrastructure;

namespace NSS.Plugin.Misc.SwiftApi.Services
{
    public interface ISpecificationAttributeApiService
    {
        IList<ProductSpecificationAttribute> GetProductSpecificationAttributes(
            int? productId = null, int? specificationAttributeOptionId = null, bool? allowFiltering = null, bool? showOnProductPage = null,
            int limit = Constants.Configurations.DefaultLimit, int page = Constants.Configurations.DefaultPageValue,
            int sinceId = Constants.Configurations.DefaultSinceId);

        IList<SpecificationAttribute> GetSpecificationAttributes(
            int limit = Constants.Configurations.DefaultLimit, int page = Constants.Configurations.DefaultPageValue,
            int sinceId = Constants.Configurations.DefaultSinceId);
    }
}
