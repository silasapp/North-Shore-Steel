using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Factories
{
    public class SwiftCatalogModelFactory
    {
        private readonly IProductService _productService;

        public SwiftCatalogModelFactory(IProductService productService)
        {
            _productService = productService;
        }

        public void PrepareSwiftCatalogModel(IList<int> shapeIds, IList<int> specIds)
        {
            _productService.SearchProducts(out var specificationOptionIds, true, categoryIds: shapeIds, filteredSpecs: specIds);
        }
    }
}
