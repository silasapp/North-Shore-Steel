using NSS.Plugin.Misc.SwiftApi.AutoMapper;
using Nop.Core.Domain.Catalog;
using NSS.Plugin.Misc.SwiftApi.DTO.Products;

namespace NSS.Plugin.Misc.SwiftApi.MappingExtensions
{
    public static class ProductDtoMappings
    {
        public static ProductDto ToDto(this Product product)
        {
            return product.MapTo<Product, ProductDto>();
        }

        public static Product ToEntity(this ProductDto productDto)
        {
            return productDto.MapTo<ProductDto, Product>();
        }

        public static ProductAttributeValueDto ToDto(this ProductAttributeValue productAttributeValue)
        {
            return productAttributeValue.MapTo<ProductAttributeValue, ProductAttributeValueDto>();
        }
    }
}