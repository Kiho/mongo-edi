using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using MongoDB.Bson;
﻿using MongoDB.Bson.Serialization;
﻿using MongoDB.Driver;
using MongoDB.Driver.Builders;
using OpusOneCorp.Data.Core;
﻿using OpusOneCorp.Data.Core.Domain;
using OpusOneCorp.Data.Core.Domain.Graphs;
using OpusOneCorp.Data.Core.Graphs;


namespace OpusOneCorp.Data.Mongo.Aggregation
{
    public class ViralDNAReportDao :AggregationReportDao<User>, IViralDNAReportDao
    {
        static  ViralDNAReportDao()
        {
            MoneySerializer.RegisterMoney();
        }

        
        private static readonly Dictionary<string, object> registrationReportFields = new Dictionary<string, object>
                                                                                          {
                                                                                              {"RegisteredOn", 1},
                                                                                              {"FirstName", 1},
                                                                                              {"LastName", 1},
                                                                                              {"Email", 1},
                                                                                              {
                                                                                                  "OptionSubscribeNewsletter"
                                                                                                  , 1
                                                                                                  },
                                                                                              {
                                                                                                  "OptionSubscribeSweepstakes"
                                                                                                  , 1
                                                                                                  }
                                                                                          };


        //private static Dictionary<string, object> winnersReportFields = new Dictionary<string, object>
        //                                                                    {
        //                                                                        {"RegisteredOn", 1},
        //                                                                        {"FirstName", 1},
        //                                                                        {"LastName", 1},
        //                                                                        {"Email", 1},
        //                                                                        {"Prizes", 1},
        //                                                                        {"UserEvent", 1}
        //                                                                    };
        private static readonly Dictionary<string, object> winnersReportFields = new Dictionary<string, object>
                                                                            {
                                                                                {"RegisteredOn", 1},
                                                                                {"FirstName", 1},
                                                                                {"LastName", 1},
                                                                                {"Email", 1},
                                                                                {"PrizeCount", 1}, 
                                                                                {"Prizes._v.CampaignPrizeDescription", 1},
                                                                                {"Prizes._v.PrizeDate", 1}
                                                                            };

        private static readonly Dictionary<string, object> winReportFields = new Dictionary<string, object>
                                                                            {
                                                                                {"RegisteredOn", 1},
                                                                                {"FirstName", 1},
                                                                                {"LastName", 1},
                                                                                {"Email", 1},
                                                                                {"Prizes._t", 1},
                                                                                {"Prizes._v.CampaignPrizeDescription", 1},
                                                                                {"Prizes._v.PrizeDate", 1}
                                                                             
                                                                            };

        private static readonly Dictionary<string, object> prizeReportFields = new Dictionary<string, object>
                                                                            {
                                                                                {"Description", 1},
                                                                                {"Awarded", 1},
                                                                                {"Quantity", 1},
                                                                                 {"CampaignId", 1}
                                                                            };

        private static readonly Dictionary<string, object> registrationGraphProjection = new Dictionary<string, object>
                          {{"Year", "$RegisteredOnLocalDate.Year"}, {"DayOfYear", "$RegisteredOnLocalDate.DayOfYear"},
                               {"Hour", "$RegisteredOnLocalDate.Hour"}, {"Minute",  "$RegisteredOnLocalDate.Minute"}
                          };


        private static readonly Dictionary<string, object> winGraphProjection = new Dictionary<string, object>
                          {{"Year", "$Prizes._v.PrizeLocalDate.Year"}, {"DayOfYear", "$Prizes._v.PrizeLocalDate.DayOfYear"},
                               {"Hour", "$Prizes._v.PrizeLocalDate.Hour"}, {"Minute",  "$Prizes._v.PrizeLocalDate.Minute"}
                          };


     

        private static readonly Dictionary<string, object> registrationGraphFields = new Dictionary<string, object> {{"RegisteredOnLocalDate",1}};

       

        protected IEnumerable<Object> RunCommand(CommandDocument cmd)
        {
            var context = new MongoContext<User>();
            var results = context.Collection.Database.RunCommand(cmd);
            return results.Response["result"].AsBsonArray;
        }

        public IEnumerable<Object> RegistrationReport(IFilterBuilder userFilter, ISort sort, int skip, int limit)
        {
            IMongoQuery query =null;

            if(userFilter != null)
            {
                query = new MongoFilterVisitor(userFilter.Build()).Build();
            }
     
            //var aggQuery = new AggregationBuilder("UserXUserEvent")
            //    .Match(query)
            //    .Project(registrationReportFields)
            //    .Unwind("$UserEvents")
            //    .Sort(sort)
            //    .Skip(skip)
            //    .Limit(limit).Pipeline;

            var aggQuery = new AggregationBuilder("User")
                .Match(query)
                .Project(registrationReportFields)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit).Pipeline;

            // Debug.WriteLine(aggQuery);
            //return RunCommand(aggQuery);
            return AggregationResultToList(RunAggreation(aggQuery));

        }


        private BsonDocument FieldDocument( String prefix, IEnumerable<String> fields)
        {
            var r = new BsonDocument();

            foreach (var f in fields)
            {
                r.Add(f, prefix + f);
            }

            return r;
        }

        protected AggregationBuilder WinsGraphBase( IFilterBuilder userFilter, IFilterBuilder prizeFilter)
        {
            var baseQuery = Query.And(
                Query.Exists("Prizes._v"), 
                Query.Not(Query.Size("Prizes._v", 0)),
                Query.NE("Hidden", true));

            var userQuery = new MongoFilterVisitor(baseQuery, userFilter.Build()).Build();

            var prizeQuery = new MongoFilterVisitor(prizeFilter.Build()).Build();

            var aggBuilder= new AggregationBuilder("User")
                .Match(userQuery)
                .Project(new Dictionary<string, object> { { "CampaignId", 1 }, { "Prizes", 1 } })
                .Unwind("$Prizes._v")
                .Match(prizeQuery);
            return aggBuilder;
        }

        /*
         * Count Prizes won, grouped by Prize description
         */

        protected AggregationBuilder PrizesGraphBase(IFilterBuilder userFilter, IFilterBuilder prizeFilter)
        {
            var group = new BsonDocument {{"_id", "$Prizes._v.PrizeId"}, {"Awarded", new BsonDocument("$sum", 1)}};
          // var groupCount = new BsonDocument { { "_id", "$Count" }, { "Count", new BsonDocument("$sum", 1) } };
            var aggQuery = WinsGraphBase( userFilter, prizeFilter)
                .Group(group)
                .Project(new Dictionary<string, object> { { "_id", 1 }, { "Awarded", 1 }, })
              ;

            return aggQuery;
            

        }

        protected IEnumerable<Object> PrizesGraph(IFilterBuilder userFilter, IFilterBuilder prizeFilter)
        {
            return AggregationResultToList(
                RunAggreation(
                    PrizesGraphBase(userFilter, prizeFilter).Pipeline));
        }


        protected IEnumerable<Object> PrizesData(IFilterBuilder userFilter, IFilterBuilder prizeFilter, int skip, int limit, ISort sort)
        {
            return AggregationResultToList(
                RunAggreation(
                    PrizesGraphBase(userFilter, prizeFilter).Sort(sort).Skip(skip).Limit(limit).Pipeline));
        }

        /*
         * Count of winners, grouped by number of prizes won within filtered date range
         */
        public IEnumerable<Object> WinnersGraph(IFilterBuilder userFilter, IFilterBuilder prizeFilter)
        {
            var group = new BsonDocument { { "_id", "$_id" }, { "Count", new BsonDocument("$sum", 1) } };
            var groupCount = new BsonDocument { { "_id", "$Count" }, { "Count", new BsonDocument("$sum", 1) } };

            var aggQuery = WinsGraphBase(userFilter, prizeFilter)
                .Group(group)//.Sum("count", "1")
               .Group(groupCount)
               .Project(new Dictionary<string, object> { { "Count", "$_id" }, { "Winners", "$Count" },{"_id", 0 } })
                .Sort(SortBy.Ascending("UserWins"));

            return AggregationResultToList(RunAggreation(aggQuery.Pipeline));

        }

    

        public IEnumerable<Object> WinnersReport(IFilterBuilder userFilter, IFilterBuilder prizeFilter, ISort sort, int skip, int limit)
            /**
             * > db.User.aggregate( 
             * {$match: { CampaignId: ObjectId("5011b6099610f710f4fd438e"), PrizeCount: {$gt:3}}}, # user fields
             * {$project: { LastName: 1, Prizes:1} } # return fields and Prizes
             * {$unwind: '$Prizes._v'}, # unwind the prizes
             * {$match: {"Prizes._v.PrizeDate": {$gte: NumberLong("634794549227340147"), $lte: NumberLong("634794559975708161")}}}
             * # match on prizes
             * {$group: { _id: { _id: '$_id', LastName: '$LastName' ....}, # re-group all the fields you want to see except...
             * "Prizes" : { $addToSet: '$Prizes._v'}}}, # prizes, which you add to set (and now they are unsorted!)
             * {$project: { _id: "$_id._id", LastName:"$_id.LastName", Prizes: 1}}) # project out of _id to get to top level
             */


        {
            IMongoQuery userQuery = null;

            if (userFilter != null )
            {
                userQuery = new MongoFilterVisitor(userFilter.Build()).Build();
            }
            var queryPrize = Query.And(Query.Exists("Prizes._v"),
                                       Query.Not(Query.Size("Prizes._v", 0)),
                                       Query.NE("Hidden", true));

            userQuery = userQuery != null ? Query.And(queryPrize, userQuery) : queryPrize;

            IMongoQuery prizeQuery = new MongoFilterVisitor(prizeFilter.Build()).Build();

            var groupFields = winnersReportFields.Keys.Where(x => ! x.StartsWith("Prizes")).ToList();
            var id = FieldDocument("$", groupFields);
            var finalProject = FieldDocument("$_id.", groupFields).Add("Prizes", 1);

            var group = new BsonDocument {{"_id", id}, {"Prizes", new BsonDocument("$addToSet", "$Prizes._v")}};
            

            var aggQuery = new AggregationBuilder("User")
                .Match(userQuery)
                .Project(winnersReportFields)
                .Unwind("$Prizes._v")
                .Match(prizeQuery)
                .Group(group)
                .Project(finalProject)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit).Pipeline;

            //Console.WriteLine(aggQuery.ToString());
            return AggregationResultToList(RunAggreation(aggQuery));
        }

        public IEnumerable<Object> SpinReport(IFilterBuilder userFilter, IFilterBuilder eventFilter, ISort sort, int skip, int limit)
        {
            IMongoQuery query = new MongoFilterVisitor(userFilter.Build()).Build();
            IMongoQuery eventQuery = new MongoFilterVisitor(eventFilter.Build()).Build();

            var aggQuery = new AggregationBuilder("UserXUserEvent")
                .Match(query)
                .Project(registrationReportFields)
                .Unwind("UserEvents")
                .Match(eventQuery)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit)
                .Pipeline;

            return AggregationResultToList(RunAggreation(aggQuery));
            
        }
        //private MongoCursor<UserEvent> GetEventsCursor(ObjectId campaignId, 
        //    IEnumerable<EventTypeEnum> eventTypes, DateTime? start, DateTime? end)
        //{
        //    var query = Query.And(Query.EQ("CampaignId", campaignId),
        //                          Query.In("EventTypeId", eventTypes.Select(x => BsonValue.Create((int)x))));
        //    if (start.HasValue)
        //        query = Query.And(query, Query.GTE("EventDate", start.Value.ToUniversalTime().Ticks));

        //    if (end.HasValue)
        //        query = Query.And(query, Query.LTE("EventDate", end.Value.ToUniversalTime().Ticks));
           
        //    query = Query.And(query, Query.NE("Hidden", true));
        //    return Context..SlaveServerCollection.FindAs<UserEvent>(query);
        //}

        public IEnumerable<Object> WinsReport(IFilterBuilder userFilter, IFilterBuilder prizeFilter, ISort sort, int skip, int limit)
        {
            IMongoQuery query = null;

            if (userFilter != null)
            {
                query = new MongoFilterVisitor(userFilter.Build()).Build();
            }
            var queryPrize = Query.And(Query.Exists("Prizes._v"),
                                       Query.Not(Query.Size("Prizes._v", 0)),
                                       Query.NE("Hidden", true));
            if (query != null)
            {
                query = Query.And(queryPrize, query);
            }
            else query = queryPrize;
            var aggQuery = new AggregationBuilder("User")
                .Match(query)
                .Project(winReportFields)
                .Unwind("$Prizes._v")
                .Match(new MongoFilterVisitor(prizeFilter.Build()).Build())
                .Sort(sort)
                .Skip(skip)
                .Limit(limit).Pipeline;

            //Console.WriteLine(aggQuery.ToString());
            return AggregationResultToList(RunAggreation(aggQuery));
        }

        public IEnumerable<Object> PrizesReport(IFilterBuilder userFilter, ISort sort, int skip, int limit)
        {
            IMongoQuery query = null;

            if (userFilter != null)
            {
                query = new MongoFilterVisitor(userFilter.Build()).Build();
            }

            var aggQuery = new AggregationBuilder("Prize")
                .Match(query)
                .Project(prizeReportFields)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit).Pipeline;

            //Console.WriteLine(aggQuery.ToString());
            return RunAggreationToList<Prize>(aggQuery);
        }

        public IPrize AggregationResultForPrizeReport(Object listitem)
        {
            return BsonSerializer.Deserialize<Prize>(listitem.ToJson());

        }

        public IUserProfileEvent AggregationResultAsUserProfileEvent(Object listitem)
        {
            return BsonSerializer.Deserialize<UserProfileEvent>(listitem.ToJson());

        }


        public IUserProfileWinner AggregationResultForWinsReport(Object listitem)
        {

            UserProfileWinner up = new UserProfileWinner();
            try
            {


                var doc = listitem as BsonDocument;
                up.FirstName = doc["FirstName"].AsString;
                up.LastName = doc["LastName"].AsString;
                if (doc.Contains("RegisteredOn"))
                {
                    up.RegisteredOn = new DateTime(doc["RegisteredOn"].AsInt64).ToLocalTime();
                }
                up.Email = doc["Email"].AsString;

                if (doc.Contains("Prizes"))
                {
                    BsonValue ps = doc["Prizes"].AsBsonDocument["_v"];
                    var prize = BsonSerializer.Deserialize<UserPrize>(ps.ToJson());
                    up.Prizes.Add(prize);


                }


            }
            catch (Exception)
            {

            }

            return up;
        }

        public IUserProfileWinner AggregationResultAsUserProfileWinner(Object listitem)
        {

            UserProfileWinner up = new UserProfileWinner();
            try
            {


                var doc = listitem as BsonDocument;
                up.FirstName = doc["FirstName"].AsString;
                up.LastName = doc["LastName"].AsString;
                if (doc.Contains("RegisteredOn"))
                {
                    up.RegisteredOn = new DateTime(doc["RegisteredOn"].AsInt64).ToLocalTime();
                }
                up.Email = doc["Email"].AsString;
                if (doc.Contains("PrizeCount"))
                {
                    up.User.PrizeCount = doc["PrizeCount"].ToDouble();
                }
                if (doc.Contains("Prizes"))
                {
                    BsonValue ps = doc["Prizes"];
                    List<UserPrize> plist = BsonSerializer.Deserialize<List<UserPrize>>(ps.ToJson());

                    foreach (var p in plist)
                    {
                       
                        up.Prizes.Add(p);
                      
                    }

                    up.User.PrizeCount = up.Prizes.Count;
                }


            }
            catch (Exception)
            {

            }

            return up;
        }

        public IPrizeGraph GetWinnersGraphData(IFilterBuilder userFilter, IFilterBuilder prizeFilter)
        {
            var wnrsList = WinnersGraph( userFilter, prizeFilter).ToList();

            
            var totalCount = wnrsList.Select(w => w as BsonDocument)
                                         .Where(doc => doc.Contains("Winners"))
                                         .Sum(doc => GetBsonValue(doc["Winners"]));

            var gs = new Dictionary<string, int>();
           foreach (var l in wnrsList)
               {
                   var o = l as BsonDocument;
                   if(o.ToJson().Contains("Count"))
                   {
                       gs.Add(o["Count"].AsInt32.ToString(), o["Winners"].AsInt32);
                   }
               
               }

            var pg = new PrizeGraph(totalCount, gs);

            return pg;
        }

        private int GetBsonValue(BsonValue value)
        {
            if(value.BsonType == BsonType.Int32)
            {
                return value.AsInt32;
            }
            return (int)value.AsDouble;
        }

        public IEnumerable<IPrize> GetPrizesGraphData(IFilterBuilder userFilter, IFilterBuilder prizeFilter)
        {
           var pzsList = PrizesGraph( userFilter, prizeFilter).ToList();

           var list = BsonSerializer.Deserialize<List<Prize>>(pzsList.ToJson()); 
            //// pzsList.Add(tObj);
            //tObj.Result = Result.ConvertAll(o => (IPrize))
            return list;
        }

        public IEnumerable<IPrize> GetPrizesGridData(IFilterBuilder userFilter, IFilterBuilder prizeFilter, int skip, int limit, ISort sort)
        {
            var pzsList = PrizesData(userFilter, prizeFilter,skip,limit,sort).ToList();

            var list = BsonSerializer.Deserialize<List<Prize>>(pzsList.ToJson());
            //// pzsList.Add(tObj);
            //tObj.Result = Result.ConvertAll(o => (IPrize))
            return list;
        }
       

        public long GetWinsCount(ObjectId campaignId, IFilterBuilder userFilter, IFilterBuilder prizeFilter)
        {
            var pList = WinsGraphBase(userFilter, prizeFilter);
            long totalCount = 0;
            //foreach (var o in pList)
            //{
            //    var u = BsonSerializer.Deserialize<User>(o.ToJson());
            //    totalCount += u.Prizes.Count;
            //}
            return totalCount;
        }

    

        public IEnumerable<Tuple<DateTime, Int32>> RegistrationGraph( IFilterBuilder userFilter, bool byDay)
        {
            IMongoQuery query = null;

            if (userFilter != null)
            {
                query = new MongoFilterVisitor(userFilter.Build()).Build();
            }
         
           
            var aggQuery = new AggregationBuilder("User")
                .Match(query)
                .Project(registrationGraphProjection)
                .Group( byDay ? byDayGroup : byMinuteGroup);

            var im = AggregationResultToList(RunAggreation(aggQuery.Pipeline));

            return byDay ? GraphByDays(im) : GraphByMinutes(im);
        }

        public IEnumerable<Tuple<DateTime, Int32>> WinsGraph(IFilterBuilder userFilter, IFilterBuilder prizeFilter, bool byDay)
        {
            IMongoQuery query = null;

            if (userFilter != null)
            {
                query = new MongoFilterVisitor(userFilter.Build()).Build();
            }
            var queryPrize = Query.And(Query.Exists("Prizes._v"),
                                       Query.Not(Query.Size("Prizes._v", 0)),
                                       Query.NE("Hidden", true));
            if (query != null)
            {
                query = Query.And(queryPrize, query);
            }

            var aggQuery = new AggregationBuilder("User")
                .Match(query)
                .Project(new Dictionary<String, object> {{"Prizes._v", 1}})
                .Unwind("$Prizes._v")
                .Match(new MongoFilterVisitor(prizeFilter.Build()).Build())
                .Project(winGraphProjection)
                .Group(byDay ? byDayGroup : byMinuteGroup);

            var im = AggregationResultToList(RunAggreation(aggQuery.Pipeline));
            return byDay ? GraphByDays(im) : GraphByMinutes(im);
        }

    }}
