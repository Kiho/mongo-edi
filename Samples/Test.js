db = connect("localhost:27017/test-db");

var Facebook = {
    "Name" : "Facebook",
    "Path" : "socialmedia/facebook",
}
var Twitter = {
    "Name": "Twitter",
    "Path": "socialmedia/twitter"
}

var addProject = function (obj) {
    var query = { Name: obj.Name };
    var feature = db.Project.find(query).toArray()[0];
    if (!feature) {
        db.Project.insert(obj);
        print("Insert: " + query.Name);
    } else {
        print("Found: " + query.Name);
    } 
};

print('Start');

addProject(Facebook);
addProject(Twitter);

print('Done!');