using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Seo;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Nop.Core.Domain.Orders;
using static NSS.Plugin.Misc.SwiftPortalOverride.Models.ProductOverviewModel;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public partial class ProductModelFactory : IProductModelFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IShapeService _shapeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public ProductModelFactory(
            ILocalizationService localizationService,
            ISpecificationAttributeService specificationAttributeService,
            IShapeService shapeService,
            IGenericAttributeService genericAttributeService,
            IUrlRecordService urlRecordService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IStoreContext storeContext
            )
        {
            _localizationService = localizationService;
            _specificationAttributeService = specificationAttributeService;
            _shapeService = shapeService;
            _genericAttributeService = genericAttributeService;
            _urlRecordService = urlRecordService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public async Task<IEnumerable<ProductOverviewModel>> PrepareSwiftProductOverviewmodelAsync(IEnumerable<Product> products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<ProductOverviewModel>();
            var wishlistItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);
            var cartItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    IsFavoriteItem = wishlistItems.Any(x => x.ProductId == product.Id),
                    IsCartItem = cartItems.Any(x => x.ProductId == product.Id),
                    CartQuantity = cartItems.FirstOrDefault(x => x.ProductId == product.Id)?.Quantity,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                    FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                    SeName = await _urlRecordService.GetSeNameAsync(product),
                    Sku = product.Sku,
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
                };


                // erp
                var attr = await _genericAttributeService.GetAttributesForEntityAsync(model.Id, nameof(Product));

                model.ProductCustomAttributes = attr;


                models.Add(model);
            }

            return models;
        }

        /// <summary>
        /// Prepare the product specification models
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of product specification model</returns>
        public virtual async Task<IList<ProductSpecificationModel>> PrepareProductSpecificationModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            return await (await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id, 0, null, true))
                .SelectAwait(async psa =>
                {
                    var specAttributeOption =
                       await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(
                            psa.SpecificationAttributeOptionId);
                    var specAttribute =
                       await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specAttributeOption
                            .SpecificationAttributeId);

                    var m = new ProductSpecificationModel
                    {
                        SpecificationAttributeId = specAttribute.Id,
                        SpecificationAttributeName = await _localizationService.GetLocalizedAsync(specAttribute, x => x.Name),
                        SpecificationAttributeOptionId = specAttributeOption.Id,
                        SpecificationAttributeOptionName = specAttributeOption.Name,
                        ColorSquaresRgb = specAttributeOption.ColorSquaresRgb,
                        AttributeTypeId = psa.AttributeTypeId
                    };

                    switch (psa.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            m.ValueRaw =
                                WebUtility.HtmlEncode(
                                  await _localizationService.GetLocalizedAsync(specAttributeOption, x => x.Name));
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw =
                                WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue));
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            m.ValueRaw = await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue);
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            m.ValueRaw = $"<a href='{psa.CustomValue}' target='_blank'>{psa.CustomValue}</a>";
                            break;
                        default:
                            break;
                    }

                    return m;
                }).ToListAsync();
        }
    }

}
