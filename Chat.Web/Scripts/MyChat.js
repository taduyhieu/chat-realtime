//function pointMessage(id) {
//    console.log(id);
//    document.getElementById('message-' + id).scrollIntoView({ behavior: 'smooth', block: 'center'});
//}
$(function () {

    var chatHub = $.connection.chatHub;
    $.connection.hub.start().done(function () {
        console.log("SignalR started");
        model.roomList();
        model.userList();
        model.userRoomList();
        model.userAllList();
        model.joinedRoom.id = 0;
        model.joinedRoom.name = "Home";
        model.joinRoom(null);
    });



    // Client Operations
    chatHub.client.updateChatRoom = function(roomView){
        model.userRoomList(roomView.Id);
    };
    chatHub.client.updateUser = function (userView) {
        model.userUpdatedOnline(userView.Id, userView.Device);
    };

    chatHub.client.pinMessage = function (messageView) {
        console.log("change pin");
        var message = new ChatMessage(
            messageView.Id,
            messageView.Content,
            messageView.Timestamp,
            messageView.From,
            null,
            messageView.Avatar,
            messageView.Stick,
        );
        model.pinMess(message);
    };

    chatHub.client.unpinMessage = function (messageView) {
        console.log("remove pin");
        model.unpinMess();
    };
    chatHub.client.newMessage = function (messageView) {

        var isMine = messageView.From === model.myName();
        var message = new ChatMessage(
            messageView.Id,
            messageView.Content,
            messageView.Timestamp,
            messageView.From,
            isMine,
            messageView.Avatar,
            0
        );
        model.chatMessages.push(message);
        $(".chat-body").animate({ scrollTop: $(".chat-body")[0].scrollHeight }, 1000);
    };

    chatHub.client.getProfileInfo = function (userId, userName, displayName, avatar) {
        model.myUserId(userId);
        model.myUserName(userName);
        model.myName(displayName);
        model.myAvatar(avatar);
    };

    chatHub.client.addUser = function (user) {
        model.userAdded(new ChatUser(
            user.Id, 
            user.Username,
            user.DisplayName,
            user.Avatar,
            user.CurrentRoom,
            user.Device
        ));
    };

    chatHub.client.removeUser = function (user) {
        model.userRemoved(user.Id);
    };

    $('#create-room').on('click', function () {

        var self = this;
        model.userCreateRoom.removeAll();
        for (var i = 0; i < model.allUsers.length; i++) {
            model.userCreateRoom.push(model.allUsers[i]);
        }
    });

    $('ul#room-list').on('click', 'a', function () {

        //var roomName = $(this).text();
        //model.joinedRoom = roomName;
        //model.joinRoom();
        //model.chatMessages.removeAll();
        //$("input#iRoom").val(roomName);
        //$("#joinedRoom").html("<b>" + roomName + "</b>" + "{<span id='userRoom'></span>}");
        //$('#room-list a').removeClass('active');

        $("#userReceiverId").val("");
        $(this).addClass('active');
    });

    chatHub.client.addChatRoom = function (room) {
        model.roomAdded(new ChatRoom(room.Id, room.Name));
    };

    chatHub.client.removeChatRoom = function (room) {
        model.roomDeleted(room.Id);
    };

    chatHub.client.onError = function (message) {
        model.serverInfoMessage(message);

        $("#errorAlert").removeClass("hidden").show().delay(5000).fadeOut(500);
    };

    chatHub.client.onRoomDeleted = function (message) {
        model.serverInfoMessage(message);
        $("#errorAlert").removeClass("hidden").show().delay(5000).fadeOut(500);

        // Join to the first room in list
        $("ul#room-list li a")[0].click();
    };

    var Model = function () {
        var self = this;
        self.message = ko.observable("");
        self.chatRooms = ko.observableArray([]);
        self.chatUsers = ko.observableArray([]);
        self.chatUserRooms = ko.observableArray([]);
        self.notChatUserRooms = ko.observableArray([]);
        self.allUsers = ko.observableArray([]);
        self.userSelected = ko.observableArray([]);
        self.userCreateRoom = ko.observableArray([]);
        self.chatMessages = ko.observableArray([]);
        self.pinnedMessages = {
            id: ko.observable(),
            content: ko.observable(),
            timestamp: ko.observable(),
            from: ko.observable(),
            isMine: ko.observable(),
            avatar: ko.observable(),
            stick: ko.observable(),
        };
        self.joinedRoom = {
            id: ko.observable(""),
            name: ko.observable(""),
        };
        self.serverInfoMessage = ko.observable("");
        self.myName = ko.observable("");
        self.myAvatar = ko.observable("");
        self.myUserId = ko.observable("");
        self.myUserName = ko.observable("");
        self.toUserId = ko.observable("");
        self.editPermissionRoom = ko.observable("");
        self.onEnter = function (d, e) {
            if (e.keyCode === 13) {
                if (self.message() != null && self.message() != "") {
                    self.sendNewMessage();
                }
            }
            return true;
        }
        self.filter = ko.observable("");
        self.forceUpdate  = ko.observable("");
        self.filteredChatUsers = ko.computed(function () {
            if (!self.filter()) {
                return ko.utils.arrayFilter(self.chatUsers(), function (user) {
                    var displayName = user.displayName().toLowerCase();
                    return displayName.includes("");
                });
            } else {
                if (self.forceUpdate()) {
                    return ko.utils.arrayFilter(self.chatUsers(), function (user) {
                        var displayName = user.displayName().toLowerCase();
                        return displayName.includes("");
                    });
                    self.forceUpdate(null);
                }
                else {
                    return ko.utils.arrayFilter(self.chatUsers(), function (user) {
                        var displayName = user.displayName().toLowerCase();
                        return displayName.includes(self.filter().toLowerCase());
                    });
                }
                
            }
        });
    };

    Model.prototype = {

        // Server Operations
        sendNewMessage: function () {
            var self = this;
            let room;
            if (self.joinedRoom.id == 0) {
                roomId = 0;
            }
            else {
                roomId = self.joinedRoom.id;
            }
            console.log(roomId);
            console.log(self.myUserId());
            console.log(self.toUserId());
            console.log(self.message());
            chatHub.server.send(roomId, self.myUserId(), self.toUserId(), self.message()).done(function (result) {
                let message = self.chatMessages.pop();
                let updateMessage = new ChatMessage(
                    result,
                    message.content(),
                    message.timestamp(),
                    message.from(),
                    message.isMine(),
                    message.avatar(),
                    0
                );
                self.chatMessages.push(updateMessage);
            });
            self.message("");
        },

        joinRoom: function (room) {
            var self = this;
            if (room) {
                $("input#iRoom").val(room.name());
                $("#editedRoomName").val(room.name());
                $("#editedRoomId").val(room.roomId());
                $("#joinedRoom").html("<b>" + room.name() + "</b>" + "{<span id='userRoom'></span>}");
            }
            else {
                $("input#iRoom").val("Trang chủ");
                $("#joinedRoom").html("<b>" + "Trang chủ" + "</b>");
            }
            $('#room-list a').removeClass('active');

            if (room) {
                self.joinedRoom.id = room.roomId() ? room.roomId() : 0 ;
                self.joinedRoom.name = room.name() ? room.name() : "";
            }
            
            self.chatMessages.removeAll();
            
            chatHub.server.join(self.joinedRoom.id).done(function () {
                self.userRoomList(self.joinedRoom.id);
                self.messageHistory(null, null);
            });
        },

        joinSingleRoom: function (toUser) {
            var self = this;
            self.joinedRoom.id = 0;
            self.joinedRoom.name = "";

            $("#joinedRoom").html("<b>" + toUser.displayName() + "</b>");
            $("#userReceiverId").val(toUser.id());

            self.toUserId(toUser.id());
            let toUserId = toUser.id();
            chatHub.server.join(0).done(function () {
                self.messageHistory(self.myUserId(), toUserId);
            });
        },

        pinMess: function (mess) {
            console.log(mess);
            var self = this;
            self.pinnedMessages.id(mess.id());
            self.pinnedMessages.content(mess.content());
            self.pinnedMessages.timestamp(mess.timestamp());
            self.pinnedMessages.from(mess.from());
            self.pinnedMessages.isMine(null);
            self.pinnedMessages.avatar(mess.avatar());
            self.pinnedMessages.stick(mess.stick());
        },

        unpinMess: function () {
            console.log("aaaaaaaaaa");
            var self = this;
            self.pinnedMessages.id(null);
            self.pinnedMessages.content(null);
            self.pinnedMessages.timestamp(null);
            self.pinnedMessages.from(null);
            self.pinnedMessages.isMine(null);
            self.pinnedMessages.avatar(null);
            self.pinnedMessages.stick(null);
        },

        stickMess: function (mess) {
            var self = this;
            chatHub.server.stickMess(mess.id()).done(function (result) {
                console.log("done pin");
                //self.pinnedMessages.id(mess.id());
                //self.pinnedMessages.content(mess.content());
                //self.pinnedMessages.timestamp(mess.timestamp());
                //self.pinnedMessages.from(mess.from());
                //self.pinnedMessages.isMine(mess.isMine());
                //self.pinnedMessages.avatar(mess.avatar());
                //self.pinnedMessages.stick(mess.stick());
            });
        },

        removeStickMessage: function () {
            var self = this;
            chatHub.server.removeStickMess(self.pinnedMessages.id()).done(function (result) {
                console.log("done unpin");
                //self.pinnedMessages.id(null);
                //self.pinnedMessages.content(null);
                //self.pinnedMessages.timestamp(null);
                //self.pinnedMessages.from(null);
                //self.pinnedMessages.isMine(null);
                //self.pinnedMessages.avatar(null);
                //self.pinnedMessages.stick(null);
            });
        },


        pointMessage: function (id) {
            document.getElementById('message-' + id()).scrollIntoView({ behavior: 'smooth', block: 'center' });
        },

        roomList: function () {
            var self = this;
            chatHub.server.getRooms(self.myUserId()).done(function (result) {
                self.chatRooms.removeAll();
                for (var i = 0; i < result.length; i++) {
                    self.chatRooms.push(new ChatRoom(result[i].RoomId, result[i].RoomName));
                }
            });
        },
      
        userList: function () {
            var self = this;
            chatHub.server.getOnlineUsers().done(function (result) {
                self.chatUsers.removeAll();
                for (var i = 0; i < result.length; i++) {
                    self.chatUsers.push(new ChatUser(
                        result[i].Id,
                        result[i].UserName,
                        result[i].DisplayName,
                        result[i].Avatar,
                        result[i].CurrentRoom,
                        result[i].Device)
                    );
                }
            });
            
        },

        userRoomList: function (roomId) {
            var self = this; 

            chatHub.server.getUsersRoom(roomId).done(function (result) {
                self.chatUserRooms.removeAll();
                $("#userRoom").html("");
                for (var i = 0; i < result.length; i++) {
                    $("#userRoom").append(result[i].DisplayName + ",");
                    self.chatUserRooms.push(new ChatUser(
                        result[i].UserId,
                        result[i].UserName,
                        result[i].DisplayName,
                        result[i].Avatar,
                        null,
                        null,
                        result[i].Role,
                    ))
                }
                self.editPermissionRoom = '0';
                $('#edit-room').html('');
                for (let i = 0; i < self.chatUserRooms().length; i++) {
                    if (self.chatUserRooms()[i].id() == self.myUserId() && self.chatUserRooms()[i].roomRole() == 1) {
                        self.editPermissionRoom = '1';
                    }
                }
                if (self.editPermissionRoom == '1') {
                    $('#edit-room').html('<button data-toggle="modal" data-target="#edit-room-modal" class="btnEditRoom glyphicon glyphicon-edit"></button>');
                }
                self.notChatUserRooms.removeAll();
                for (let i = 0; i < self.allUsers().length; i++) {
                    let user_1 = self.allUsers()[i];
                    let exist = false;
                    for (let j = 0; j < self.chatUserRooms().length; j++) {
                        let user_2 = self.chatUserRooms()[j];
                        if (user_1.id() == user_2.id()) {
                            exist = true;
                        }
                    }
                    if (!exist) {
                        self.notChatUserRooms.push(user_1);
                    }
                }
            });

        },

        userAllList: function () {
            var self = this;
            chatHub.server.getAllUsers().done(function (result) {
                self.allUsers.removeAll();
                for (var i = 0; i < result.length; i++) {
                    self.allUsers.push(new ChatUser(
                        result[i].Id,
                        result[i].UserName,
                        result[i].DisplayName,
                        result[i].Avatar,
                        null,
                        null))
                }
            });
        },

        createRoom: function () {
            var self = this;
            var name = $("#roomName").val();
            if (name != null && name != "") {
                let userSelected = "";
                for (var i = 0; i < self.userSelected().length; i++) {
                    userSelected += self.userSelected()[i].id() + ";" + self.userSelected()[i].userName() +"|";
                }
                userSelected += self.myUserId() + ";" + self.myUserName();
                chatHub.server.createRoom(name, userSelected).done(function (result) {
                    chatHub.server.getAllUsers().done(function (result) {
                        self.allUsers.removeAll();
                        self.userSelected.removeAll();
                        for (var i = 0; i < result.length; i++) {
                            self.allUsers.push(new ChatUser(
                                result[i].Id,
                                result[i].UserName,
                                result[i].DisplayName,
                                result[i].Avatar,
                                result[i].CurrentRoom,
                                result[i].Device))
                        }
                    });
                });
            }
            else {
                console.log("Tên phòng không được để trống!");
            }
            
        },

        editRoom: function () {
            var self = this;
            var roomId = $("#editedRoomId").val();
            if (roomId != null && roomId != "") {
                let userSelected = "";
                for (var i = 0; i < self.chatUserRooms().length; i++) {
                    userSelected += self.chatUserRooms()[i].id() + ";" + self.chatUserRooms()[i].userName() + "|";
                }
                chatHub.server.editRoom(roomId, userSelected).done(function (result) {
                    let userRoom = "";
                    for (var i = 0; i < self.chatUserRooms().length; i++) {
                        userRoom += self.chatUserRooms()[i].displayName() + ',';
                    }
                    $("#userRoom").text(userRoom);
                });
            }
            else {
                console.log("Tên phòng không được để trống!");
            }

        },

        closeRoom: function () {
            //var self = this;
            //var name = $("#roomName").val();
            //var roomId;
            //chatHub.server.createRoom(name).done(function (result) {
            //    roomId = result;
            //    for (var i = 0; i < self.allTempSelected().length; i++) {
            //        let userId = self.allTempSelected()[i].id();

            //        chatHub.server.addUserToRoom(userId, roomId).done(function (result) {
            //            console.log(result);
            //        });
            //    }
            //});
        },

        deleteRoom: function () {
            var self = this;
            chatHub.server.deleteRoom(self.joinedRoom.id);
        },

        messageHistory: function (fromUserId = null, toUserId = null) {
            var self = this;
            //set null stick message
            self.pinnedMessages.id(null);
            self.pinnedMessages.content(null);
            self.pinnedMessages.timestamp(null);
            self.pinnedMessages.from(null);
            self.pinnedMessages.isMine(null);
            self.pinnedMessages.avatar(null);
            self.pinnedMessages.stick(null);
            let roomId;
            if (fromUserId != null && toUserId != null) {
                roomId = 0;
            }
            else { 
                roomId = self.joinedRoom.id;
            }
            chatHub.server.getMessageHistory(roomId, fromUserId, toUserId).done(function (result) {
                self.chatMessages.removeAll();

                
                if (result) {
                    for (var i = 0; i < result.length; i++) {
                        var isMine = result[i].From == self.myName();
                        self.chatMessages.push(new ChatMessage(
                            result[i].Id,
                            result[i].Content,
                            result[i].Timestamp,
                            result[i].From,
                            isMine,
                            result[i].Avatar,
                            result[i].Stick)
                        )
                        if (result[i].Stick == 1) {
                            self.pinnedMessages.id(result[i].Id);
                            self.pinnedMessages.content(result[i].Content);
                            self.pinnedMessages.timestamp(result[i].Timestamp);
                            self.pinnedMessages.from(result[i].From);
                            self.pinnedMessages.isMine(isMine);
                            self.pinnedMessages.avatar(result[i].Avatar);
                            self.pinnedMessages.stick(result[i].Stick);
                        }
                    }
                    $(".chat-body").animate({ scrollTop: $(".chat-body")[0].scrollHeight }, 0);
                }
            }).fail(function (jqXHR, textStatus) {
                console.log(jqXHR);
                console.log(textStatus);
            });
        },

        roomAdded: function (room) {
            var self = this;
            self.chatRooms.push(room);
        },

        roomDeleted: function(id){
            var self = this;
            var temp;
            ko.utils.arrayForEach(self.chatRooms(), function (room) {
                if (room.roomId() == id)
                    temp = room;
            });
            self.chatRooms.remove(temp);
        },

        userAdded: function (user) {
            var self = this;
            if (self.chatUsers().length > 0) {
                for (var i = 0; i < self.chatUsers().length; i++) {
                    if (self.chatUsers()[i].id() == user.id) {
                        self.chatUsers()[i].device(user().device);
                        self.forceUpdate("1");
                        console.log(self.forceUpdate());
                        break;
                    }
                }
            }
        },

        
        userRemoved: function (id) {
            var self = this;
            if (self.chatUsers().length > 0) {
                for (var i = 0; i < self.chatUsers().length; i++) {
                    if (self.chatUsers()[i].id() == id) {
                        self.chatUsers()[i].device(null);
                        self.forceUpdate("1");
                        console.log(self.forceUpdate());
                        break;
                    }
                }
            }
        },

        userUpdatedOnline: function (id, device) {
            var self = this;
            console.log("update");
            //ko.utils.arrayForEach(self.chatUsers(), function (user) {
            //    console.log(user.id());
            //});

            console.log(self.chatUsers().length);
            if (self.chatUsers().length > 0) {
                for (var i = 0; i < self.chatUsers().length; i++) {
                    console.log(self.chatUsers()[i].id());
                    if (self.chatUsers()[i].id() == id) {
                        self.chatUsers()[i].device(device);
                        self.forceUpdate("1");
                        console.log(self.forceUpdate());
                        break;
                    }
                }
            }
            
        },

        addUserToRoom: function (user, type) {
            var self = this;
            if (type === 'create') {
                self.allUsers.remove(user);
                self.userSelected.push(user);
            }
            else if (type === 'edit') {
                self.notChatUserRooms.remove(user);
                self.chatUserRooms.push(user);
            }
            
        },
        removeUserSelectedRoom: function (user, type) {
            var self = this;
            if (type === 'create') {
                self.userSelected.remove(user);
                self.allUsers.push(user);
            }
            else if (type === 'edit') {
                self.notChatUserRooms.push(user);
                self.chatUserRooms.remove(user);
            }
        },
    };

    // Represent server data
    function ChatRoom(roomId, name) {
        var self = this;
        self.roomId = ko.observable(roomId);
        self.name = ko.observable(name);
    }

    function ChatUser(id = null, userName, displayName, avatar, currentRoom, device, roomRole = null) {
        var self = this;
        self.id = ko.observable(id);
        self.userName = ko.observable(userName);
        self.displayName = ko.observable(displayName);
        self.avatar = ko.observable(avatar);
        self.currentRoom = ko.observable(currentRoom);
        self.device = ko.observable(device);
        self.roomRole = ko.observable(roomRole);
    }
    
    function ChatMessage(id, content, timestamp, from, isMine, avatar, stick) {
        var self = this;
        self.id = ko.observable(id);
        self.content = ko.observable(content);
        self.timestamp = ko.observable(timestamp);
        self.from = ko.observable(from);
        self.isMine = ko.observable(isMine);
        self.avatar = ko.observable(avatar);
        self.stick = ko.observable(stick);
    }

    var model = new Model();
    ko.applyBindings(model);

});