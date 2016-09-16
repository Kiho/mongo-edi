db = connect("localhost:27017/self-serve");

var Facebook = {
        "Who" : ObjectId("54d955aefaa2e313acc67978"),
        "Name" : "Facebook Share Enable",
        "Path" : "promotionwizard/socialmedia/viralshare/facebookenable",
        "Description" : "",
        "IsAddOn" : false,
        "HelpText" : "",
        "Order" : 0,
        "DefaultVisibility" : 0,
        "LiveEditRoles" : 6
}
var Email = {
    "Name": "Email Share Enable",
    "Path": "promotionwizard/socialmedia/viralshare/emailenable"
}
var Twitter = {
    "Name": "Twitter Share Enable",
    "Path": "promotionwizard/socialmedia/viralshare/twitterenable"
}

var addProductFeature = function (obj, item) {
    if (item) {
        obj.Name = item.Name;
        obj.Path = item.Path;
    }
    var query = { Name: obj.Name };
    var feature = db.ProductFeature.find(query).toArray()[0];
    if (!feature) {
        db.ProductFeature.insert(obj);
        print("Insert: " + query.Name);
    } else {
        print("Found: " + query.Name);
    } 
};

print('Start');

addProductFeature(Facebook);
addProductFeature(Facebook, Twitter);
addProductFeature(Facebook, Email);

print('Done!');