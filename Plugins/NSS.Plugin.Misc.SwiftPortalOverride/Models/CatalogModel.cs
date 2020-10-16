using Nop.Web.Factories;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial class CatalogModel : BaseNopEntityModel
    {
        public CatalogModel()
        {
            Shapes = new List<Shape>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            Products = new List<ProductOverviewModel>();
        }
        public string Warning { get; set; }
        public bool NoResults { get; set; }
        public IList<Shape> Shapes { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }

        #region Nested Class

        #endregion
    }
}
