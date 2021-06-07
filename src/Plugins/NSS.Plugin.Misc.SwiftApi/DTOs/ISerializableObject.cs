using System;

namespace NSS.Plugin.Misc.SwiftApi.DTO
{
    public interface ISerializableObject
    {
        string GetPrimaryPropertyName();
        Type GetPrimaryPropertyType();
    }
}
