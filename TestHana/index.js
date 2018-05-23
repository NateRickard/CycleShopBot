var hdb    = require('hdb');


module.exports = function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    var payload = '';
    var client = hdb.createClient({
        host     : '10.161.8.5',
        port     : 31041 ,
        //instanceNumber : '10',       // instance number of the HANA system
        //databaseName   : 'S4D',   
        user     : 'Xamarin',
        password : 'Microsoft1234'
    });
    context.log('Configured client.');


    client.on('error', function (err) {
        context.log('Network connection error', err);
    });

    client.connect(function (err) {
    if (err) {
      	return context.log('Connect error', err);
    }
    context.log('about to SELECT data');

    //client.exec('select * from DUMMY', function (err, rows) {
    //client.exec('select SQL_PORT from M_SERVICES', function (err, rows) {
    client.exec('select count(*) from "DEMODB"."DimCurrency"',function(err,rows) {
    context.log('finished SELECT data');

	client.end();
    if (err) {
      return context.log('Execute error:', err);
    }
    context.log('returned from SELECT')
    context.log(rows)
    payload = rows

    if (req.query.name || (req.body && req.body.name)) {
        context.res = {
            // status: 200, /* Defaults to 200 */
            body: "Hello " + (req.query.name || req.body.name) + payload

        };
    }
    else {
        context.res = {
            status: 400,
            body: "Please pass a name on the query string or in the request body"
        };
    }
    context.done();
  });
});


   
};

