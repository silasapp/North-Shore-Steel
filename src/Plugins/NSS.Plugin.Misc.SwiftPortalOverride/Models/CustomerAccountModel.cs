using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public record CustomerAccountModel : BaseNopModel
    {
        public Notification Notification { get; set; }

    }
    public class Notification
    {
        public bool IsMyOrderConfirmedEmail { get; set; }
        public bool IsMyOrderConfirmedSms { get; set; }
        public bool IsMyOrderScheduledDateEmail { get; set; }
        public bool IsMyOrderScheduledDateSms { get; set; }
        public bool IsMyOrderPromiseDateEmail { get; set; }
        public bool IsMyOrderPromiseDateSms { get; set; }
        public bool IsMyOrderReadyEmail { get; set; }
        public bool IsMyOrderReadySms { get; set; }
        public bool IsMyOrderLoadingEmail { get; set; }
        public bool IsMyOrderLoadingSms { get; set; }
        public bool IsMyOrderShippedEmail { get; set; }
        public bool IsMyOrderShippedSms { get; set; }
        public bool IsAnyOrderConfirmedEmail { get; set; }
        public bool IsAnyOrderConfirmedSms { get; set; }
        public bool IsAnyOrderShippedEmail { get; set; }
        public bool IsAnyOrderShippedSms { get; set; }
    }
}
