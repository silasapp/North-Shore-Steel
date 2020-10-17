using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Models
{
    public partial class ErpProductModel
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

        [JsonProperty("weightPerFoot")]
        public decimal? weightPerFoot { get; set; }

        [JsonProperty("weight")]
        public decimal? weight { get; set; }

        [JsonProperty("condition")]
        public string condition { get; set; }

        [JsonProperty("grade")]
        public string grade { get; set; }

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
            return typeof(ErpProductModel);
        }
    }
}
