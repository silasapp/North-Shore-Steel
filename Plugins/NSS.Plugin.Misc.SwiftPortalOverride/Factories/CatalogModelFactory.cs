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
using System.Linq;
using System.Text;
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
        public CatalogModel PrepareSwiftCatalogModel(IList<int> shapeIds, IList<int> specIds, string searchKeyword = null)
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

            if (shapeIds.Count > 0 || specIds.Count > 0)
            {
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
            }

            model.Products = _productModelFactory.PrepareSwiftProductOverviewmodel(products).OrderBy(o => o.Sku).ToList();

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
            PrepareSpecsFilters(specIds,
                filterableSpecificationAttributeOptionIds?.ToArray(), _cacheKeyService,
                _specificationAttributeService, _localizationService, _webHelper, _workContext, _staticCacheManager, ref model, shapeIds);

            PrepareShapeFilterModel(ref model);

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

            var defaultSpecFilters = new List<string> { SwiftCore.Helpers.Constants.CoatingFieldAttribute, SwiftCore.Helpers.Constants.MetalFieldAttribute };

            var specs = model.Products.Select(x => x.SpecificationAttributeModels).SelectMany(specs => specs);

            // not plate individual shape
            if (!shapeIds.Any(x => x.ToString().StartsWith("13")))
                specs = specs.Where(x => defaultSpecFilters.Contains(x.SpecificationAttributeName));

            var specGroup = specs.GroupBy(x => x.SpecificationAttributeOptionId);

            var specList = new List<SpecificationFilterItem>();

            foreach (var spec in specGroup)
            {
                var activeSpec = spec.FirstOrDefault(z => z.SpecificationAttributeOptionId == spec.Key);

                specList.Add(new SpecificationFilterItem
                {
                    SpecificationAttributeName = activeSpec?.SpecificationAttributeName,
                    SpecificationAttributeOptionId = spec.Key,
                    SpecificationAttributeOptionColorRgb = "",
                    SpecificationAttributeOptionName = activeSpec?.SpecificationAttributeOptionName,
                    ProductCount = spec.Count()
                });
            }

            model.PagingFilteringContext.SpecificationFilter.FilterItems = new List<SpecificationFilterItem>(specList);

        }

        public void PrepareShapeFilterModel(ref CatalogModel model)
        {
            //var shapes = _shapeService.GetShapes();
            var productGroup = model.Products.GroupBy(x => x.Shape.Id);

            if(productGroup.Count() > 0)
            {
                foreach (var shapeGroup in productGroup)
                {
                    var filterItem = new ShapeFilterItem { Shape = shapeGroup.FirstOrDefault(x => x.Shape.Id == shapeGroup.Key).Shape, ProductCount = shapeGroup.Count() };

                    if (filterItem.Shape.Parent != null)
                    {
                        if (model.PagingFilteringContext.ShapeFilter.FilterItems.Any(x => x.Shape.Id == filterItem.Shape.ParentId))
                        {
                            model.PagingFilteringContext.ShapeFilter.FilterItems.FirstOrDefault(x => x.Shape.Id == filterItem.Shape.ParentId).ProductCount += filterItem.ProductCount;
                        }
                        else
                        {
                            // build parent
                            model.PagingFilteringContext.ShapeFilter.FilterItems.Add(new ShapeFilterItem { Shape = filterItem.Shape.Parent, ProductCount = shapeGroup.Count() });
                        }
                    }

                    if (model.PagingFilteringContext.ShapeFilter.FilterItems.Any(x => x.Shape.Id == filterItem.Shape.Id))
                    {
                        model.PagingFilteringContext.ShapeFilter.FilterItems.FirstOrDefault(x => x.Shape.Id == filterItem.Shape.Id).ProductCount += filterItem.ProductCount;
                    }
                    else
                    {
                        // build shape
                        model.PagingFilteringContext.ShapeFilter.FilterItems.Add(filterItem);
                    }
                }
            }
            else
            {
                model.PagingFilteringContext.ShapeFilter.FilterItems = new List<ShapeFilterItem>(_shapeService.GetShapes(parentsOnly: true).Select(shape => new ShapeFilterItem { Shape = shape, ProductCount = 0 }));
            }
        }
    }
}
