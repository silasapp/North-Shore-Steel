using Nop.Core;
using System.Collections.Generic;

namespace Nop.Plugin.Swift.Api.Domain.Shapes
{
    public class Shape : BaseEntity
    {
        /// <summary>
        /// Gets or sets the Shape name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the saw option
        /// </summary>
        public bool SawOption { get; set; }

        /// <summary>
        /// Gets or sets the order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the attributes
        /// </summary>
        public ICollection<ShapeAttribute> Atttributes { get; set; }

        /// <summary>
        /// Gets or sets the parent identifier
        /// </summary>
        public int? ParentId { get; set; }

        public virtual Shape Parent { get; set; }

        public virtual ICollection<Shape> SubCategories { get; set; }
    }
}
