using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebWhatsappBotCore
{
    public static class Scripting
    {
        //Script for send a message by id, example id: 52111111111@c.us
        public static string SendMessageByID = @"
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
                                                        Chats[chat].sendMessage(message);
                                                        return true;
                                                    }
                                                }
                                                return false;
                                           ";



        //Script for send a message by id, example id: 52111111111@c.us
        public static string GetGroups = @"

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
                                                return GroupOutput;
                                                console.log(GroupOutput);
                                           ";

    }
}
