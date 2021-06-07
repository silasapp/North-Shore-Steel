using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public class ERPUpdateNotificationPreferencesRequest
    {
        public Notificatioon[] Preferences { get; set; }
    }


    public class Notificatioon
    {
        public string Key { get; set; }
        public bool Value { get; set; }
    }
}
