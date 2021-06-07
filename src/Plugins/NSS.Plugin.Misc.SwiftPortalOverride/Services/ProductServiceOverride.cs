using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Core.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Services
{
    public class ProductServiceOverride : ProductService
    {
        public ProductServiceOverride(CatalogSettings catalogSettings, CommonSettings commonSettings, IAclService aclService, ICustomerService customerService, INopDataProvider dataProvider, IDateRangeService dateRangeService, IEventPublisher eventPublisher, ILanguageService languageService, ILocalizationService localizationService, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IRepository<AclRecord> aclRepository, IRepository<CrossSellProduct> crossSellProductRepository, IRepository<DiscountProductMapping> discountProductMappingRepository, IRepository<Product> productRepository, IRepository<ProductAttributeCombination> productAttributeCombinationRepository, IRepository<ProductAttributeMapping> productAttributeMappingRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductPicture> productPictureRepository, IRepository<ProductReview> productReviewRepository, IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository, IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, IRepository<RelatedProduct> relatedProductRepository, IRepository<Shipment> shipmentRepository, IRepository<StockQuantityHistory> stockQuantityHistoryRepository, IRepository<StoreMapping> storeMappingRepository, IRepository<TierPrice> tierPriceRepository, IRepository<Warehouse> warehouseRepositor, IStaticCacheManager staticCacheManager, IStoreService storeService, IStoreMappingService storeMappingService, IWorkContext workContext, LocalizationSettings localizationSettings) : base(catalogSettings, commonSettings, aclService, cacheKeyService, customerService, dataProvider, dateRangeService, eventPublisher, languageService, localizationService, productAttributeParser, productAttributeService, aclRepository, crossSellProductRepository, discountProductMappingRepository, productRepository, productAttributeCombinationRepository, productAttributeMappingRepository, productCategoryRepository, productPictureRepository, productReviewRepository, productReviewHelpfulnessRepository, productWarehouseInventoryRepository, relatedProductRepository, shipmentRepository, stockQuantityHistoryRepository, storeMappingRepository, tierPriceRepository, warehouseRepositor, staticCacheManager, storeService, storeMappingService, workContext, localizationSettings)
        {
        }

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="shapeIds">Shape identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="markedAsNewOnly">A values indicating whether to load only products marked as "new"; "false" to load all records; "true" to load "marked as new" only</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>Products</returns>
        public override async Task<(IPagedList<Product>, IList<int> filterableSpecificationAttributeOptionIds)> SearchProductsAsync(
         
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> shapeIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
           var filterableSpecificationAttributeOptionIds = new List<int>();

            //search by keyword
            var searchLocalizedValue = false;
            if (languageId > 0)
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
                    var totalPublishedLanguages = _languageService.GetAllLanguagesAsync().Result.Count();
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }

            //validate "shapeIds" parameter
            if (shapeIds != null && shapeIds.Contains(0))
                shapeIds.Remove(0);

            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());

            //pass category identifiers as comma-delimited string
            var commaSeparatedShapeIds = shapeIds == null ? string.Empty : string.Join(",", shapeIds);

            //pass customer role identifiers as comma-delimited string
            var commaSeparatedAllowedCustomerRoleIds = string.Join(",", allowedCustomerRolesIds);

            //pass specification identifiers as comma-delimited string
            var commaSeparatedSpecIds = string.Empty;
            if (filteredSpecs != null)
            {
                ((List<int>)filteredSpecs).Sort();
                commaSeparatedSpecIds = string.Join(",", filteredSpecs);
            }

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            //prepare input parameters
            var pCategoryIds = SqlParameterHelper.GetStringParameter("ShapeIds", commaSeparatedShapeIds);
            var pManufacturerId = SqlParameterHelper.GetInt32Parameter("ManufacturerId", manufacturerId);
            var pStoreId = SqlParameterHelper.GetInt32Parameter("StoreId", !_catalogSettings.IgnoreStoreLimitations ? storeId : 0);
            var pVendorId = SqlParameterHelper.GetInt32Parameter("VendorId", vendorId);
            var pWarehouseId = SqlParameterHelper.GetInt32Parameter("WarehouseId", warehouseId);
            var pProductTypeId = SqlParameterHelper.GetInt32Parameter("ProductTypeId", (int?)productType);
            var pVisibleIndividuallyOnly = SqlParameterHelper.GetBooleanParameter("VisibleIndividuallyOnly", visibleIndividuallyOnly);
            var pMarkedAsNewOnly = SqlParameterHelper.GetBooleanParameter("MarkedAsNewOnly", markedAsNewOnly);
            var pProductTagId = SqlParameterHelper.GetInt32Parameter("ProductTagId", productTagId);
            var pFeaturedProducts = SqlParameterHelper.GetBooleanParameter("FeaturedProducts", featuredProducts);
            var pPriceMin = SqlParameterHelper.GetDecimalParameter("PriceMin", priceMin);
            var pPriceMax = SqlParameterHelper.GetDecimalParameter("PriceMax", priceMax);
            var pKeywords = SqlParameterHelper.GetStringParameter("Keywords", keywords);
            var pSearchDescriptions = SqlParameterHelper.GetBooleanParameter("SearchDescriptions", searchDescriptions);
            var pSearchManufacturerPartNumber = SqlParameterHelper.GetBooleanParameter("SearchManufacturerPartNumber", searchManufacturerPartNumber);
            var pSearchSku = SqlParameterHelper.GetBooleanParameter("SearchSku", searchSku);
            var pSearchProductTags = SqlParameterHelper.GetBooleanParameter("SearchProductTags", searchProductTags);
            var pUseFullTextSearch = SqlParameterHelper.GetBooleanParameter("UseFullTextSearch", _commonSettings.UseFullTextSearch);
            var pFullTextMode = SqlParameterHelper.GetInt32Parameter("FullTextMode", (int)_commonSettings.FullTextMode);
            var pFilteredSpecs = SqlParameterHelper.GetStringParameter("FilteredSpecs", commaSeparatedSpecIds);
            var pLanguageId = SqlParameterHelper.GetInt32Parameter("LanguageId", searchLocalizedValue ? languageId : 0);
            var pOrderBy = SqlParameterHelper.GetInt32Parameter("OrderBy", (int)orderBy);
            var pAllowedCustomerRoleIds = SqlParameterHelper.GetStringParameter("AllowedCustomerRoleIds", !_catalogSettings.IgnoreAcl ? commaSeparatedAllowedCustomerRoleIds : string.Empty);
            var pPageIndex = SqlParameterHelper.GetInt32Parameter("PageIndex", pageIndex);
            var pPageSize = SqlParameterHelper.GetInt32Parameter("PageSize", pageSize);
            var pShowHidden = SqlParameterHelper.GetBooleanParameter("ShowHidden", showHidden);
            var pOverridePublished = SqlParameterHelper.GetBooleanParameter("OverridePublished", overridePublished);
            var pLoadFilterableSpecificationAttributeOptionIds = SqlParameterHelper.GetBooleanParameter("LoadFilterableSpecificationAttributeOptionIds", loadFilterableSpecificationAttributeOptionIds);

            //prepare output parameters
            var pFilterableSpecificationAttributeOptionIds = SqlParameterHelper.GetOutputStringParameter("FilterableSpecificationAttributeOptionIds");
            pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
            var pTotalRecords = SqlParameterHelper.GetOutputInt32Parameter("TotalRecords");

            //invoke stored procedure
            var products = await _productRepository.EntityFromSqlAsync("ProductLoadAllPagedSwiftPortal",
                pCategoryIds,
                pManufacturerId,
                pStoreId,
                pVendorId,
                pWarehouseId,
                pProductTypeId,
                pVisibleIndividuallyOnly,
                pMarkedAsNewOnly,
                pProductTagId,
                pFeaturedProducts,
                pPriceMin,
                pPriceMax,
                pKeywords,
                pSearchDescriptions,
                pSearchManufacturerPartNumber,
                pSearchSku,
                pSearchProductTags,
                pUseFullTextSearch,
                pFullTextMode,
                pFilteredSpecs,
                pLanguageId,
                pOrderBy,
                pAllowedCustomerRoleIds,
                pPageIndex,
                pPageSize,
                pShowHidden,
                pOverridePublished,
                pLoadFilterableSpecificationAttributeOptionIds,
                pFilterableSpecificationAttributeOptionIds,
                pTotalRecords).ToList();

            //get filterable specification attribute option identifier
            var filterableSpecificationAttributeOptionIdsStr =
                pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value
                    ? (string)pFilterableSpecificationAttributeOptionIds.Value
                    : string.Empty;

            if (loadFilterableSpecificationAttributeOptionIds &&
                !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
            {
                filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x.Trim()))
                    .ToList();
            }
            //return products
            var totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;

            return (new PagedList<Product>(products, pageIndex, pageSize, totalRecords), ( pFilterableSpecificationAttributeOptionIds));
        }
    }
}
