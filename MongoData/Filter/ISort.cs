using System.Collections.Generic;

namespace MongoData.Filter
{
    public interface ISort
    {
        string Name { get; }
        int Direction { get; set; }
        IDictionary<string, object> ToDictionary();
    }
}
