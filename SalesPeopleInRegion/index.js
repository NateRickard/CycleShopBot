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
        var numtoreturn = 10

        if (req.query.count)
            numtoreturn = req.query.count

        var query
        if (req.query.regionCode > 0)
        {
            query = `SELECT TOP ` + numtoreturn + ` * FROM "DEMODB"."DimEmployee"
            WHERE "SalesTerritoryKey" = ` + req.query.regionCode
        } else
        {
            query = `SELECT TOP ` + numtoreturn + ` * FROM "DEMODB"."DimEmployee"
            INNER JOIN "DEMODB"."DimSalesTerritory" ON "DEMODB"."DimEmployee"."SalesTerritoryKey" = "DEMODB"."DimSalesTerritory"."SalesTerritoryKey"
            WHERE "DEMODB"."DimSalesTerritory"."SalesTerritoryRegion" = '` + req.query.region + `'`
        }


        client.exec(query, function(err, rows) {
	        client.end();

            if (err) {
            return context.log('Execute error:', err);
            }

            if (req.query.region) {
                context.res = {
                    // status: 200, /* Defaults to 200 */
                    body: JSON.stringify(rows)
                };
            }
            else {
                context.res = {
                    status: 400,
                    body: "Please pass region number in query string"
                };
            }
            context.done();
        });
    });
};
