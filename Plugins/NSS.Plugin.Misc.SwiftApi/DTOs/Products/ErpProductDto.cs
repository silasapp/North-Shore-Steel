using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NSS.Plugin.Misc.SwiftApi.DTO;
using NSS.Plugin.Misc.SwiftApi.DTO.Base;
using System;
using System.Collections.Generic;
using System.Globalization;

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
        [JsonProperty("itemId", Required = Required.Always)]
        public int itemId { get; set; }

        [JsonProperty("shapeId", Required = Required.Always)]
        public int shapeId { get; set; }

        [JsonProperty("itemNo")]
        public string itemNo { get; set; }

        [JsonProperty("itemTagNo")]
        public string itemTagNo { get; set; }

        [JsonProperty("itemName", Required = Required.Always)]
        public string itemName { get; set; }

        [JsonProperty("displayLeg1")]
        public string displayLeg1 { get; set; }

        [JsonProperty("leg1")]
        public decimal? leg1 { get; set; }

        [JsonProperty("displayLeg2")]
        public string displayLeg2 { get; set; }

        [JsonProperty("leg2")]
        public decimal? leg2 { get; set; }

        [JsonProperty("displayThickness")]
        public string displayThickness { get; set; }

        [JsonProperty("thickness")]
        public decimal? thickness { get; set; }

        [JsonProperty("displayLength")]
        public string displayLength { get; set; }

        [JsonProperty("length")]
        public decimal? length { get; set; }

        [JsonProperty("displayWidth")]
        public string displayWidth { get; set; }

        [JsonProperty("width")]
        public decimal? width { get; set; }

        [JsonProperty("displayHeight")]
        public string displayHeight { get; set; }

        [JsonProperty("height")]
        public decimal? height { get; set; }

        [JsonProperty("displayPipeSize")]
        public string displayPipeSize { get; set; }

        [JsonProperty("pipeSize")]
        public decimal? pipeSize { get; set; }
        [JsonProperty("displaySize")]
        public string displaySize { get; set; }

        [JsonProperty("size")]
        public decimal? size { get; set; }
        [JsonProperty("displayOD")]
        public string displayOD { get; set; }

        [JsonProperty("OD")]
        public decimal? OD { get; set; }
        [JsonProperty("wall")]
        public string wall { get; set; }

        [JsonProperty("dimensions")]
        public string dimensions { get; set; }

        [JsonProperty("heat")]
        public string heat { get; set; }

        [JsonProperty("slab")]
        public string slab { get; set; }

        [JsonProperty("countryOfOrigin")]
        public string countryOfOrigin { get; set; }

        [JsonProperty("millName")]
        public string millName { get; set; }
        [JsonProperty("displayWeightPerFoot")]
        public string displayWeightPerFoot { get; set; }

        [JsonProperty("weightPerFoot")]
        public decimal? weightPerFoot { get; set; }

        [JsonProperty("weight")]
        public decimal? weight { get; set; }

        [JsonProperty("condition")]
        public string condition { get; set; }

        [JsonProperty("grade")]
        public string grade { get; set; }
        [JsonProperty("metal")]
        public string metal { get; set; }
        [JsonProperty("coating")]
        public string coating { get; set; }

        [JsonProperty("mtr")]
        public string mtr { get; set; }

        [JsonProperty("pricePerPiece")]
        public decimal? pricePerPiece { get; set; }

        [JsonProperty("pricePerCWT")]
        public decimal? pricePerCWT { get; set; }

        [JsonProperty("pricePerFt")]
        public decimal? pricePerFt { get; set; }

        [JsonProperty("serialized")]
        public bool serialized { get; set; }

        [JsonProperty("quantity")]
        public int? quantity { get; set; }

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
