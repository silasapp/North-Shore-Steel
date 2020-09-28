using Nop.Plugin.Swift.Api.Domain.Shapes;
using System.Collections.Generic;

namespace Nop.Plugin.Swift.Api.Services
{
    public interface IShapeService
    {
        void InsertShapes(List<Shape> shapes);

        IList<Shape> GetShapes();

        void DeleteShapes();
    }
}
