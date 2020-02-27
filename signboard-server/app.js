/* library */
//const https = require('https');
const http = require('http');
const fs = require('fs');
const path = require('path');
const express = require('express');
const bodyParser = require('body-parser');

/* core */
const { DB } = require('./db');
const { UserService } = require('./account');

/* define app(2 for secure & download) */
const app = express();//,app_s = express();


app.use(bodyParser.urlencoded({
	limit: '1000mb',
	extended: true
}));
app.use(bodyParser.json({limit: '1000mb'}));

/* Initialize all module */
/* Need to separate "Secure service" & "Download Service" */
//const secure_server = https.createServer(options,app_s);
const download_server = http.createServer(app);

UserService.init(app);

download_server.listen(8888, function() {
    console.log("Download Server listening on port 8888");
});