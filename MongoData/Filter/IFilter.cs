using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoData;

namespace MongoData.Filter
{

    public enum ComparisonType
    {
        NONE, LT, LTE, EQ, GTE, GT
    }

    public interface IFilterVisitor
    {
        IFilterVisitor Visit(IIntFilter filter);
        IFilterVisitor Visit(IStringFilter filter);
        IFilterVisitor Visit(IDateTimeFilter filter);
        IFilterVisitor Visit(IObjectIdFilter filter);
        IFilterVisitor Visit(IEnumerable<IFilter> filters);
    }

    public interface IFilter
    {
        String Name { get; }
        ComparisonType Comparison { get; }
        void AcceptVisitor(IFilterVisitor visitor);
        IFilter Make(Object o);
    }

    public interface IFilter<out T> : IFilter
    {
        T Value { get; }
    }

    public interface IIntFilter : IFilter<int>
    {
    }

    public interface IStringFilter : IFilter<String>
    {
    }

    public interface IDateTimeFilter : IFilter<long>
    {
    }

    public interface IObjectIdFilter : IFilter<ObjectId>
    {
    }

}