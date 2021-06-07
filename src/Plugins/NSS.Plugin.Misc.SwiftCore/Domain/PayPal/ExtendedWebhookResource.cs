﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Domain.PayPal
{
    /// <summary>
    /// Represents an extended webhook resource object. Some important properties were missed in the original object.
    /// </summary>
    [DataContract]
    public class ExtendedWebhookResource
    {
        /// <summary>
        /// The API caller-provided external ID. Used to reconcile client transactions with PayPal transactions. Appears in transaction and settlement reports but is not visible to the payer.
        /// </summary>
        [DataMember(Name = "custom_id", EmitDefaultValue = false)]
        public string CustomId { get; set; }
    }
}
