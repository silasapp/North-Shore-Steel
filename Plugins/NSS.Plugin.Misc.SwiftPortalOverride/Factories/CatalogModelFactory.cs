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
using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Nop.Web.Models.Catalog.CatalogPagingFilteringModel;

namespace Nop.Web.Factories
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
        public CatalogModel PrepareSwiftCatalogModel(IList<int> shapeIds, IList<int> specIds)
        {
            var model = new CatalogModel();
            var searchTerms = string.Empty;

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            // only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
            //we don't use "!string.IsNullOrEmpty(searchTerms)" in cases of "ProductSearchTermMinimumLength" set to 0 but searching by other parameters (e.g. category or price filter)
            var isSearchTermSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("q");

            if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("q", out var query))
                searchTerms = query.FirstOrDefault();


            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            var searchInDescriptions = false;

            //var searchInProductTags = false;
            //var searchInProductTags = searchInDescriptions;

            //products
            products = _productService.SearchProducts(out var filterableSpecificationAttributeOptionIds,
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

            model.Products = _productModelFactory.PrepareSwiftProductOverviewmodel(products).ToList();

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

            //specs
            model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(specIds,
                filterableSpecificationAttributeOptionIds?.ToArray(), _cacheKeyService,
                _specificationAttributeService, _localizationService, _webHelper, _workContext, _staticCacheManager);

            model.PagingFilteringContext.ShapeFilter = PrepareShapeFilterModel();

            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }

        public ShapeFilterModel PrepareShapeFilterModel()
        {
            var model = new ShapeFilterModel();

            var shapes = _shapeService.GetShapes();

            if(shapes.Count > 0)
                model.Shapes = shapes;

            return model;
        }
    }
}
