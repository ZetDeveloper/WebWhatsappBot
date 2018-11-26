using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebWhatsappBotCore
{
    public static class Scripting
    {

        public static string masterScript = @"
if (!window.Store) {
    (function() {
        function getStore(modules) {
            let foundCount = 0;
            let neededObjects = [
                { id: 'Store', conditions: (module) => (module.Chat && module.Msg) ? module : null },
                { id: 'Wap', conditions: (module) => (module.createGroup)? module : null },
                { id: 'MediaCollection', conditions: (module) => (module.default && module.default.prototype && module.default.prototype.processFiles !== undefined) ? module.default : null },
                { id: 'WapDelete', conditions: (module) => (module.sendConversationDelete && module.sendConversationDelete.length == 2) ? module : null },
                { id: 'Conn', conditions: (module) => (module.default && module.default.ref && module.default.refTTL) ? module.default : null },
                { id: 'WapQuery', conditions: (module) => (module.queryExist)? module : null },
                { id: 'ProtoConstructor', conditions: (module) => (module.prototype && module.prototype.constructor.toString().indexOf('binaryProtocol deprecated version') >= 0) ? module : null },
                { id: 'UserConstructor', conditions: (module) => (module.default && module.default.prototype && module.default.prototype.isServer && module.default.prototype.isUser) ? module.default : null }
            ];

            for (let idx in modules) {
                if ((typeof modules[idx] === 'object') && (modules[idx] !== null)) {
                    let first = Object.values(modules[idx])[0];
                    if ((typeof first === 'object') && (first.exports)) {
                        for (let idx2 in modules[idx]) {
                            let module = modules(idx2);
                            if (!module) {
                                continue;
                            }

                            neededObjects.forEach((needObj) => {
                                if(!needObj.conditions || needObj.foundedModule) return;
                                let neededModule = needObj.conditions(module);
                                if(neededModule !== null) {
                                    foundCount++;
                                    needObj.foundedModule = neededModule;
                                }
                            });

                            if(foundCount == neededObjects.length) {
                                break;
                            }
                        }

                        let neededStore = neededObjects.find((needObj) => needObj.id === 'Store');
window.Store = neededStore.foundedModule? neededStore.foundedModule : { };
neededObjects.splice(neededObjects.indexOf(neededStore), 1);
                        neededObjects.forEach((needObj) => {
                            if(needObj.foundedModule) {
                                window.Store[needObj.id] = needObj.foundedModule;
                            }
                        });

                        return window.Store;
                    }
                }
            }
        }

        webpackJsonp([], { 'parasite': (x, y, z) => getStore(z)}, 'parasite');
    })();
}

";

        public static string codeAjax = @"
 var data = jjj;
        var xhr = new XMLHttpRequest();
        xhr.withCredentials = true;
        xhr.addEventListener(""readystatechange"", function () {
            if (this.readyState === 4) {
                console.log(this.responseText);
            }
        });
        xhr.open(""POST"", 'http://localhost:8080/zetdeveloper/"");
        xhr.setRequestHeader(""Content-Type"", ""application/json"");
        xhr.setRequestHeader(""cache-control"", ""no-cache"");
        xhr.send(data);

";

        public static string newMessageScan = masterScript + @"

    eventMsgNew = window.Store.Msg.on('add', (newMessage) => {
    if (newMessage && newMessage.isNewMsg && !newMessage.isSentByMe) {
        var jsonMessage = JSON.stringify(newMessage.toJSON());



        console.log(jsonMessage);
        var data = jsonMessage;
        var xhr = new XMLHttpRequest();
        xhr.withCredentials = true;
        xhr.addEventListener('readystatechange', function () {
            if (this.readyState === 4) {
                console.log(this.responseText);
            }
        });
        xhr.open('POST', 'http://localhost:8080/zetdeveloper/');
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.setRequestHeader('cache-control', 'no-cache');
        xhr.send(data);
    }
});

return 0;

";


        //Script for create group
        public static string createGroup = masterScript + @"

            var nameGroup = arguments[0];
            var splitNumber = arguments[1].split(',');
            var participants = [];

            splitNumber.forEach(x => {
                participants.push(x);
            });


            function getContact(_id)
                    {
                        let result = null;
                        Store.Contact.models.forEach(x => {
                            if (x.hasOwnProperty('__x_id') && x.__x_id == _id)
                            {
                                result = x;
                            }
                        });
                        return result;
                    }

            var canCreate = true;
            var p = [];
			for (var x = 0; x < participants.length; x++)
			{
                var c = getContact(participants[x]);
                if(c!=null){
                    p.push(c);
                }else
                {
                    canCreate = fasle;
                }
			}

           if(canCreate){ 
                Store.Chat.createGroup(nameGroup, null, null, p, undefined);
                return true;
            }else{ return false; }";




        //Script for get all new messages
        public static string getAllChats = masterScript + @"
                                                var msgs = []

                                                Store.Msg.models.forEach(model => {
				                                                if (model.__x_isNewMsg && !model.__x_isSentByMe && 
                                                                    model.__x_isUserCreatedType && !model.__x_isNotification) {
					                                                model.__x_isNewMsg = false;
					                                                console.log(model.__x_id._serialized);
                                                                    console.log(model.__x_body);
                                                                    console.log(model);
                                                                    msgs.push(model);
				                                                }
			                                                });


                                                return  JSON.stringify(msgs);
                                           ";

        public static string SendMessageByName = masterScript + @"
                                                var Chats = Store.Chat.models;
                                                var id = arguments[0];
                                                var message = arguments[1];
                                                for (chat in Chats) {
                                                    if (isNaN(chat)) {
                                                        continue;
                                                    };
                                                    var temp = {};
                                                   temp.contact = Chats[chat].__x__formattedTitle;
                                                   temp.id = Chats[chat].__x_formattedTitle;
                                                    if(temp.id == id){
                                                        Chats[chat].sendMessage(message + ' https://github.com/ZetDeveloper/WebWhatsappBot Credit: ZetDeveloper');
                                                        return true;
                                                    }
                                                }
                                                return false;
                                           ";


        //Script for send a message by id, example id: 52111111111@c.us
        public static string SendMessageByID = masterScript + @"
                                                var Chats = Store.Chat.models;
                                                var id = arguments[0];
                                                var message = arguments[1];
                                                for (chat in Chats) {
                                                    if (isNaN(chat)) {
                                                        continue;
                                                    };
                                                    var temp = {};
                                                    temp.contact = Chats[chat].__x__formattedTitle;
                                                    temp.id = Chats[chat].__x_id;
                                                    if(temp.id == id){
                                                        Chats[chat].sendMessage(message + ' https://github.com/ZetDeveloper/WebWhatsappBot Credit: ZetDeveloper');
                                                        return true;
                                                    }
                                                }
                                                return false;
                                           ";



        //Script for send a message by id, example id: 52111111111@c.us
        public static string GetGroups = masterScript + @"
                                               var Groups = Store.GroupMetadata.models;

                                                var GroupOutput = [];

                                                for (group in Groups) {
                                                    if (isNaN(group)) {
                                                        continue;
                                                    }

                                                    //if(!(Groups[group].__x_id.toLowerCase().indexOf('@broadcast') >= 0))

                                                    var group_name = ggn(Groups[group].__x_id);

                                                    var i = Store.GroupMetadata.models[group].participants.models;
                                                    var ii = [];

                                                    for (p in i) {
                                                        if (isNaN(group)) {
                                                            continue;
                                                        };
                                                        var n = Store.GroupMetadata.models[group].participants.models[p].__x_id;
                                                        var m = ggn(Store.GroupMetadata.models[group].participants.models[p].__x_id);
                                                        if (n == null){
                                                            continue;
                                                        }
                                                        ii.push(n);

                                                    }
                                                    GroupOutput.push({
                                                        'Group' : {'Name' :group_name, 'Participants' : ii }
                                                    });

                                                }

                                                function ggn(pno) {
                                                    var contacts = window.Store.Contact.models;
                                                        for(var i in contacts){
                                                        if(isNaN(i)) {
                                                            continue;
                                                        }
                                                        if(pno == contacts[i].__x_id){
                                                            return (contacts[i].__x_name);
                                                        }
                                                    }
                                                }
                                                return JSON.stringify(GroupOutput);
                                                console.log(GroupOutput);
                                           ";

    }
}
