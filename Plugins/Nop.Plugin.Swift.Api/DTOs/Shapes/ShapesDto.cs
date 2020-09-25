using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Swift.Api.DTOs.Shapes
{
    public class ShapesDto
    {
        /// <summary>
        /// Gets or sets the list of Shapes
        /// </summary>
        [JsonProperty("shapes", Required = Required.Always)]
        public List<ShapeDto> Shapes { get; set; }

    }

    public class ShapeDto
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Shape name
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the saw option
        /// </summary>
        [JsonProperty("sawOption", Required = Required.Always)]
        public bool SawOption { get; set; }

        /// <summary>
        /// Gets or sets the order
        /// </summary>
        [JsonProperty("order", Required = Required.Always)]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the attributes
        /// </summary>
        [JsonProperty("attributes", Required = Required.Always)]
        public List<ShapeAttributeDto> Atttributes { get; set; }

        /// <summary>
        /// Gets or sets the sub categories
        /// </summary>
        [JsonProperty("subCategories", Required = Required.Always)]
        public List<ShapeDto> SubCategories { get; set; }
    }

    public class ShapeAttributeDto
    {
        /// <summary>
        /// Gets or sets the code
        /// </summary>
        [JsonProperty("code", Required = Required.Always)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        [JsonProperty("displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the order
        /// </summary>
        [JsonProperty("order", Required = Required.Always)]
        public int Order { get; set; }
    }

}
