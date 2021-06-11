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
        public ProductServiceOverride(CatalogSettings catalogSettings, CommonSettings commonSettings, IAclService aclService, ICustomerService customerService, IDateRangeService dateRangeService, ILanguageService languageService, ILocalizationService localizationService, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IRepository<CrossSellProduct> crossSellProductRepository, IRepository<DiscountProductMapping> discountProductMappingRepository, IRepository<LocalizedProperty> localizedPropertyRepository, IRepository<Product> productRepository, IRepository<ProductAttributeCombination> productAttributeCombinationRepository, IRepository<ProductAttributeMapping> productAttributeMappingRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductManufacturer> productManufacturerRepository, IRepository<ProductPicture> productPictureRepository, IRepository<ProductProductTagMapping> productTagMappingRepository, IRepository<ProductReview> productReviewRepository, IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository, IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, IRepository<ProductTag> productTagRepository, IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, IRepository<RelatedProduct> relatedProductRepository, IRepository<Shipment> shipmentRepository, IRepository<StockQuantityHistory> stockQuantityHistoryRepository, IRepository<TierPrice> tierPriceRepository, IRepository<Warehouse> warehouseRepository, IStaticCacheManager staticCacheManager, IStoreService storeService, IStoreMappingService storeMappingService, IWorkContext workContext, LocalizationSettings localizationSettings) : base(catalogSettings, commonSettings, aclService, customerService, dateRangeService, languageService, localizationService, productAttributeParser, productAttributeService, crossSellProductRepository, discountProductMappingRepository, localizedPropertyRepository, productRepository, productAttributeCombinationRepository, productAttributeMappingRepository, productCategoryRepository, productManufacturerRepository, productPictureRepository, productTagMappingRepository, productReviewRepository, productReviewHelpfulnessRepository, productSpecificationAttributeRepository, productTagRepository, productWarehouseInventoryRepository, relatedProductRepository, shipmentRepository, stockQuantityHistoryRepository, tierPriceRepository, warehouseRepository, staticCacheManager, storeService, storeMappingService, workContext, localizationSettings)
        {
        }


        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerIds">Manufacturer identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecOptions">Specification options list to filter products; null to load all records</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public override async Task<IPagedList<Product>> SearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> shapeIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
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
            if (filteredSpecOptions != null)
            {
                ((List<int>)filteredSpecOptions).Sort();
                commaSeparatedSpecIds = string.Join(",", filteredSpecOptions);
            }

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            //prepare input parameters
            var pCategoryIds = SqlParameterHelper.GetStringParameter("ShapeIds", commaSeparatedShapeIds);
            var pManufacturerId = SqlParameterHelper.GetInt32Parameter("ManufacturerId", 0);
            var pStoreId = SqlParameterHelper.GetInt32Parameter("StoreId", !_catalogSettings.IgnoreStoreLimitations ? storeId : 0);
            var pVendorId = SqlParameterHelper.GetInt32Parameter("VendorId", vendorId);
            var pWarehouseId = SqlParameterHelper.GetInt32Parameter("WarehouseId", warehouseId);
            var pProductTypeId = SqlParameterHelper.GetInt32Parameter("ProductTypeId", (int?)productType);
            var pVisibleIndividuallyOnly = SqlParameterHelper.GetBooleanParameter("VisibleIndividuallyOnly", visibleIndividuallyOnly);
            var pMarkedAsNewOnly = SqlParameterHelper.GetBooleanParameter("MarkedAsNewOnly", false);
            var pProductTagId = SqlParameterHelper.GetInt32Parameter("ProductTagId", productTagId);
            var pFeaturedProducts = SqlParameterHelper.GetBooleanParameter("FeaturedProducts", null);
            var pPriceMin = SqlParameterHelper.GetDecimalParameter("PriceMin", priceMin);
            var pPriceMax = SqlParameterHelper.GetDecimalParameter("PriceMax", priceMax);
            var pKeywords = SqlParameterHelper.GetStringParameter("Keywords", keywords);
            var pSearchDescriptions = SqlParameterHelper.GetBooleanParameter("SearchDescriptions", searchDescriptions);
            var pSearchManufacturerPartNumber = SqlParameterHelper.GetBooleanParameter("SearchManufacturerPartNumber", searchManufacturerPartNumber);
            var pSearchSku = SqlParameterHelper.GetBooleanParameter("SearchSku", searchSku);
            var pSearchProductTags = SqlParameterHelper.GetBooleanParameter("SearchProductTags", searchProductTags);
            var pUseFullTextSearch = SqlParameterHelper.GetBooleanParameter("UseFullTextSearch", false);
            var pFullTextMode = SqlParameterHelper.GetInt32Parameter("FullTextMode", 0);
            var pFilteredSpecs = SqlParameterHelper.GetStringParameter("FilteredSpecs", commaSeparatedSpecIds);
            var pLanguageId = SqlParameterHelper.GetInt32Parameter("LanguageId", searchLocalizedValue ? languageId : 0);
            var pOrderBy = SqlParameterHelper.GetInt32Parameter("OrderBy", (int)orderBy);
            var pAllowedCustomerRoleIds = SqlParameterHelper.GetStringParameter("AllowedCustomerRoleIds", !_catalogSettings.IgnoreAcl ? commaSeparatedAllowedCustomerRoleIds : string.Empty);
            var pPageIndex = SqlParameterHelper.GetInt32Parameter("PageIndex", pageIndex);
            var pPageSize = SqlParameterHelper.GetInt32Parameter("PageSize", pageSize);
            var pShowHidden = SqlParameterHelper.GetBooleanParameter("ShowHidden", showHidden);
            var pOverridePublished = SqlParameterHelper.GetBooleanParameter("OverridePublished", overridePublished);
            var pLoadFilterableSpecificationAttributeOptionIds = SqlParameterHelper.GetBooleanParameter("LoadFilterableSpecificationAttributeOptionIds", false);

            //prepare output parameters
            var pFilterableSpecificationAttributeOptionIds = SqlParameterHelper.GetOutputStringParameter("FilterableSpecificationAttributeOptionIds");
            pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
            var pTotalRecords = SqlParameterHelper.GetOutputInt32Parameter("TotalRecords");

            //invoke stored procedure
            var products = (await _productRepository.EntityFromSqlAsync("ProductLoadAllPagedSwiftPortal",
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
                pTotalRecords)).ToList();

            //get filterable specification attribute option identifier
            var filterableSpecificationAttributeOptionIdsStr =
                pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value
                    ? (string)pFilterableSpecificationAttributeOptionIds.Value
                    : string.Empty;

            if (false &&
                !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
            {
                filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x.Trim()))
                    .ToList();
            }
            //return products
            var totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;

            return (new PagedList<Product>(products, pageIndex, pageSize, totalRecords));
        }

    }
}
