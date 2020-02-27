// using mongodb as our database
const mongoose = require('mongoose');


class DB {
	constructor(){

		mongoose.connect('mongodb://anakin1:fefe3939@localhost:27017/csieproject');
		this.DB = mongoose.connection;

		// shop info
		this.shopInfoSchema = mongoose.Schema({
			shopID: String,
			shopName: String,
			shopAddress: String,
			telephone: String,
			gps: {latitude: String, longitude: String},
			openTime: {Items: [{
				timePeriod: [{
					begin_hr: String,
					begin_min: String,
					end_hr: String,
					end_min: String
				}],
				open: Boolean
			}]},
			infoList: {Items: [{
				title: String,
				content: String
			}]},
			picture: {Items: [{ 
				data: Buffer,
				contentType: String 
			}]}
		}); 
		
			fav_shopID:[{shopID: String}],
			event_coll:[{id: Number, num: Number}],
			achievement_coll:[{id: Number, level: Number}],
			chara_startTime: String,
			chara_coll: [{
				id: Number,
				name: String,
				value_strength: Number,
				value_intelligence: Number,
				value_like: Number,
				ending: Number,
				startTime: String,
				endTime: String
			}],
			gameMode: Boolean,
			gameObj: Boolean,
			infoObj: Boolean,
			decoration: [{
				id: Number,
				pos: { x: Number, y: Number, z: Number },
				index: Number
			}]
		});

		// shop keeper info
		this.shopKeeperSchema = mongoose.Schema({
			shopID: String,
			email: String,
			password: String,
			shopName: String,
			phone: String,
			updateTime: String,
			problemReport: [{ who: String, date: String, content: String }],
			shop_category_1: Number,
			shop_category_2: Number,
			shop_principal: String,
			shop_principal_gender: Number,
			shop_principal_phone: String,
			shop_principal_email: String,
			tmp: String
		});
		
		// user comment
		this.commentSchema = mongoose.Schema({
			userID: String,
			shopID: String,
			text_content: String,
			picture: { data: Buffer, contentType: String },
			time: String,
			score: Number
		});
		// adf usage
		this.adfSchema = mongoose.Schema({
			beaconID: String,
			adfID: [ { name: String , shopID: [ { name: String } ] } ]
		});
		this.adfObjSchema = mongoose.Schema({
			id: String,
			beaconID: String,
			adfID: String,
			shopID: String,
			shopName: String,
			shopIntro: String,
			pos: String,
			rot: String,
			scale: String
		});
		// NavGamer Lite (Also, can reuse the Schema of origin one)
		this.liteSchema = mongoose.Schema({
			imageID: String,
			shopID: String,
			shopName: String
		});*/

		this.shop_m = mongoose.model('shop_m',this.shopInfoSchema);
	}


	shopInfo_update(shopID,shopName,shopAddress,telephone,gps,openTime,infoList,picture,callback){
		// using shopName to fetch
		var shop_model = this.shop_m;

		shop_model.findOne({shopID: shopID, shopName: shopName},'',function(err,match){
			if(err)
				callback(1,"internal error");
			else{
				if(match == null){
					// not found , and create 
					let newInfo = new shop_model({shopID: shopID,shopName: shopName,
					shopAddress: shopAddress,telephone: telephone,gps: gps,
					openTime: openTime,infoList: infoList,picture: picture});
					newInfo.save(function(err,newInfo){
						if(err)
							callback(1,"internal error");
						else
							callback(0,"success");
					});
				}
				else{
					// existed, and then update
					match.shopAddress = shopAddress;
					match.openTime = openTime;
					match.infoList = infoList;
					match.telephone = telephone;
					match.gps = gps;
					match.picture = picture;
					match.save(function(err,match){
						if(err)
							callback(1,"internal error");
						else{
							callback(0,"success");
						}
					});
				}
			}
		});
	}

	shopInfo_download(shopID,callback){
		this.shop_m.findOne({shopID: shopID},'shopName shopAddress telephone gps openTime infoList picture',function(err,match){
			if(err)
				callback(1,"internal error");
			else{
				if(match == null){
					// not found 
					callback(1,"not found");
				}
				else{
					// found 
					callback(0,match);
				}
			}
		});
	}

module.exports = {
	DB: new DB()
}