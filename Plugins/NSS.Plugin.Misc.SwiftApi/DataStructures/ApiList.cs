using System.Collections.Generic;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftApi.DataStructures
{
    public class ApiList<T> : List<T>
    {
        public ApiList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            PageSize = pageSize;
            PageIndex = pageIndex;
            AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
        }

        public int PageIndex { get; }
        public int PageSize { get; }
    }
}
