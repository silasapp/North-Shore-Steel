using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using Nop.Web.Infrastructure.Cache;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        private readonly ICacheKeyService _cacheKeyService;
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
            ICacheKeyService cacheKeyService,
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
            _cacheKeyService = cacheKeyService;
            _shapeService = shapeService;
        }

        /// <summary>
        /// Prepare search model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Catalog paging filtering command</param>
        /// <returns>Search model</returns>
        public CatalogModel PrepareSwiftCatalogModel(IList<int> shapeIds, IList<int> specIds, string searchKeyword = null, bool isPageLoad = false)
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

            Stopwatch searchTimer = Stopwatch.StartNew();

            products = _productService.SearchProducts(out filterableSpecificationAttributeOptionIds,
                true,
                categoryIds: shapeIds,
                storeId: _storeContext.CurrentStore.Id,
                //visibleIndividuallyOnly: false,
                featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                keywords: searchTerms,
                filteredSpecs: specIds
                //orderBy: (ProductSortingEnum)command.OrderBy,
                //pageIndex: command.PageNumber - 1,
                //pageSize: command.PageSize
                );

            searchTimer.Stop();
            TimeSpan timespan = searchTimer.Elapsed;
            Console.WriteLine("search timer (ms)", timespan.Milliseconds.ToString());


            Stopwatch productModelTimer = Stopwatch.StartNew();
            
            model.Products = _productModelFactory.PrepareSwiftProductOverviewmodel(products).OrderBy(o => o.Sku).ToList();
            productModelTimer.Stop();
            Console.WriteLine("prod model timer (ms)", productModelTimer.Elapsed.Milliseconds.ToString());

            model.NoResults = !model.Products.Any();

            //search term statistics
            if (!string.IsNullOrEmpty(searchTerms))
            {
                var searchTerm =
                    _searchTermService.GetSearchTermByKeyword(searchTerms, _storeContext.CurrentStore.Id);
                if (searchTerm != null)
                {
                    searchTerm.Count++;
                    _searchTermService.UpdateSearchTerm(searchTerm);
                }
                else
                {
                    searchTerm = new SearchTerm
                    {
                        Keyword = searchTerms,
                        StoreId = _storeContext.CurrentStore.Id,
                        Count = 1
                    };
                    _searchTermService.InsertSearchTerm(searchTerm);
                }

                //event
                _eventPublisher.Publish(new ProductSearchEvent
                {
                    SearchTerm = searchTerms,
                    SearchInDescriptions = searchInDescriptions,
                    CategoryIds = shapeIds,
                    ManufacturerId = 0,
                    WorkingLanguageId = _workContext.WorkingLanguage.Id,
                    VendorId = 0
                });
            }

            //specs
            Stopwatch specFilterTimer = Stopwatch.StartNew();
            if(!isPageLoad)
                PrepareSpecsFilters(specIds,
                    filterableSpecificationAttributeOptionIds?.ToArray(), _cacheKeyService,
                    _specificationAttributeService, _localizationService, _webHelper, _workContext, _staticCacheManager, ref model, shapeIds);
            else
                PrepareSpecsFilters(specIds,
                    filterableSpecificationAttributeOptionIds?.ToArray(), _cacheKeyService,
                    _specificationAttributeService, _localizationService, _webHelper, _workContext, _staticCacheManager, ref model);
            specFilterTimer.Stop();
            Console.WriteLine("spec timer (ms)", specFilterTimer.Elapsed.Milliseconds.ToString());

            Stopwatch shapeFilterTimer = Stopwatch.StartNew();
            
            PrepareShapeFilterModel(ref model);
            if (isPageLoad)
                PrepareShapeData(ref model);
            shapeFilterTimer.Stop();
            Console.WriteLine("shape timer (ms)", shapeFilterTimer.Elapsed.Milliseconds.ToString());

            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }

        public void PrepareSpecsFilters(IList<int> alreadyFilteredSpecOptionIds,
            int[] filterableSpecificationAttributeOptionIds,
            ICacheKeyService cacheKeyService,
            ISpecificationAttributeService specificationAttributeService, ILocalizationService localizationService,
            IWebHelper webHelper, IWorkContext workContext, IStaticCacheManager staticCacheManager, ref CatalogModel model, IList<int> shapeIds)
        {
            model.PagingFilteringContext.SpecificationFilter.Enabled = false;
            var cacheKey = cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.SpecsFilterModelKey, filterableSpecificationAttributeOptionIds, workContext.WorkingLanguage);

            var allOptions = specificationAttributeService.GetSpecificationAttributeOptionsByIds(filterableSpecificationAttributeOptionIds);
            var allFilters = staticCacheManager.Get(cacheKey, () => allOptions.Select(sao =>
            {
                var specAttribute = specificationAttributeService.GetSpecificationAttributeById(sao.SpecificationAttributeId);

                return new SpecificationAttributeOptionFilter
                {
                    SpecificationAttributeId = specAttribute.Id,
                    SpecificationAttributeName = localizationService.GetLocalized(specAttribute, x => x.Name, workContext.WorkingLanguage.Id),
                    SpecificationAttributeDisplayOrder = specAttribute.DisplayOrder,
                    SpecificationAttributeOptionId = sao.Id,
                    SpecificationAttributeOptionName = localizationService.GetLocalized(sao, x => x.Name, workContext.WorkingLanguage.Id),
                    SpecificationAttributeOptionColorRgb = sao.ColorSquaresRgb,
                    SpecificationAttributeOptionDisplayOrder = sao.DisplayOrder
                };
            }).ToList());

            if (!allFilters.Any())
                return;

            //prepare the model properties
            model.PagingFilteringContext.SpecificationFilter.Enabled = true;

            //var defaultSpecFilters = new List<string> { SwiftCore.Helpers.Constants.CoatingFieldAttribute, SwiftCore.Helpers.Constants.MetalFieldAttribute };

            var specFilters = allFilters.Select(af =>
            {
                var prodSpecs = _specificationAttributeService.GetProductSpecificationAttributes(specificationAttributeOptionId: af.SpecificationAttributeOptionId);

                return new SpecificationFilterItem
                {
                    SpecificationAttributeName = af.SpecificationAttributeName,
                    SpecificationAttributeOptionId = af.SpecificationAttributeOptionId,
                    SpecificationAttributeOptionName = af.SpecificationAttributeOptionName,
                    ProductCount = prodSpecs.GroupBy(ps => ps.SpecificationAttributeOptionId).Count()
                };

            });

            model.PagingFilteringContext.SpecificationFilter.FilterItems = new List<SpecificationFilterItem>(specFilters);

        }

        public void PrepareSpecsFilters(IList<int> alreadyFilteredSpecOptionIds,
            int[] filterableSpecificationAttributeOptionIds,
            ICacheKeyService cacheKeyService,
            ISpecificationAttributeService specificationAttributeService, ILocalizationService localizationService,
            IWebHelper webHelper, IWorkContext workContext, IStaticCacheManager staticCacheManager, ref CatalogModel model)
        {
            model.PagingFilteringContext.SpecificationFilter.Enabled = false;
            var cacheKey = cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.SpecsFilterModelKey, filterableSpecificationAttributeOptionIds, workContext.WorkingLanguage);

            var allOptions = specificationAttributeService.GetSpecificationAttributeOptionsByIds(filterableSpecificationAttributeOptionIds);
            var allFilters = staticCacheManager.Get(cacheKey, () => allOptions.Select(sao =>
            {
                var specAttribute = specificationAttributeService.GetSpecificationAttributeById(sao.SpecificationAttributeId);

                return new SpecificationAttributeOptionFilter
                {
                    SpecificationAttributeId = specAttribute.Id,
                    SpecificationAttributeName = localizationService.GetLocalized(specAttribute, x => x.Name, workContext.WorkingLanguage.Id),
                    SpecificationAttributeDisplayOrder = specAttribute.DisplayOrder,
                    SpecificationAttributeOptionId = sao.Id,
                    SpecificationAttributeOptionName = localizationService.GetLocalized(sao, x => x.Name, workContext.WorkingLanguage.Id),
                    SpecificationAttributeOptionColorRgb = sao.ColorSquaresRgb,
                    SpecificationAttributeOptionDisplayOrder = sao.DisplayOrder
                };
            }).ToList());

            model.AllFilters = new List<SpecificationAttributeOptionFilter>(allFilters);
        }

        public void PrepareShapeData(ref CatalogModel model)
        {
            var shapes = _shapeService.GetShapes().OrderBy(s => s.Order);

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
                };
                model.Shapes.Add(data);
            }
        }

        public void PrepareShapeFilterModel(ref CatalogModel model)
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
        }
    }
}
