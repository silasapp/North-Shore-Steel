using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Shipping;
using NSS.Plugin.Misc.SwiftCore.DTOs;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Shipping.NSS.Services
{
    public class ShippingChargeService
    {
        #region Fields

        private readonly IApiService _apiService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IShapeService _shapeService;

        #endregion

        #region Ctor

        public ShippingChargeService(IApiService apiService, IStateProvinceService stateProvinceService, IGenericAttributeService genericAttributeService, IShapeService shapeService)
        {
            _apiService = apiService;
            _stateProvinceService = stateProvinceService;
            _genericAttributeService = genericAttributeService;
            _shapeService = shapeService;
        }

        #endregion

        #region Methods
        public virtual async Task<GetShippingOptionResponse> GetRatesAsync(GetShippingOptionRequest shippingOptionRequest)
        {
            var (shippingOptions, error) = await GetShippingOptionsAsync(shippingOptionRequest);

            GetShippingOptionResponse response = new GetShippingOptionResponse { 
                ShippingOptions = shippingOptions,
            };

            if (!string.IsNullOrEmpty(error))
                response.AddError(error);

            return response;
        }

        private async Task<(IList<ShippingOption> shippingOptions, string error)> GetShippingOptionsAsync(GetShippingOptionRequest shippingOptionRequest)
        {
            var request = await CreateShippingChargeRequestAsync(shippingOptionRequest);
            var (error, response) = await _apiService.GetShippingRateAsync(request);

            var options = new List<ShippingOption>();

            if (response != null && string.IsNullOrEmpty(error))
            {
                var option = new ShippingOption
                {
                    Name = "Delivery",
                    Rate = response.ShippingCost,
                    TransitDays = DateTime.TryParse(response.DeliveryDate, out var deliveryDate) ? (int?)(deliveryDate - DateTime.Now).TotalDays : null
                };

                options.Add(option);
            }

            return (options, error);
        }

        private async Task<ERPCalculateShippingRequest> CreateShippingChargeRequestAsync(GetShippingOptionRequest shippingOptionRequest)
        {
            var shipStateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(shippingOptionRequest.ShippingAddress);

            var request = new ERPCalculateShippingRequest
            {
                DeliveryMethod = "Shipping",
                DestinationAddressLine1 = shippingOptionRequest.ShippingAddress.Address1,
                DestinationAddressLine2 = shippingOptionRequest.ShippingAddress.Address2,
                State = shipStateProvince?.Abbreviation,
                City = shippingOptionRequest.ShippingAddress.City,
                PostalCode = shippingOptionRequest.ShippingAddress.ZipPostalCode,
                PickupLocationId = 0
            };

            // build package items
            var orderItems = new List<Item>();
            request.OrderWeight = 0;

            var cart = shippingOptionRequest.Items;

            foreach (var item in cart)
            {
                var attr = await _genericAttributeService.GetAttributesForEntityAsync(item.Product.Id, nameof(Product));

                decimal.TryParse(attr.FirstOrDefault(x => x.Key == "weight")?.Value, out decimal weight);
                int.TryParse(attr.FirstOrDefault(x => x.Key == "shapeId")?.Value, out int shapeId);
                int.TryParse(attr.FirstOrDefault(x => x.Key == "itemId")?.Value, out int itemId);
                decimal.TryParse(attr.FirstOrDefault(x => x.Key == "lengthFt")?.Value, out decimal length);

                request.OrderWeight += (int)Math.Round(weight * item.GetQuantity(), 0);

                if (length > request.MaxLength)
                    request.MaxLength = (int)Math.Round(length);

                var shape = _shapeService.GetShapeByIdAsync(shapeId);

                orderItems.Add(new Item
                {
                    ItemId = itemId,
                    ShapeId = shapeId,
                    ShapeName = shape?.Name
                });
            }

            request.Items = JsonConvert.SerializeObject(orderItems.ToArray());

            return request;
        }

        #endregion
    }
}
