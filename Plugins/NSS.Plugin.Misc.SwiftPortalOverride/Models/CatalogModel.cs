using Nop.Web.Factories;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial class CatalogModel : BaseNopEntityModel
    {
        public CatalogModel()
        {
            PagingFilteringContext = new CatalogPagingFilteringModel();
            Products = new List<ProductOverviewModel>();
        }
        public string Warning { get; set; }
        public bool NoResults { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }
        public IEnumerable<IGrouping<string, CatalogPagingFilteringModel.SpecificationFilterItem>> GroupedSpecificationAttributeName { get; internal set; }

        #region Nested Class

        #endregion
    }
}
