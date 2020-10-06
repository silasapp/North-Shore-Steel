using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.DTO;

namespace NSS.Plugin.Misc.SwiftApi.Tests.SerializersTests.DummyObjects
{
    public class SerializableDummyObjectWithSimpleTypes : ISerializableObject
    {
        public SerializableDummyObjectWithSimpleTypes()
        {
            Items = new List<DummyObjectWithSimpleTypes>();    
        }

        [JsonProperty("primary_property")]
        public IList<DummyObjectWithSimpleTypes> Items { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "primary_property";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof (DummyObjectWithSimpleTypes);
        }
    }
}