using Nop.Web.Factories;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using NSS.Plugin.Misc.SwiftPortalOverride.Controllers;
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
        public CatalogOverrideController.FilterParams FilterParams { get; internal set; }
        public List<ShapeData> Shapes { get; set; }

        #region Nested Class

        public class ShapeData
        {
            public int Id { get; set; }
            public int? ParentId { get; set; }
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public bool? HasChild { get; set; }
            public int Count { get; set; }
        }

        #endregion
    }
}
