using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NSS.Plugin.Misc.SwiftApi.DTO;
using NSS.Plugin.Misc.SwiftApi.DTO.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.DTOs.Products
{
    public class ErpProductsRootObjectDto : ISerializableObject
    {
        public ErpProductsRootObjectDto()
        {
            Products = new List<ErpProductDto>();
        }

        [JsonProperty("products")]
        public IList<ErpProductDto> Products { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "products";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ErpProductDto);
        }

    }

    [JsonObject(Title = "product")]
    public partial class ErpProductDto : BaseDto, ISerializableObject
    {
        [JsonProperty("itemId")]
        public long itemId { get; set; }

        [JsonProperty("shapeId")]
        public int shapeId { get; set; }

        [JsonProperty("itemNo")]
        public string itemNo { get; set; }

        [JsonProperty("itemTagNo")]
        public string itemTagNo { get; set; }

        [JsonProperty("itemName")]
        public string itemName { get; set; }

        [JsonProperty("leg1")]
        public string leg1 { get; set; }

        [JsonProperty("leg2")]
        public string leg2 { get; set; }

        [JsonProperty("thickness")]
        public double thickness { get; set; }

        [JsonProperty("length")]
        public long length { get; set; }

        [JsonProperty("width")]
        public long width { get; set; }

        [JsonProperty("height")]
        public long height { get; set; }

        [JsonProperty("pipe_size")]
        public long pipe_size { get; set; }

        [JsonProperty("size")]
        public long size { get; set; }

        [JsonProperty("wall")]
        public long wall { get; set; }

        [JsonProperty("od")]
        public long od { get; set; }

        [JsonProperty("dimensions")]
        public string dimensions { get; set; }

        [JsonProperty("heat")]
        public string heat { get; set; }

        [JsonProperty("slab")]
        public string slab { get; set; }

        [JsonProperty("country_of_origin")]
        public string country_of_origin { get; set; }

        [JsonProperty("mill_name")]
        public string mill_name { get; set; }

        [JsonProperty("weight_per_foot")]
        public long weight_per_foot { get; set; }

        [JsonProperty("weight")]
        public long weight { get; set; }

        [JsonProperty("condition")]
        public string condition { get; set; }

        [JsonProperty("grade")]
        public string grade { get; set; }

        [JsonProperty("mtr")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long mtr { get; set; }

        [JsonProperty("pricePerPiece")]
        public double pricePerPiece { get; set; }

        [JsonProperty("pricePerCWT")]
        public double pricePerCWT { get; set; }

        [JsonProperty("pricePerFt")]
        public double pricePerFt { get; set; }

        [JsonProperty("serialized")]
        public bool serialized { get; set; }

        [JsonProperty("quantity")]
        public int quantity { get; set; }

        [JsonProperty("visible")]
        public bool visible { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "product";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ErpProductDto);
        }
    }

    #region JsonConversionMeta
    public partial class ErpProductDto
    {
        public static ErpProductDto FromJson(string json) => JsonConvert.DeserializeObject<ErpProductDto>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ErpProductDto self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
    #endregion
}
