using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using OpusOneCorp.Data.Core;
using OpusOneCorp.Data.Core.Domain;

namespace OpusOneCorp.Data.Mongo.Aggregation
{
    public class ViralDNAUserPromoDao : AggregationReportDao<User>, IViralDNAUserPromoDao
    {                                                                             
        private static readonly Dictionary<string, object> userPromoListFields = new Dictionary<string, object>
                                                                                       {
                                                                                           {"RegisteredOn", 1},
                                                                                           {"FirstName", 1},
                                                                                           {"LastName", 1},
                                                                                           {"Email", 1},
                                                                                           {"RegisteredOnLocalDate", 1},
                                                                                           {"PromoCodes._v", 1}
                                                                                       };

        private static readonly Dictionary<string, object> userPromoListFieldsNoV = new Dictionary<string, object>
                                                                                       {
                                                                                           {"RegisteredOn", 1},
                                                                                           {"FirstName", 1},
                                                                                           {"LastName", 1},
                                                                                           {"Email", 1},
                                                                                           {"RegisteredOnLocalDate", 1},
                                                                                           {"PromoCode", "$PromoCodes._v"}
                                                                                       };

        private static readonly Dictionary<string, object> userPromoListFieldsAfterUnwind = new Dictionary<string, object>
                                                                                       {
                                                                                           {"RegisteredOn", "$_id.RegisteredOn"},
                                                                                           {"FirstName", "$_id.FirstName"},
                                                                                           {"LastName", "$_id.LastName"},
                                                                                           {"Email", "$_id.Email"},
                                                                                           {"RegisteredOnLocalDate", "$_id.RegisteredOnLocalDate"},
                                                                                           {"PromoCode", 1},
                                                                                           {"_id", 0}
                                                                                       };

        private static readonly Dictionary<string, object> userPromoFinalFields = new Dictionary<string, object>
                                                                                       {
                                                                                           {"RegisteredOn", 1},
                                                                                           {"FirstName", 1},
                                                                                           {"LastName", 1},
                                                                                           {"Email", 1},
                                                                                           {"PromoCode.Code", 1},
                                                                                           {"PromoCode.CodeType", 1},
                                                                                           {"PromoCode.UsedDate", 1}
                                                                                       };

        private static readonly Dictionary<string, object> userPromoCsvFields = new Dictionary<string, object>
                                                                                       {                                                                                           
                                                                                           {"LocationAddress1", 1},
                                                                                           {"LocationCity", 1},
                                                                                           {"LocationState", 1},
                                                                                           {"LocationZipCode", 1},
                                                                                           {"ContactPhoneHome", 1},
                                                                                           {"DemographicDateofBirth", 1}
                                                                                       };

        static ViralDNAUserPromoDao()
        {
            foreach (var f in userPromoCsvFields)
            {
                userPromoListFields.Add(f.Key, f.Value);
                userPromoListFieldsNoV.Add(f.Key, f.Value);
                userPromoFinalFields.Add(f.Key, f.Value);
                userPromoListFieldsAfterUnwind.Add(f.Key, "$_id." + f.Key);
            }
        }

        //private static readonly Dictionary<string, object> userPromoCsvFields = new Dictionary<string, object>
        //                                                                               {
        //                                                                                   {"RegisteredOn", 1},
        //                                                                                   {"FirstName", 1},
        //                                                                                   {"LastName", 1},
        //                                                                                   {"LocationAddress1", 1},
        //                                                                                   {"LocationCity", 1},
        //                                                                                   {"LocationState", 1},
        //                                                                                   {"LocationZipCode", 1},
        //                                                                                   {"ContactPhoneHome", 1},
        //                                                                                   {"Email", 1},
        //                                                                                   {"DemographicDateofBirth", 1},
        //                                                                                   {"PromoCode.Code", 1},
        //                                                                                   {"PromoCode.CodeType", 1},
        //                                                                                   {"PromoCode.UsedDate", 1}
        //                                                                               };

        private static readonly Dictionary<string, object> registrationGraphProjection = new Dictionary<string, object>
                          {{"_id", 0 }, {"Year", "$RegisteredOnLocalDate.Year"}, {"DayOfYear", "$RegisteredOnLocalDate.DayOfYear"},
                               {"Hour", "$RegisteredOnLocalDate.Hour"}, {"Minute",  "$RegisteredOnLocalDate.Minute"}
                          };


        private static readonly Dictionary<string, object> userPromoGraphProjection = new Dictionary<string, object>
                          {{"Year", "$PromoCode.UsedLocalDate.Year"}, {"DayOfYear", "$PromoCode.UsedLocalDate.DayOfYear"},
                               {"Hour", "$PromoCode.UsedLocalDate.Hour"}, {"Minute",  "$PromoCode.UsedLocalDate.Minute"}
                          };

        public IEnumerable<IUserProfilePromo> UserPromoCodeReport(IFilterBuilder userFilter, IFilterBuilder promoFilter, ISort sort, int skip, int limit, EventTypeEnum eventType)
        {
            List<IUserProfilePromo> result = null;
            if (eventType == EventTypeEnum.RegistrationSuccessful)
                result = ReportRegistration(userFilter, promoFilter, sort, skip, limit).ToList();
            if (eventType == EventTypeEnum.PromoCodeSuccessful)
                result = ReportUserPromoCode(userFilter, promoFilter, sort, skip, limit).ToList();
            return result;
        }

        private IEnumerable<IUserProfilePromo> GetAggregationResult(BsonDocument[] aggQuery)
        {
            var result = RunAggreationToList<UserProfilePromo>(aggQuery);
            foreach (UserProfilePromo userProfilePromo in result)
            {
                var doc = userProfilePromo.ExtendedProperties["PromoCode"].AsBsonDocument;
                var promoCode = BsonSerializer.Deserialize<UserPromoCode>(doc.ToJson());

                userProfilePromo.Code = promoCode.Code;
                userProfilePromo.CodeType = promoCode.CodeType;
                userProfilePromo.UsedDate = promoCode.UsedDate;
            }
            return result;
        }

        //public IUserProfilePromo AggregationResultForReport(Object listitem)
        //{
        //    var up = new UserProfilePromo();
        //    try
        //    {
        //        var doc = listitem as BsonDocument;
        //        if (doc != null)
        //        {
        //            //if (doc["_id"].BsonType == BsonType.Document)
        //            //    doc = doc["_id"].AsBsonDocument;
        //            up.FirstName = doc["FirstName"].AsString;
        //            up.LastName = doc["LastName"].AsString;
        //            if (doc.Contains("RegisteredOn"))
        //            {
        //                up.RegisteredOn = new DateTime(doc["RegisteredOn"].AsInt64).ToLocalTime();
        //            }
        //            up.Email = doc["Email"].AsString;

        //            BsonDocument sub = doc["PromoCode"].AsBsonDocument;
        //            var promoCode = BsonSerializer.Deserialize<UserPromoCode>(sub.ToJson());
        //            up.Code = promoCode.Code;
        //            up.CodeType = promoCode.CodeType;
        //            up.UsedDate = promoCode.UsedDate;
        //            up.PromoCodes.Add(promoCode);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return up;
        //}

        protected IMongoQuery BuildQuery(IFilterBuilder userFilter)
        {
            IMongoQuery query = null;

            if (userFilter != null)
            {
                query = new MongoFilterVisitor(userFilter.Build()).Build();
            }
            var queryPromoCode = Query.And(Query.Exists("PromoCodes._v"),
                                       Query.Not(Query.Size("PromoCodes._v", 0)),
                                       Query.NE("Hidden", true));
            if (query != null)
            {
                query = Query.And(queryPromoCode, query);
            }
            else query = queryPromoCode;

            return query;
        }


        protected AggregationBuilder RegistrationReportCommon(Dictionary<string, object> projection, IFilterBuilder userFilter, IFilterBuilder promoFilter)
        {
            IMongoQuery query = BuildQuery(userFilter);
            var sortByUsedDate = SortBy.Ascending("_id", "PromoCodes._v.UsedDate");
            var groupByUserPromo = FieldDocument("$", "_id", "RegisteredOn", "Email", "FirstName", "LastName", "RegisteredOnLocalDate",
                "LocationAddress1", "LocationCity", "LocationState", "LocationZipCode", "ContactPhoneHome", "DemographicDateofBirth");
            return new AggregationBuilder("User")
                .Match(query)
                .Project(projection)
                .Unwind("$PromoCodes._v")
                .Sort(sortByUsedDate)
                .Group("_id", groupByUserPromo)
                .First("PromoCode", "$PromoCodes._v")
                .Project(userPromoListFieldsAfterUnwind)
                .Match(new MongoFilterVisitor(promoFilter.Build()).Build());
        }

        private IEnumerable<IUserProfilePromo> ReportRegistration(IFilterBuilder userFilter, IFilterBuilder promoFilter, ISort sort, int skip, int limit)
        {
            var aggQuery = RegistrationReportCommon(userPromoListFields, userFilter, promoFilter)
                .Project(userPromoFinalFields)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit).Pipeline;

            // Debug.WriteLine(aggQuery.ToJson());
            // var result = RunAggreationToList<UserProfilePromo>(aggQuery);
            return GetAggregationResult(aggQuery); 
        }

        protected AggregationBuilder UserPromoCodeReportCommon(Dictionary<string, object> projection, IFilterBuilder userFilter, IFilterBuilder promoFilter)
        {
            IMongoQuery query = BuildQuery(userFilter);
            return new AggregationBuilder("User")
                .Match(query)
                .Project(projection)
                .Unwind("$PromoCodes._v")
                .Project(userPromoListFieldsNoV)
                .Match(new MongoFilterVisitor(promoFilter.Build()).Build());
        }

        private IEnumerable<IUserProfilePromo> ReportUserPromoCode(IFilterBuilder userFilter, IFilterBuilder promoFilter, ISort sort, int skip, int limit)
        {
            var aggQuery = UserPromoCodeReportCommon(userPromoListFields, userFilter, promoFilter)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit).Pipeline;

            // Debug.WriteLine(aggQuery.ToJson());
            // return RunAggreationToList<UserProfilePromo>(aggQuery);
            return GetAggregationResult(aggQuery);
        }

        public IEnumerable<Tuple<DateTime, Int32>> UserPromoCodeGraph(IFilterBuilder userFilter, IFilterBuilder promoFilter, bool byDay, EventTypeEnum eventType)
        {
            if (eventType == EventTypeEnum.RegistrationSuccessful)
                return GraphRegistration(userFilter, promoFilter, byDay);
            if (eventType == EventTypeEnum.PromoCodeSuccessful)
                return GraphUserPromoCode(userFilter, promoFilter, byDay);
            return null;
        }

        private IEnumerable<Tuple<DateTime, Int32>> GraphRegistration(IFilterBuilder userFilter, IFilterBuilder promoFilter, bool byDay)
        {
            var aggQuery = RegistrationReportCommon(userPromoListFields, userFilter, promoFilter)
                .Project(registrationGraphProjection)
                .Group(byDay ? byDayGroup : byMinuteGroup).Pipeline;

            // Debug.WriteLine(aggQuery.ToJson());
            var im = AggregationResultToList(RunAggreation(aggQuery));
            return byDay ? GraphByDays(im) : GraphByMinutes(im);
        }

        private IEnumerable<Tuple<DateTime, Int32>> GraphUserPromoCode(IFilterBuilder userFilter, IFilterBuilder promoFilter, bool byDay)
        {
            var aggQuery = UserPromoCodeReportCommon(new Dictionary<String, object> { { "PromoCodes._v", 1 } }, userFilter,
                                                     promoFilter)
                .Project(userPromoGraphProjection)
                .Group(byDay ? byDayGroup : byMinuteGroup).Pipeline;

            // Debug.WriteLine(aggQuery.ToJson());
            var im = AggregationResultToList(RunAggreation(aggQuery));
            return byDay ? GraphByDays(im) : GraphByMinutes(im);
        }

    }
}