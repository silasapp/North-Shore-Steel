using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Infrastructure.Cache;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    /// <summary>
    /// Filtering and paging model for catalog
    /// </summary>
    public partial record CatalogPagingFilteringModel : BasePageableModel
    {
        #region Ctor

        /// <summary>
        /// Constructor
        /// </summary>
        public CatalogPagingFilteringModel()
        {
            AvailableSortOptions = new List<SelectListItem>();
            AvailableViewModes = new List<SelectListItem>();
            PageSizeOptions = new List<SelectListItem>();

            SpecificationFilter = new SpecificationFilterModel();
            ShapeFilter = new ShapeFilterModel();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Specification filter model
        /// </summary>
        public SpecificationFilterModel SpecificationFilter { get; set; }

        /// <summary>
        /// Shape filter model
        /// </summary>
        public ShapeFilterModel ShapeFilter { get; set; }

        /// <summary>
        /// Available sort options
        /// </summary>
        public IList<SelectListItem> AvailableSortOptions { get; set; }

        /// <summary>
        /// Available view mode options
        /// </summary>
        public IList<SelectListItem> AvailableViewModes { get; set; }

        /// <summary>
        /// Available page size options
        /// </summary>
        public IList<SelectListItem> PageSizeOptions { get; set; }

        #endregion

        #region Nested classes

        /// <summary>
        /// Specification filter model
        /// </summary>
        public partial record SpecificationFilterModel : BaseNopModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "specs";

            #endregion

            #region Ctor

            /// <summary>
            /// Ctor
            /// </summary>
            public SpecificationFilterModel()
            {
                AlreadyFilteredItems = new List<SpecificationFilterItem>();
                NotFilteredItems = new List<SpecificationFilterItem>();
                FilterItems = new List<SpecificationFilterItem>();
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Exclude query string parameters
            /// </summary>
            /// <param name="url">URL</param>
            /// <param name="webHelper">Web helper</param>
            /// <returns>New URL</returns>
            protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
            {
                //comma separated list of parameters to exclude
                const string excludedQueryStringParams = "pagenumber";
                var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var exclude in excludedQueryStringParamsSplitted)
                    url = webHelper.RemoveQueryString(url, exclude);
                return url;
            }

            #endregion

            #region Methods


            #endregion

            #region Properties

            /// <summary>
            /// Gets or sets a value indicating whether filtering is enabled
            /// </summary>
            public bool Enabled { get; set; }
            /// <summary>
            /// Already filtered items
            /// </summary>
            public IList<SpecificationFilterItem> AlreadyFilteredItems { get; set; }
            /// <summary>
            /// Not filtered yet items
            /// </summary>
            public IList<SpecificationFilterItem> NotFilteredItems { get; set; }
            public IList<SpecificationFilterItem> FilterItems { get; set; }
            /// <summary>
            /// URL of "remove filters" button
            /// </summary>
            public string RemoveFilterUrl { get; set; }

            #endregion
        }

        /// <summary>
        /// Specification filter item
        /// </summary>
        public partial record SpecificationFilterItem : BaseNopModel
        {
            /// <summary>
            /// Specification attribute id
            public int SpecificationAttributeOptionId { get; set; }
            /// </summary>
            /// <summary>
            /// Specification attribute name
            /// </summary>
            public string SpecificationAttributeName { get; set; }
            /// <summary>
            /// Specification attribute option name
            /// </summary>
            public string SpecificationAttributeOptionName { get; set; }

            public int ProductCount { get; set; }
        }


        /// <summary>
        /// Shape filter model
        /// </summary>
        public partial record ShapeFilterModel : BaseNopModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "shape";

            #endregion 

            #region Ctor

            /// <summary>
            /// Ctor
            /// </summary>
            public ShapeFilterModel()
            {
                //Shapes = new List<Shape>();
                FilterItems = new List<ShapeFilterItem>();
            }

            #endregion

            #region Utilities


            #endregion

            #region Methods

           
            #endregion

            #region Properties
       
            /// <summary>
            /// Filter items
            /// </summary>
            public IList<ShapeFilterItem> FilterItems { get; set; }
            /// <summary>
            /// URL of "remove filters" button
            /// </summary>
            public string RemoveFilterUrl { get; set; }

            #endregion
        }

        /// <summary>
        /// Specification filter item
        /// </summary>
        public partial record ShapeFilterItem : BaseNopModel
        {
            public ShapeFilterItem()
            {

            }

            public int ShapeId { get; set; }
            public int ProductCount { get; set; }
        }

        #endregion
    }
}
