using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftApi.Factories
{
    public interface IFactory<T>
    {
        Task<T> InitializeAsync();
    }
}
