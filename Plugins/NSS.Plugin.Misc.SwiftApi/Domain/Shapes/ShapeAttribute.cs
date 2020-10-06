using Nop.Core;

namespace NSS.Plugin.Misc.SwiftApi.Domain.Shapes
{
    public class ShapeAttribute : BaseEntity
    {
        /// <summary>
        /// Gets or sets the code
        /// </summary>
        public string Code { get; set; }        
        
        /// <summary>
        /// Gets or sets the sort
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Shape identifier
        /// </summary>
        public int ShapeId { get; set; }

        /// <summary>
        /// Gets or sets the Shape identifier
        /// </summary>
        public Shape Shape { get; set; }
    }
}
