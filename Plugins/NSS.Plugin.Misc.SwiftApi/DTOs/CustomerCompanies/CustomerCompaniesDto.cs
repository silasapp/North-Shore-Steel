using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace NSS.Plugin.Misc.SwiftApi.DTOs.CustomerCompanies
{
    [JsonObject(Title = "userCompany")]
    public class CustomerCompaniesDto
    {
        /// <summary>
        /// Gets or sets the Company entity identifier
        /// </summary>
        [JsonProperty("companyId", Required = Required.Always)]
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the Company name
        /// </summary>
        [JsonProperty("companyName", Required = Required.Always)]
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the Company name
        /// </summary>
        [JsonProperty("buyer", Required = Required.Always)]
        public bool Buyer { get; set; }

        /// <summary>
        /// Gets or sets the Company name
        /// </summary>
        [JsonProperty("buyerCredit", Required = Required.Always)]
        public bool CanCredit { get; set; }

        /// <summary>
        /// Gets or sets the Company name
        /// </summary>
        [JsonProperty("AP", Required = Required.Always)]
        public bool AP { get; set; }

        /// <summary>
        /// Gets or sets the Company name
        /// </summary>
        [JsonProperty("operations", Required = Required.Always)]
        public bool Operations { get; set; }

        /// <summary>
        /// Gets or sets the Sales contact
        /// </summary>
        //[JsonProperty("salesContact")]
        //public SalesContact SalesContact { get; set; }


        /// <summary>
        /// Gets or sets a sales contact name
        /// </summary>
        [JsonProperty("salesContactName")]
        public string SalesContactName { get; set; }

        /// <summary>
        /// Gets or sets a sales contact email
        /// </summary>
        [JsonProperty("salesContactEmail")]
        public string SalesContactEmail { get; set; }

        /// <summary>
        /// Gets or sets a sales contact phone
        /// </summary>
        [JsonProperty("salesContactPhone")]
        public string SalesContactPhone { get; set; }

        /// <summary>
        /// Gets or sets a sales contact Image Url
        /// </summary>
        [JsonProperty("salesContactImageUrl")]
        public string SalesContactImageUrl { get; set; }
    }

    public class SalesContact
    {
        /// <summary>
        /// Gets or sets a sales contact live chat id
        /// </summary>
        [JsonProperty("liveChatId")]
        public string LiveChatId { get; set; }

        /// <summary>
        /// Gets or sets a sales contact name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a sales contact email
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a sales contact phone
        /// </summary>
        [JsonProperty("phone")]
        public string Phone { get; set; }
    }
}
