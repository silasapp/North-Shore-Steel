using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Shipping.NSS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Shipping.NSS
{
    public class ShippingChargeComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        private readonly ShippingChargeService _shippingChargeService;
        public ShippingChargeComputationMethod(ShippingChargeService shippingChargeService)
        {
            _shippingChargeService = shippingChargeService;
        }

        public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            if (!getShippingOptionRequest.Items?.Any() ?? true)
                return new GetShippingOptionResponse { Errors = new[] { "No shipment items" } };

            if (getShippingOptionRequest.ShippingAddress?.CountryId == null)
                return new GetShippingOptionResponse { Errors = new[] { "Shipping address is not set" } };

            return await _shippingChargeService.GetRatesAsync(getShippingOptionRequest);
        }

        public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            return Task.FromResult<decimal?>(null);
        }


        public IShipmentTracker ShipmentTracker => null;
    }
}
