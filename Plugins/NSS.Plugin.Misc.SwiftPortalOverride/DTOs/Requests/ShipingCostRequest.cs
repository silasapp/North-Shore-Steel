﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests
{
    public class ShippingCostRequest
    {
        public bool IsPickup { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int? StateProvinceId { get; set; }
        public string City { get; set; }
        public string ZipPostalCode { get; set; }
    }
}