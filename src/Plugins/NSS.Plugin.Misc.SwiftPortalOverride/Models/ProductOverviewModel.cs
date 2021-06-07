using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using NSS.Plugin.Misc.SwiftCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial class ProductOverviewModel : BaseNopEntityModel
    {
        public ProductOverviewModel()
        {
            ProductPrice = new ProductPriceModel();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
            ReviewOverviewModel = new ProductReviewOverviewModel();

            // erp custom
            ErpProduct = new ErpProductModel();
            Shape = new Shape();
            ProductCustomAttributes = new List<GenericAttribute>();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }

        public bool IsFavoriteItem { get; set; }
        public bool IsCartItem { get; set; }
        public int? CartQuantity { get; set; }

        public string Sku { get; set; }

        public ProductType ProductType { get; set; }

        public bool MarkAsNew { get; set; }

        // swift props
        public Shape Shape { get; set; }
        public IList<GenericAttribute> ProductCustomAttributes { get; set; }
        public ErpProductModel ErpProduct { get; set; } 

        //price
        public ProductPriceModel ProductPrice { get; set; }
        //specification attributes
        public IList<ProductSpecificationModel> SpecificationAttributeModels { get; set; }
        //price
        public ProductReviewOverviewModel ReviewOverviewModel { get; set; }

        #region Nested Classes

        public partial class ProductPriceModel : BaseNopModel
        {
            public string OldPrice { get; set; }
            public string Price { get; set; }
            public decimal PriceValue { get; set; }
            /// <summary>
            /// PAngV baseprice (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }

            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }
            public bool DisableAddToCompareListButton { get; set; }

            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

            public bool IsRental { get; set; }

            public bool ForceRedirectionAfterAddingToCart { get; set; }

            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
        }

        /// <summary>
        /// Specification attribute model
        /// </summary>
        public partial class ProductSpecificationModel : BaseNopModel
        {
            /// <summary>
            /// Specificartion attribute ID
            /// </summary>
            public int SpecificationAttributeId { get; set; }

            /// <summary>
            /// Specificartion attribute name
            /// </summary>
            public string SpecificationAttributeName { get; set; }
            public string SpecificationAttributeOptionName { get; set; }
            public int SpecificationAttributeOptionId { get; set; }

            /// <summary>
            /// Option value
            /// this value is already HTML encoded
            /// </summary>
            public string ValueRaw { get; set; }

            /// <summary>
            /// Option color (if specified). Used to display color squares
            /// </summary>
            public string ColorSquaresRgb { get; set; }

            /// <summary>
            /// Attribyte type ID
            /// </summary>
            public int AttributeTypeId { get; set; }
        }

        #endregion
    }
}
