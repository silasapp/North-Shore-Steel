﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.SwiftPortalOverride.DTOs.Requests
{
    public class SwiftAuthenticateRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}