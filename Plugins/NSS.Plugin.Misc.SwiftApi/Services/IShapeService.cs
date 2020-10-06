using NSS.Plugin.Misc.SwiftApi.Domain.Shapes;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftApi.Services
{
    public interface IShapeService
    {
        void InsertShapes(List<Shape> shapes);

        IList<Shape> GetShapes();

        void DeleteShapes();
    }
}
