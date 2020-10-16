﻿using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Nop.Web.Factories
{
    public partial class ProductModelFactory : IProductModelFactory
    {
        public ILocalizationService _localizationService;
        public IUrlRecordService _urlRecordService;
        public ISpecificationAttributeService _specificationAttributeService;
        public IShapeService _shapeService;

        public ProductModelFactory(
            ILocalizationService localizationService,
            ISpecificationAttributeService specificationAttributeService
            )
        {
            _localizationService = localizationService;
            _specificationAttributeService = specificationAttributeService;

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

                model.SpecificationAttributeModels = PrepareProductSpecificationModel(product);

                //reviews
                //model.ReviewOverviewModel = PrepareProductReviewOverviewModel(product);

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
