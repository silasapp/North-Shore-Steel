using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NSS.Plugin.DiscountRules.TenThousandLBS.Models
{
    public class RequirementModel
    {
        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        public bool IsApplied { get; set; }
    }
}