using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoData.Filter;

namespace MongoData.Aggregation
{
    public class AggregationBuilder
    {
        private BsonDocument step;
        private readonly string collection;
        private readonly List<BsonDocument> pipeline = new List<BsonDocument>();

        public AggregationBuilder(string col)
        {
            collection = col;
        }

        /*
            $project => Select()
            $limit => Take()
            $group => GroupBy()
            $sort => OrderBy(),OrderByDescending()
            $unwind => SelectMany()
            $skip => Skip()
            $match => Where()
        */

        public AggregationBuilder Add(AggregationBuilder builder)
        {
            this.pipeline.AddRange(builder.pipeline);
            return this;
        }

        public CommandDocument CommandDocument { get; set; }

        public BsonDocument[] Pipeline
        {
            get { return pipeline.ToArray(); }
        }

        public CommandDocument Build()
        {
            var pipe = new BsonArray(pipeline.ToArray());
            this.CommandDocument = new CommandDocument
                                       {
                                           {"aggregate", collection},
                                           {"pipeline", pipe}
                                       };
            return CommandDocument;
        }

        public AggregationBuilder Group(string key, string field)
        {
            var newPipe = new BsonDocument { { "$group", new BsonDocument { { key, field } } } };
            this.step = new BsonDocument { this.step, newPipe };
            pipeline.Add(step);
            return this;
        }

        public AggregationBuilder Group(BsonDocument doc)
        {
            pipeline.Add(new BsonDocument { {"$group", doc } });
            return this;
        }

        public AggregationBuilder Group(string key, BsonDocument doc)
        {
            var newPipe = new BsonDocument { { "$group", new BsonDocument { { key, doc } } } };
            this.step = new BsonDocument { this.step, newPipe };
            pipeline.Add(step);
            return this;
        }

        public AggregationBuilder Group(string key, IDictionary<string, object> dict)
        {
            return Group(key, new BsonDocument { dict });
        }

        public AggregationBuilder Sum(string key, string field)
        {
            return Aggregate(key, "$sum", field);
        }

        public AggregationBuilder First(string key, string field)
        {
            return Aggregate(key, "$first", field);
        }

        private AggregationBuilder Aggregate(string key, string func, string field)
        {
            var newPipe = new BsonDocument { { key, new BsonDocument { { func, field } } } };
            this.step["$group"].AsBsonDocument.Merge(newPipe);
            return this;
        }

        public AggregationBuilder Unwind(string key)
        {
            var newPipe = new BsonDocument { { "$unwind", key } };
            pipeline.Add(newPipe);
            return this;
        }

        public AggregationBuilder Match(IMongoQuery query)
        {
            if(query != null && query != Query.Null)
            {
                var newPipe = new BsonDocument { { "$match", query.ToBsonDocument() } };
                pipeline.Add(newPipe);
            }

            return this;
        }

        public AggregationBuilder Project(IDictionary<string, object> dict)
        {
            var newPipe = new BsonDocument { { "$project", new BsonDocument { dict } } };
            pipeline.Add(newPipe);
            return this;
        }

        public AggregationBuilder Project(params string[] fields)
        {
            return Project(fields.ToDictionary(field => field, field => (Object)1));
        }


        public AggregationBuilder Project(BsonDocument doc)
        {
            pipeline.Add(new BsonDocument { { "$project", doc } });
            return this;
        }


        public AggregationBuilder Sort(BsonDocument sort)
        {
            var newPipe = new BsonDocument { { "$sort", sort } };
            //  this.step = new BsonDocument { this.step, newPipe };
            pipeline.Add(newPipe);
            return this;
        }

        public AggregationBuilder Sort(IMongoSortBy sortBy)
        {
            var newPipe = new BsonDocument { { "$sort", sortBy.ToBsonDocument() } };
          //  this.step = new BsonDocument { this.step, newPipe };
            pipeline.Add(newPipe);
            return this;
        }

        public AggregationBuilder Sort(ISort sort)
        {
            if (sort != null)
            {
                pipeline.Add(new BsonDocument {{"$sort", new BsonDocument(sort.ToDictionary())}});
            }
            return this;
        }

        public AggregationBuilder Skip(int count)
        {
            if (count == 0) return this;
            var newPipe = new BsonDocument { { "$skip", count } };
            pipeline.Add(newPipe);
            return this;
        }

        public AggregationBuilder Limit(int count)
        {
            if (count == 0) return this;
            var newPipe = new BsonDocument { { "$limit", count } };
            pipeline.Add(newPipe);
            return this;
        }
    }
}
