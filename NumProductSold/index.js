var hdb = require('hdb');

module.exports = function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    var client = hdb.createClient({
        host     : '10.161.8.5',
        port     : 31041 , 
        user     : 'Xamarin',
        password : 'Microsoft1234'
    });

    client.on('error', function (err) {
        context.res = {
                    status: 400,
                    body: "Network connection error. Function side."
        };
        context.done();
    });

    client.connect(function (err) {
        if (err) {
         	return context.log('Connect error', err);
        }

        //Default query parameters
        var d = new Date();
        var curMonth = d.getMonth() + 1;
        var dateclause = 'AND "DEMODB"."DimDate"."MonthNumberOfYear" = ' + curMonth + ' '
        
        if (req.query.quarter)
            dateclause = 'AND "DEMODB"."DimDate"."CalendarQuarter" = ' + req.query.quarter + ' '
        if (req.query.month)
            dateclause = 'AND "DEMODB"."DimDate"."MonthNumberOfYear" = ' + req.query.month + ' '

        var query = `SELECT "DEMODB"."DimSalesTerritory"."SalesTerritoryCountry" AS country,
                        SUM("DEMODB"."FactInternetSalesPartition"."OrderQuantity")AS totalqty
                    FROM "DEMODB"."FactInternetSalesPartition" 
                    INNER JOIN  "DEMODB"."DimProduct"
	                    ON "DEMODB"."FactInternetSalesPartition"."ProductKey" = "DEMODB"."DimProduct"."ProductKey"
                    INNER JOIN "DEMODB"."DimSalesTerritory"
	                    ON "DEMODB"."FactInternetSalesPartition"."SalesTerritoryKey" = "DEMODB"."DimSalesTerritory"."SalesTerritoryKey"
                    INNER JOIN "DEMODB"."DimDate"
	                    ON "DEMODB"."DimDate"."DateKey" = "DEMODB"."FactInternetSalesPartition"."OrderDateKey"
                    WHERE  UCase("DEMODB"."DimProduct"."EnglishProductName") LIKE UCase('%` + req.query.product + `%')` +
                    `   AND "DEMODB"."DimDate"."CalendarYear" = 2013 ` + dateclause +
                    `GROUP BY "DEMODB"."DimSalesTerritory"."SalesTerritoryCountry"`


        client.exec(query, function(err, rows) {
	        client.end();

            if (err) {
            return context.log('Execute error:', err);
            }

            if (req.query.product) {
                context.res = {
                    // status: 200, /* Defaults to 200 */
                    body: JSON.stringify(rows)
                };
            }
            else {
                context.res = {
                    status: 400,
                    body: "Please pass product in query string"
                };
            }
            context.done();
        });
    });
};

