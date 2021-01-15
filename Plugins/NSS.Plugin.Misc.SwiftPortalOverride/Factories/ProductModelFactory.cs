using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Seo;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using static NSS.Plugin.Misc.SwiftPortalOverride.Models.ProductOverviewModel;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public partial class ProductModelFactory : IProductModelFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IShapeService _shapeService;
        private readonly IGenericAttributeService _genericAttributeService;

        public ProductModelFactory(
            ILocalizationService localizationService,
            ISpecificationAttributeService specificationAttributeService,
            IShapeService shapeService,
            IGenericAttributeService genericAttributeService,
            IUrlRecordService urlRecordService
            )
        {
            _localizationService = localizationService;
            _specificationAttributeService = specificationAttributeService;
            _shapeService = shapeService;
            _genericAttributeService = genericAttributeService;
            _urlRecordService = urlRecordService;
        }

        public IEnumerable<ProductOverviewModel> PrepareSwiftProductOverviewmodel(IEnumerable<Product> products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = _localizationService.GetLocalized(product, x => x.Name),
                    ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription),
                    FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription),
                    SeName = _urlRecordService.GetSeName(product),
                    Sku = product.Sku,
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
                };

                //price
                //if (preparePriceModel)
                //{
                //    model.ProductPrice = PrepareProductOverviewPriceModel(product, forceRedirectionAfterAddingToCart);
                //}

                //picture
                //if (preparePictureModel)
                //{
                //    model.DefaultPictureModel = PrepareProductOverviewPictureModel(product, productThumbPictureSize);
                //}

                //specs

                //model.SpecificationAttributeModels = PrepareProductSpecificationModel(product);

                //reviews
                //model.ReviewOverviewModel = PrepareProductReviewOverviewModel(product);

                // erp
                var attr = _genericAttributeService.GetAttributesForEntity(model.Id, nameof(Product));
                //if(int.TryParse(attr.FirstOrDefault(x => x.Key == "shapeId")?.Value, out int shapeId))
                //    model.Shape = _shapeService.GetShapeById(shapeId);

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
        public virtual IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            return _specificationAttributeService.GetProductSpecificationAttributes(product.Id, 0, null, true)
                .Select(psa =>
                {
                    var specAttributeOption =
                        _specificationAttributeService.GetSpecificationAttributeOptionById(
                            psa.SpecificationAttributeOptionId);
                    var specAttribute =
                        _specificationAttributeService.GetSpecificationAttributeById(specAttributeOption
                            .SpecificationAttributeId);

                    var m = new ProductSpecificationModel
                    {
                        SpecificationAttributeId = specAttribute.Id,
                        SpecificationAttributeName = _localizationService.GetLocalized(specAttribute, x => x.Name),
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
                                    _localizationService.GetLocalized(specAttributeOption, x => x.Name));
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw =
                                WebUtility.HtmlEncode(_localizationService.GetLocalized(psa, x => x.CustomValue));
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            m.ValueRaw = _localizationService.GetLocalized(psa, x => x.CustomValue);
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            m.ValueRaw = $"<a href='{psa.CustomValue}' target='_blank'>{psa.CustomValue}</a>";
                            break;
                        default:
                            break;
                    }

                    return m;
                }).ToList();
        }
    }

}
