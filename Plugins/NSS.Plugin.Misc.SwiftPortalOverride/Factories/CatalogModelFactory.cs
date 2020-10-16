using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Web.Factories
{
    public partial class CatalogModelFactory : ICatalogModelFactory
    {

        private readonly CatalogSettings _catalogSettings;
        private readonly DisplayDefaultMenuItemSettings _displayDefaultMenuItemSettings;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly ISearchTermService _searchTermService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        public CatalogModelFactory(
            CatalogSettings catalogSettings,
            DisplayDefaultMenuItemSettings displayDefaultMenuItemSettings,
            //ForumSettings forumSettings,
            //IActionContextAccessor actionContextAccessor,
            //ICacheKeyService cacheKeyService,
            ICategoryService categoryService,
            ICategoryTemplateService categoryTemplateService,
            ICurrencyService currencyService,
            //ICustomerService customerService,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IManufacturerTemplateService manufacturerTemplateService,
            //IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            ISearchTermService searchTermService,
            ISpecificationAttributeService specificationAttributeService,
            //IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            //ITopicService topicService,
            //IUrlHelperFactory urlHelperFactory,
            //IUrlRecordService urlRecordService,
            //IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext
            )
        {
            _catalogSettings = catalogSettings;
            _displayDefaultMenuItemSettings = displayDefaultMenuItemSettings;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _eventPublisher = eventPublisher;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _searchTermService = searchTermService;
            _specificationAttributeService = specificationAttributeService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
        }
        public void PrepareSwiftCatalogModel(IList<int> shapeIds, IList<int> specIds)
        {
            var model = new SearchModel();

            var products = _productService.SearchProducts(out var filterableSpecificationAttributeOptionIds, categoryIds: shapeIds, filteredSpecs: specIds);

        }

        /// <summary>
        /// Prepare search model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Catalog paging filtering command</param>
        /// <returns>Search model</returns>
        public CatalogModel PrepareSwiftCatalogModel(CatalogPagingFilteringModel command)
        {
            var model = new CatalogModel();
            var searchTerms = string.Empty;

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            // only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
            //we don't use "!string.IsNullOrEmpty(searchTerms)" in cases of "ProductSearchTermMinimumLength" set to 0 but searching by other parameters (e.g. category or price filter)
            var isSearchTermSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("q");

            if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("q", out var query))
                searchTerms = query.FirstOrDefault();

            var shapeIds = new List<int>();
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            var searchInDescriptions = false;

            //var searchInProductTags = false;
            //var searchInProductTags = searchInDescriptions;

            //products
            products = _productService.SearchProducts(
                categoryIds: shapeIds,
                //manufacturerId: manufacturerId,
                //storeId: _storeContext.CurrentStore.Id,
                //visibleIndividuallyOnly: false,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                keywords: searchTerms,
                //searchDescriptions: searchInDescriptions,
                //searchProductTags: searchInProductTags,
                languageId: _workContext.WorkingLanguage.Id,
                orderBy: (ProductSortingEnum)command.OrderBy
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

            model.PagingFilteringContext.LoadPagedList(products);
            return model;
        }
    }
}
