var https = require('https');
var crypto = require('crypto');
var moment = require('moment')

var namespace = null;
var hubname = null;
var keyvalue = null;
var keyname = null;


function AzureEventHubManager(namespace, hubname, keyname, keyvalue)
{
    this.namespace = namespace;
    this.hubname = hubname;
    this.keyname = keyname;
    this.keyvalue = keyvalue;
}

AzureEventHubManager.prototype = {
    create_sas_token: function(uri)
    {
        // Token expires in one hour
        var expiry = moment().add(1, 'hours').unix();

        var string_to_sign = encodeURIComponent(uri) + '\n' + expiry;
        var hmac = crypto.createHmac('sha256', this.keyvalue);
        hmac.update(string_to_sign);
        var signature = hmac.digest('base64');
        var token = 'SharedAccessSignature sr=' + encodeURIComponent(uri) + '&sig=' + encodeURIComponent(signature) + '&se=' + expiry + '&skn=' + this.keyname;

        return token;
    },
    getOptions : function(sas, payload, devicename)
    {
        return {
            hostname: this.namespace + '.servicebus.windows.net',
            port: 443,
            path: '/' + this.hubname + '/publishers/' + devicename + '/messages',
            method: 'POST',
            headers: {
                'Authorization': sas,
                'Content-Length': payload.length,
                'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
            }
        };
    },
    sendMessage : function(payload, devicename)
    {
        var uri = 'https://' + this.namespace + '.servicebus.windows.net' + '/' + this.hubname + '/publishers/' + devicename + '/messages';

        var sas = this.create_sas_token(uri);
        var options = this.getOptions(sas, payload, devicename);

        var req = https.request(options, function(res) {
            console.log("statusCode: ", res.statusCode);
            console.log("headers: ", res.headers);

            res.on('data', function(d) {
                process.stdout.write(d);
            });
        });

        req.on('error', function(e) {
            console.error(e);
        });

       var res =  req.write(payload);
        req.end();
    }

}

module.exports = AzureEventHubManager