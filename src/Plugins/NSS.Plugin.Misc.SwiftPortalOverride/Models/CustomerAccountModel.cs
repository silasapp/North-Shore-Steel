using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class CustomerAccountModel : BaseNopModel
    {
        public Notification Notification { get; set; }

    }
    public class Notification
    {
        public bool isMyOrderConfirmedEmail { get; set; }
        public bool isMyOrderConfirmedSms { get; set; }
        public bool isMyOrderScheduledDateEmail { get; set; }
        public bool isMyOrderScheduledDateSms { get; set; }
        public bool isMyOrderPromiseDateEmail { get; set; }
        public bool isMyOrderPromiseDateSms { get; set; }
        public bool isMyOrderReadyEmail { get; set; }
        public bool isMyOrderReadySms { get; set; }
        public bool isMyOrderLoadingEmail { get; set; }
        public bool isMyOrderLoadingSms { get; set; }
        public bool isMyOrderShippedEmail { get; set; }
        public bool isMyOrderShippedSms { get; set; }
        public bool isAnyOrderConfirmedEmail { get; set; }
        public bool isAnyOrderConfirmedSms { get; set; }
        public bool isAnyOrderShippedEmail { get; set; }
        public bool isAnyOrderShippedSms { get; set; }
    }
}
