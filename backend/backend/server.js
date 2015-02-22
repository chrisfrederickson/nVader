var port = process.env.port || 8080;
var sql = require('mssql');
var express = require('express');
var app = express();
var bodyParser = require("body-parser");
var coordinator = require('coordinator');
var llUtm = coordinator('latlong', 'utm');
var http = require('http');
var bot = require('nodemw');
var _ = require('underscore');

// pass configuration object
var wikiClient = new bot({
    server: 'en.wikipedia.org',  // host name of MediaWiki-powered site
    path: '/w',                  // path to api.php script
    debug: false                 // is more verbose when set to true
});

app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

var config = {
    user: process.env.dbusername,
    password: process.env.dbpassword,
    server: "jplzp8dar5.database.windows.net",
    database: "nVader",
    options: { encrypt: true }
};
console.log(process.env.dbusername);
sql.connect(config, function (err) {
    if (err) {
        console.log("ERROR IN SQL CONNECT!:");
        console.log(err);
        return;
    }
    console.log("SQL DONE!");
    app.get("/polygon", function (req, res) {
        var sql_req = new sql.Request();
        sql_req.query("SELECT * FROM dbo.polygon WHERE CAST(Refnum AS INT) = " + req.query.id, function (err, recordset) {
            if (err) {
                console.log("ERROR IN SQL POLYGON!:" + err);
                return;
            }
            //console.dir(recordset);
            res.send(recordset);
        });
    });
    app.get("/goodies", function (req, res){
        var Package = {};
        var sql_req1 = new sql.Request();
        var zone = 0;
        var easting = 0;
        var northing = 0;
        var xy = [];
        var pos = llUtm(req.query.lat || 39.955277, req.query.lon || -75.186532);
        zone = pos.zoneNumber;
        easting = pos.easting;
        northing = pos.northing;
        var query = "SELECT TOP 10 Refnum,UTM_easting,UTM_northing FROM dbo.polygon WHERE CAST(UTM_zone AS INT) = " + zone + " ORDER BY SQRT(POWER(CAST((UTM_easting-" + easting + ") AS BIGINT),2) + POWER(CAST((UTM_northing-" + northing + ") AS BIGINT),2)) ASC";
        sql_req1.query(query, function (err, recordset) {
            if (err) {
                res.sendStatus(500);
                console.log("ERROR IN GOODIES1:");
                console.log(err);
                return;
            }
            if (recordset.length == 0) {
                res.send("{\"none\":true}");
                console.log("NONE!");
                console.log(query);
                return;
            }
            console.log(recordset);
            var sql_req2 = new sql.Request();
            Package.dist = Math.sqrt(Math.pow(recordset[0].UTM_easting - easting, 2) + Math.pow(recordset[0].UTM_northing - northing, 2));
            sql_req2.query("SELECT Resname FROM dbo.main WHERE CAST(Refnum AS INT) =" + recordset[0].Refnum, function (err, recordset2) {
                if (err) {
                    res.sendStatus(500);
                    console.log("ERROR IN GOODIES2:");
                    console.log(err);
                    return;
                }
                Package.name = recordset2[0].Resname;
                
                var options = {
                    host: "api.duckduckgo.com",
                    path: "/?q="+require('querystring').escape(Package.name)+"&format=json&t=nvader"
                };
                http.request(options, function (response) {
                    var ddg = "";
                    response.on('data', function (chunk) {
                        ddg += chunk;
                    });
                    response.on('end', function () {
                        ddg = JSON.parse(ddg);
                        Package.description = ddg.AbstractText;
                        var regexp = /\/([^\/]*)$/;
                        var abstURLPart = regexp.exec(ddg.AbstractURL);
                        var wikiArticle = require('querystring').unescape(abstURLPart[1]);
                        Package.wikiArticle = wikiArticle;
                        wikiClient.getArticle(wikiArticle, function (err, data) {
                            if (err) {
                                console.log("ERROR IN WIKI:");
                                console.log(err);
                                res.send(Package);
                            } else {
                                var builtRegexp = /built = (\d{4})/.exec(data);
                                if (builtRegexp != null) {
                                    Package.buildDate = builtRegexp[1];
                                } else {
                                    Package.buildDate = Math.floor(1900 + Math.random() * 100);
                                }
                                res.send(Package);
                            }
                        });
                    });
                }).end();
            });
        });
        
        
    })
});

app.get("/", function (req, res) {
    res.send("Hello World!");
});

app.listen(port);