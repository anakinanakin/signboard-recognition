// dealing with user account entries
const path = require('path');
const fs = require('fs');
//const mkdirp = require('mkdirp');
const { DB } = require('./db');
//const { MailMan } = require('./mail');
 
var upload = multer({ storage: storage })

class UserService {
    init(app){
        app.post('/set_shopInfo',this.set_shopInfo);
        app.get('/get_shopInfo',this.get_shopInfo);
        
    }

    get_shopInfo(req,res){
        // using shopID to get information of shop
        DB.shopInfo_download(req.query.shopID,function(err,msg){
            if(err)
                res.status(500).send(msg);
            else{
                res.end(JSON.stringify(msg)); // stringify json object
                console.log(JSON.stringify(msg));
            }
        });
    }

    set_shopInfo(req,res){
        // need password to set shopinfo
        const infoBody = req.body;
        console.log(infoBody);
        DB.shopInfo_update(infoBody.shopID,infoBody.shopName,infoBody.shopAddress,
            infoBody.telephone,JSON.parse(infoBody.gps),JSON.parse(infoBody.openTime),
            JSON.parse(infoBody.infoList),JSON.parse(infoBody.picture),function(err,msg){
                if(err)
                    res.status(500).send(msg);
                else
                    res.end(msg);
        });
    }
}

module.exports = {
    UserService: new UserService()
}