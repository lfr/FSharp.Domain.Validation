var path = require('path');
var express = require('express');

var app = express();

app.use(express.static(path.join(__dirname, 'public')));
app.set('port', process.env.PORT);
app.set('host', '0.0.0.0');

var server = app.listen(app.get('port'), app.get('host'), function () {
    console.log('listening on port ', server.address().port);
});