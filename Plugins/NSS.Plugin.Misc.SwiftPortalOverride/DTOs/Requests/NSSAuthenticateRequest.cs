﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests
{
    public class NSSAuthenticateRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}