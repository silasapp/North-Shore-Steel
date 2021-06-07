using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NSS.Plugin.Misc.SwiftApi.DTO.SpecificationAttributes
{
    public class ProductSpecificationAttributesRootObjectDto : ISerializableObject
    {
        public ProductSpecificationAttributesRootObjectDto()
        {
            ProductSpecificationAttributes = new List<ProductSpecificationAttributeDto>();
        }

        [JsonProperty("product_specification_attributes")]
        public IList<ProductSpecificationAttributeDto> ProductSpecificationAttributes { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "product_specification_attributes";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof (ProductSpecificationAttributeDto);
        }
    }
}