using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoData;

namespace MongoData.Aggregation
{
    public abstract class AggregationReportDao<T>
    {

        private MongoContext<T> context;


        protected AggregationReportDao()
        {
            context = new MongoContext<T>();
        }

        protected AggregationReportDao(string collectionName)
        {
            context = new MongoContext<T>(collectionName);
        }

        protected MongoContext<T> Context
        {
            get { return context; }
        }


        private static readonly BsonDocument byDayGroupId = new BsonDocument { { "Year", "$Year" }, { "DayOfYear", "$DayOfYear" } };

        private static readonly BsonDocument byMinuteGroupId = new BsonDocument
                                                                   { { "Year", "$Year" }, { "DayOfYear", "$DayOfYear" },
                                                                     {"Hour", "$Hour"}, {"Minute",  "$Minute"}};

        protected static readonly BsonDocument Count = new BsonDocument { { "Count", new BsonDocument { { "$sum", 1 } } } };
        protected static readonly BsonDocument byDayGroup = new BsonDocument { { "_id", byDayGroupId }, Count };
        protected static readonly BsonDocument byMinuteGroup = new BsonDocument { { "_id", byMinuteGroupId }, Count };
        protected static readonly BsonDocument localDateSort = new BsonDocument { 
                                                                     { "_id.Year", 1 }, { "_id.DayOfYear", 1 },
                                                                     {"_id.Hour", 1}, {"_id.Minute",  1}};

        private int GetBsonValue(BsonValue value)
        {
            if (value.BsonType == BsonType.Int32)
            {
                return value.AsInt32;
            }
            return (int)value.AsDouble;
        }

        private Tuple<DateTime, Int32> fromYearDay(BsonDocument b)
        {
            BsonDocument id = b["_id"].AsBsonDocument;
            return new Tuple<DateTime, Int32>(new DateTime(GetBsonValue(id["Year"]), 1, 1).AddDays(GetBsonValue(id["DayOfYear"]) - 1), GetBsonValue(b["Count"]));
        }

        private Tuple<DateTime, Int32> fromYearDayHourMinute(BsonDocument b)
        {
            var r = fromYearDay(b);
            BsonDocument id = b["_id"].AsBsonDocument;
            r.Item1.AddHours(GetBsonValue(id["Hour"])).AddMinutes(GetBsonValue(id["Minute"]));
            return r;
        }

        protected IEnumerable<Tuple<DateTime, Int32>> GraphByDays(IEnumerable<Object> data)
        {
            return data.Where(d => d.ToString().Contains("Year")).Select(x => fromYearDay(x as BsonDocument));
        }

        protected IEnumerable<Tuple<DateTime, Int32>> GraphByMinutes(IEnumerable<Object> data)
        {
            return data.Where(d => d.ToString().Contains("Year")).Select(x => fromYearDayHourMinute(x as BsonDocument));
        }

        protected AggregateResult RunAggreation(params BsonDocument[] ops)
        {
            return context.Collection.Aggregate(ops);
        }

        protected IList<T> RunAggreationToList(params BsonDocument[] ops)
        {
            var result = context.Collection.Aggregate(ops);
            return result.Response["result"].AsBsonArray.Select(x => BsonSerializer.Deserialize<T>(x as BsonDocument)).ToList();
        }

        protected IList<U> RunAggreationToList<U>(params BsonDocument[] opsa)
        {
            var result = context.Collection.Aggregate(opsa);
            Debug.WriteLine(result.Response["result"].AsBsonArray);
            return result.Response["result"].AsBsonArray.Select(x => BsonSerializer.Deserialize<U>(x as BsonDocument)).ToList();
        }

        protected IEnumerable<Object> AggregationResultToList(AggregateResult aggregateResult)
        {
            return aggregateResult.ResultDocuments;
        }

 

        public CommandResult Aggregate(AggregationBuilder b)
        {
            var result = context.Collection.Database.RunCommand(b.Build());
            return result;

        }

        protected static BsonDocument FieldDocument(string prefix, params string[] fields)
        {
            var r = new BsonDocument();

            foreach (var f in fields)
            {
                r.Add(f, prefix + f);
            }
            return r;
        }
    }
}
