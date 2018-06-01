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

        var query = `SELECT * FROM "DEMODB"."DimEmployee" WHERE "EmployeeKey" = ` + req.query.employeeid

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
                    body: "Please pass employeeid in query string " + err
                };
            }
            context.done();
        });
    });
};
