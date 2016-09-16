using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace MongoData.Filter
{
    public interface IFilterBuilder
    {
        IFilterBuilder Add(string name, Object value);
        IFilterBuilder StringFilter(String name, String value);
        IFilterBuilder DateTimeFilter(String name, DateTime value, ComparisonType comparison = ComparisonType.NONE);
        IFilterBuilder ObjectIdFilter(String name, ObjectId value, ComparisonType comparison = ComparisonType.EQ);
        IFilterBuilder IntFilter(String name, int value, ComparisonType comparison = ComparisonType.EQ);
        IEnumerable<IFilter> Build();
        int Count { get; }
    }

}
