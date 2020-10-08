using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface IShapeService
    {
        void InsertShapes(List<Shape> shapes);

        IList<Shape> GetShapes();
        Shape GetShapeById(int id);

        void DeleteShapes();
    }
}
