﻿using Nop.Web.Framework.Models;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class SelectAccountModel : BaseNopModel
    {
        public IEnumerable<Company> Companies { get; set; }
    }
}
