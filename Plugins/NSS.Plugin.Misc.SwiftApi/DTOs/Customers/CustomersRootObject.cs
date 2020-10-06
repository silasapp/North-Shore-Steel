using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NSS.Plugin.Misc.SwiftApi.DTO.Customers
{
    public class CustomersRootObject : ISerializableObject
    {
        public CustomersRootObject()
        {
            Customers = new List<CustomerDto>();
        }

        [JsonProperty("customers")]
        public IList<CustomerDto> Customers { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "customers";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(CustomerDto);
        }
    }
}
