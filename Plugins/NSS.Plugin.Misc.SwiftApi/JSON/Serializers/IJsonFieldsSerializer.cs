using NSS.Plugin.Misc.SwiftApi.DTO;

namespace NSS.Plugin.Misc.SwiftApi.JSON.Serializers
{
    public interface IJsonFieldsSerializer
    {
        string Serialize(ISerializableObject objectToSerialize, string fields);
    }
}
