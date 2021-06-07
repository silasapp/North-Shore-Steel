using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using NSS.Plugin.Misc.SwiftCore.Domain.Catalog;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NSS.Plugin.Misc.SwiftPortalOverride.Models.CatalogModel;
using static NSS.Plugin.Misc.SwiftPortalOverride.Models.CatalogPagingFilteringModel;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public partial class CatalogModelFactory : ICatalogModelFactory
    {

        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly ISearchTermService _searchTermService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IShapeService _shapeService;

        public CatalogModelFactory(
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            ISearchTermService searchTermService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            IStaticCacheManager staticCacheManager,
            IShapeService shapeService
            )
        {
            _catalogSettings = catalogSettings;
            _eventPublisher = eventPublisher;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _searchTermService = searchTermService;
            _specificationAttributeService = specificationAttributeService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _staticCacheManager = staticCacheManager;
            _shapeService = shapeService;
        }

        /// <summary>
        /// Prepare search model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Catalog paging filtering command</param>
        /// <returns>Search model</returns>
        public async Task<CatalogModel> PrepareSwiftCatalogModelAsync(IList<int> shapeIds, IList<int> specIds, string searchKeyword = null, bool isPageLoad = false)
        {
            var model = new CatalogModel();
            var searchTerms = searchKeyword; //string.Empty;

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            // only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
            //we don't use "!string.IsNullOrEmpty(searchTerms)" in cases of "ProductSearchTermMinimumLength" set to 0 but searching by other parameters (e.g. category or price filter)
            //var isSearchTermSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("q");

            //if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("q", out var query))
            //    searchTerms = query.FirstOrDefault();


            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            var searchInDescriptions = false;

            //var searchInProductTags = false;
            //var searchInProductTags = searchInDescriptions;

            //products
            IList<int> filterableSpecificationAttributeOptionIds = new List<int>();


            products = await _productService.SearchProductsAsync(
                //pageIndex: command.PageNumber - 1,
                //pageSize: command.PageSize
                categoryIds: shapeIds,
                //manufacturerIds: new List<int> { manufacturerId },
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                visibleIndividuallyOnly: true,
                keywords: searchTerms,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                searchDescriptions: searchInDescriptions
                //searchProductTags: searchInProductTags,
                //languageId: workingLanguage.Id,
                //orderBy: (ProductSortingEnum)command.OrderBy,
                //vendorId: vendorId
                );


            model.Products = (await _productModelFactory.PrepareSwiftProductOverviewmodelAsync(products)).OrderBy(o => o.Sku).ToList();

            model.NoResults = !model.Products.Any();

            //search term statistics
            if (!string.IsNullOrEmpty(searchTerms))
            {
                var searchTerm =
                  await _searchTermService.GetSearchTermByKeywordAsync(searchTerms, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (searchTerm != null)
                {
                    searchTerm.Count++;
                    await _searchTermService.UpdateSearchTermAsync(searchTerm);
                }
                else
                {
                    searchTerm = new SearchTerm
                    {
                        Keyword = searchTerms,
                        StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                        Count = 1
                    };
                    await _searchTermService.InsertSearchTermAsync(searchTerm);
                }

                //event
                await _eventPublisher.PublishAsync(new ProductSearchEvent
                {
                    SearchTerm = searchTerms,
                    SearchInDescriptions = searchInDescriptions,

                    CategoryIds = shapeIds,
                    ManufacturerId = 0,
                    WorkingLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id,
                    VendorId = 0
                });
            }

            //specs
            if (!isPageLoad && (shapeIds.Count > 0 || specIds.Count > 0 || !string.IsNullOrEmpty(searchTerms)))
                (model) = await PrepareSpecsFiltersAsync(filterableSpecificationAttributeOptionIds?.ToArray(), model, shapeIds);
            else
                (model) = await PrepareSpecsFiltersAsync(filterableSpecificationAttributeOptionIds?.ToArray(), model);

            (model) = PrepareShapeFilterModel(model);
            if (isPageLoad)
                (model) = await PrepareShapeDataAsync(model);

            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }

        public async Task<CatalogModel> PrepareSpecsFiltersAsync(int[] filterableSpecificationAttributeOptionIds, CatalogModel model, IList<int> shapeIds)
        {
            model.PagingFilteringContext.SpecificationFilter.Enabled = false;
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(SpecsFilterModelKey, filterableSpecificationAttributeOptionIds, await _workContext.GetWorkingLanguageAsync());

            var allOptions = await _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(filterableSpecificationAttributeOptionIds);
            var allFilters = await _staticCacheManager.GetAsync(cacheKey, async () => await allOptions.SelectAwait(async sao =>
            {
                var specAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(sao.SpecificationAttributeId);

                return new SpecificationAttributeOptionFilter
                {
                    SpecificationAttributeId = specAttribute.Id,
                    SpecificationAttributeName = await _localizationService.GetLocalizedAsync(specAttribute, x => x.Name, (await _workContext.GetWorkingLanguageAsync()).Id),
                    SpecificationAttributeDisplayOrder = specAttribute.DisplayOrder,
                    SpecificationAttributeOptionId = sao.Id,
                    SpecificationAttributeOptionName = await _localizationService.GetLocalizedAsync(sao, x => x.Name, (await _workContext.GetWorkingLanguageAsync()).Id),
                    SpecificationAttributeOptionColorRgb = sao.ColorSquaresRgb,
                    SpecificationAttributeOptionDisplayOrder = sao.DisplayOrder
                };
            }).ToListAsync());

            if (!allFilters.Any())
                return model;

            //prepare the model properties
            model.PagingFilteringContext.SpecificationFilter.Enabled = true;

            //var defaultSpecFilters = new List<string> { SwiftCore.Helpers.Constants.CoatingFieldAttribute, SwiftCore.Helpers.Constants.MetalFieldAttribute };
            var filteredProductIds = model.Products.Select(p => p.Id);

            var specFilters = allFilters.Select(af =>
            {
                var prodSpecs = _specificationAttributeService.GetProductSpecificationAttributesAsync(specificationAttributeOptionId: af.SpecificationAttributeOptionId);

                // filter result
                var prodSpecGroup = prodSpecs.Result.GroupBy(ps => ps.ProductId).Where(p => filteredProductIds.Contains(p.Key));

                return new SpecificationFilterItem
                {
                    SpecificationAttributeName = af.SpecificationAttributeName,
                    SpecificationAttributeOptionId = af.SpecificationAttributeOptionId,
                    SpecificationAttributeOptionName = af.SpecificationAttributeOptionName,
                    ProductCount = prodSpecGroup.Count()
                };

            });

            model.PagingFilteringContext.SpecificationFilter.FilterItems = new List<SpecificationFilterItem>(specFilters);

            return model;

        }

        public async Task<CatalogModel> PrepareSpecsFiltersAsync(int[] filterableSpecificationAttributeOptionIds, CatalogModel model)
        {
            model.PagingFilteringContext.SpecificationFilter.Enabled = false;
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(SpecsFilterModelKey, filterableSpecificationAttributeOptionIds, await _workContext.GetWorkingLanguageAsync());

            var language = await _workContext.GetWorkingLanguageAsync();

            var allOptions = await _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(filterableSpecificationAttributeOptionIds);
            var allFilters = await _staticCacheManager.GetAsync(cacheKey, async () => await allOptions.SelectAwait(async sao =>
            {
                var specAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(sao.SpecificationAttributeId);

                return new SpecificationAttributeOptionFilter
                {
                    SpecificationAttributeId = specAttribute.Id,
                    SpecificationAttributeName = await _localizationService.GetLocalizedAsync(specAttribute, x => x.Name, language.Id),
                    SpecificationAttributeDisplayOrder = specAttribute.DisplayOrder,
                    SpecificationAttributeOptionId = sao.Id,
                    SpecificationAttributeOptionName = await _localizationService.GetLocalizedAsync(sao, x => x.Name, language.Id),
                    SpecificationAttributeOptionColorRgb = sao.ColorSquaresRgb,
                    SpecificationAttributeOptionDisplayOrder = sao.DisplayOrder
                };
            }).ToListAsync());

            model.AllFilters = new List<SpecificationAttributeOptionFilter>(allFilters);

            return model;
        }

        public async Task<CatalogModel> PrepareShapeDataAsync(CatalogModel model)
        {
            var shapes = (await _shapeService.GetShapesAsync()).OrderBy(s => s.Order);

            foreach (var shape in shapes)
            {
                var childShapes = shape?.SubCategories?.ToList() ?? new List<SwiftCore.Domain.Shapes.Shape>();
                foreach (var item in childShapes)
                {
                    var childData = new ShapeData
                    {
                        Id = item.Id,
                        ParentId = item.ParentId,
                        Name = $"{item.Name}",
                        DisplayName = $"{item.Name}",
                        Count = 0,
                        HasChild = false,
                        ShapeAttributes = shape.Atttributes.ToList(),
                        SawOption = shape.SawOption
                    };
                    model.Shapes.Add(childData);
                }

                var data = new ShapeData
                {
                    Id = shape.Id,
                    ParentId = shape.ParentId,
                    Name = $"{shape.Name}",
                    DisplayName = $"{shape.Name}",
                    Count = 0,
                    HasChild = childShapes.Count > 0,
                    ShapeAttributes = shape.Atttributes.ToList(),
                    SawOption = shape.SawOption,
                };
                model.Shapes.Add(data);
            }

            return model;
        }

        public CatalogModel PrepareShapeFilterModel(CatalogModel model)
        {
            //var shapes = _shapeService.GetShapes();
            var props = model.Products.Select(x => x.ProductCustomAttributes.FirstOrDefault(x => x.Key == "shapeId"));
            var shapeGroupList = props.GroupBy(x => Convert.ToInt32(x.Value));

            if (shapeGroupList.Count() > 0)
            {
                foreach (var shapeGroup in shapeGroupList)
                {
                    var filterItem = new ShapeFilterItem { ShapeId = shapeGroup.Key, ProductCount = shapeGroup.Count() };

                    // build shape
                    model.PagingFilteringContext.ShapeFilter.FilterItems.Add(filterItem);
                }
            }

            return model;
        }


        /// <summary>
        /// Key for SpecificationAttributeOptionFilter caching
        /// </summary>
        /// <remarks>
        /// {0} : list of specification attribute option IDs
        /// {1} : language id
        /// </remarks>
        public static CacheKey SpecsFilterModelKey => new CacheKey("Nop.pres.filter.specs-{0}-{1}", SpecsFilterPrefixCacheKey);
        public static string SpecsFilterPrefixCacheKey => "Nop.pres.filter.specs";
    }
}
