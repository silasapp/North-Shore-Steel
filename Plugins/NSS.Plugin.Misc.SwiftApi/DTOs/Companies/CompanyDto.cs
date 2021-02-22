using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.DTO.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.DTOs.Companies
{
    [JsonObject(Title = "company")]
    public class CompanyDto : BaseDto
    {
        [JsonProperty("companyId")]
        public int ErpCompanyId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hasCreditTerms")]
        public bool HasCreditTerms { get; set; }

        [JsonProperty("salesContactName")]
        public string SalesContactName { get; set; }   
        
        [JsonProperty("salesContactEmail")]
        public string SalesContactEmail { get; set; }    
        
        [JsonProperty("salesContactPhone")]
        public string SalesContactPhone { get; set; }        
        
        [JsonProperty("salesContactImageUrl")]
        public string SalesContactImageUrl { get; set; }
    }
}
