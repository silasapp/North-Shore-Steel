using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Shipping.NSS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSS.Plugin.Shipping.NSS
{
    public class ShippingChargeComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        private readonly ShippingChargeService _shippingChargeService;
        public ShippingChargeComputationMethod(ShippingChargeService shippingChargeService)
        {
            _shippingChargeService = shippingChargeService;
        }

        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            if (!getShippingOptionRequest.Items?.Any() ?? true)
                return new GetShippingOptionResponse { Errors = new[] { "No shipment items" } };

            if (getShippingOptionRequest.ShippingAddress?.CountryId == null)
                return new GetShippingOptionResponse { Errors = new[] { "Shipping address is not set" } };

            return _shippingChargeService.GetRates(getShippingOptionRequest);
        }

        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            return null;
        }


        public ShippingRateComputationMethodType ShippingRateComputationMethodType => ShippingRateComputationMethodType.Realtime;

        public IShipmentTracker ShipmentTracker => null;
    }
}
