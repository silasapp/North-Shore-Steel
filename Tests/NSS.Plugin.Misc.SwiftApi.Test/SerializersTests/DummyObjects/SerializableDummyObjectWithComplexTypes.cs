using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.DTO;

namespace NSS.Plugin.Misc.SwiftApi.Tests.SerializersTests.DummyObjects
{
    public class SerializableDummyObjectWithComplexTypes : ISerializableObject
    {
        public SerializableDummyObjectWithComplexTypes()
        {
            Items = new List<DummyObjectWithComplexTypes>();
        }

        [JsonProperty("primary_complex_property")]
        public IList<DummyObjectWithComplexTypes> Items { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "primary_complex_property";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(DummyObjectWithComplexTypes);
        }
    }
}