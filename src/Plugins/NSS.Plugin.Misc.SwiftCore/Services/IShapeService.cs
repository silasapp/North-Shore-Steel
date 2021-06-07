using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface IShapeService
    {
        Task InsertShapesAsync(List<Shape> shapes);

        Task<IList<Shape>> GetShapesAsync(bool parentsOnly = false);

        Task<Shape> GetShapeByIdAsync(int id);

        Task DeleteShapesAsync();
    }
}
