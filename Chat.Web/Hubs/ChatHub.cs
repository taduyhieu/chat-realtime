using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Chat.Web.Models.ViewModels;
using Chat.Web.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using System.Web.Script.Serialization;

namespace Chat.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        #region Properties
        /// <summary>
        /// List of online users
        /// </summary>
        public readonly static List<UserViewModel> _Connections = new List<UserViewModel>();

        /// <summary>
        /// List of all users
        /// </summary>
        public readonly static List<UserViewModel> _Users = new List<UserViewModel>();

        /// <summary>
        /// List of available chat rooms
        /// </summary>
        private readonly static List<RoomViewModel> _Rooms = new List<RoomViewModel>();

        /// <summary>
        /// Mapping SignalR connections to application users.
        /// (We don't want to share connectionId)
        /// </summary>
        private readonly static Dictionary<string, string> _ConnectionsMap = new Dictionary<string, string>();
        #endregion

        public int Send(int roomId, string fromUserId, string toUserId, string message)
        {
            if(roomId != 0 && roomId != null)
            {
                return SendToRoom(roomId, message);
            }
            else
            {
                return SendPrivate(message, fromUserId, toUserId);
            }
        }

        public int SendPrivate(string message, string fromUserId, string toUserId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userSender = db.Users.Where(u => u.Id == fromUserId).FirstOrDefault();
                    var userReceiver = db.Users.Where(u => u.Id == toUserId).FirstOrDefault();

                    // Create and save message in database
                    Message msg = new Message()
                    {
                        Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty),
                        Timestamp = DateTime.Now.Ticks.ToString(),
                        FromUser = userSender,
                        ToUser = userReceiver,
                    };
                    db.Messages.Add(msg);
                    db.SaveChanges();
                    int idMess = msg.Id; 
                    var messageViewModel = Mapper.Map<Message, MessageViewModel>(msg);
                    try
                    {
                        string userId;

                        if (_ConnectionsMap.TryGetValue(userReceiver.UserName, out userId))
                        {
                            // Who is the sender;
                            var sender = _Connections.Where(u => u.UserName == IdentityName).First();

                            // Send the message
                            Clients.Client(userId).newMessage(messageViewModel);
                            Clients.Caller.newMessage(messageViewModel);
                        }
                        else
                        {
                            Clients.Caller.newMessage(messageViewModel);
                        }
                    }
                    catch (Exception)
                    {
                        Clients.Caller.newMessage(messageViewModel);
                    }

                    return idMess;
                }
            }
            catch (Exception)
            {
                Clients.Caller.onError("Message not send!");
            }
            return 0;
        }

        public int SendToRoom(int roomId, string message)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                    var room = db.Rooms.Where(r => r.Id == roomId).FirstOrDefault();

                    // Create and save message in database
                    Message msg = new Message()
                    {
                        Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty),
                        Timestamp = DateTime.Now.Ticks.ToString(),
                        FromUser = user,
                        ToRoom = room
                    };
                    db.Messages.Add(msg);
                    db.SaveChanges();
                    int idMess = msg.Id;
                    // Broadcast the message
                    var messageViewModel = Mapper.Map<Message, MessageViewModel>(msg);
                    Clients.Group(roomId.ToString()).newMessage(messageViewModel);
                    return idMess;
                }
            }
            catch (Exception)
            {
                Clients.Caller.onError("Message not send!");
            }
            return 0;
        }

        public void Join(int roomId)
        {
            try
            {
                var user = _Connections.Where(u => u.UserName == IdentityName).FirstOrDefault();
                if (user.CurrentRoomId != roomId)
                {
                    // Remove user from others list
                    if (!string.IsNullOrEmpty(user.CurrentRoomId.ToString()))
                        Clients.OthersInGroup(user.CurrentRoomId.ToString()).removeUser(user);

                    // Join to new chat room
                    Leave(user.CurrentRoomId);
                    Groups.Add(Context.ConnectionId, roomId.ToString());
                    user.CurrentRoomId = roomId;

                    // Tell others to update their list of users
                    Clients.OthersInGroup(roomId.ToString()).addUser(user);
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("You failed to join the chat room!" + ex.Message);
            }
        }

        private void Leave(int roomId)
        {
            Groups.Remove(Context.ConnectionId, roomId.ToString());
        }

        public int CreateRoom(string roomName, string userSelected)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    
                    // Create and save chat room in database
                    var user = db.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                    var room = new Room()
                    {
                        Name = roomName,
                        UserAccount = user
                    };
                    var result = db.Rooms.Add(room);
                    db.SaveChanges();
                    room.Id = room.Id;
                    if (room != null)
                    {
                        // Update room list
                        var roomViewModel = Mapper.Map<Room, RoomViewModel>(room);
                        _Rooms.Add(roomViewModel);


                        char[] spearatorUser = { '|' };
                        char[] spearatorElement = { ';' };
                        String[] arrayUserSelected = userSelected.Split(spearatorUser);

                        for (var i = 0; i < arrayUserSelected.Length; i++)
                        {
                            String[] User = arrayUserSelected[i].Split(spearatorElement);
                            if(User[0] == user.Id)
                            {
                                this.AddUserToRoom(User[0], room.Id, 1);
                            }
                            else
                            {
                                this.AddUserToRoom(User[0], room.Id);
                            }
                            

                            string userId;
                            if (_ConnectionsMap.TryGetValue(User[1], out userId))
                            {
                                Clients.Client(userId).addChatRoom(roomViewModel);
                            }
                        }
                    }
                    return room.Id;
                } //using
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("Couldn't create chat room: " + ex.Message);
            }
            return 0;
        }

        public void EditRoom(int roomId, string userEditSelected)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {

                    // Create and save chat room in database
                    var room = db.Rooms.Where(r => r.Id == roomId).FirstOrDefault();
                    if (room != null)
                    {
                        var roomViewModel = Mapper.Map<Room, RoomViewModel>(room);
                        //_Rooms.Add(roomViewModel);


                        char[] spearatorUser = { '|' };
                        char[] spearatorElement = { ';' };
                        String[] arrayUserSelected = userEditSelected.Split(spearatorUser);
                        String existUsers = null;
                        String newUsers = null;
                        for (var i = 0; i < arrayUserSelected.Length; i++)
                        {
                            String[] User = arrayUserSelected[i].Split(spearatorElement);
                            newUsers += User[0] + "|";
                        }

                        string userId;
                        foreach (UserRoom userRoom in db.UserRooms.Where(m => m.RoomId == roomId).ToList())
                        {
                            existUsers += userRoom.UserId + "|";

                            if (!newUsers.ToLower().Contains(userRoom.UserId))
                            {
                                this.DeleteUserToRoom(userRoom.UserId, room.Id);
                                if (_ConnectionsMap.TryGetValue(userRoom.User.UserName, out userId))
                                {
                                    Clients.Client(userId).removeChatRoom(roomViewModel);
                                }
                            } else {
                                if (_ConnectionsMap.TryGetValue(userRoom.User.UserName, out userId))
                                {
                                    Clients.Client(userId).updateChatRoom(roomViewModel);
                                }
                            }
                            
                        }

                        for (var i = 0; i < arrayUserSelected.Length; i++)
                        {
                            String[] User = arrayUserSelected[i].Split(spearatorElement);
                            if (!existUsers.ToLower().Contains(User[0])) {
                                this.AddUserToRoom(User[0], room.Id);
                                if (_ConnectionsMap.TryGetValue(User[1], out userId))
                                {
                                    Clients.Client(userId).addChatRoom(roomViewModel);
                                }
                            }
                        }
                    }
                } //using
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("Couldn't edit chat room: " + ex.Message);
            }
        }

        public void StickMess(int Id)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var message = db.Messages.Where(m => m.Id == Id).FirstOrDefault();
                    if (message.ToRoom != null && message.ToRoom.Id > 0)
                    {
                        var listMessages = db.Messages.Where(m => m.ToRoom.Id == message.ToRoom.Id && m.Id != Id).ToList();

                        foreach (Message m in listMessages)
                        {
                            m.Stick = 0;
                        }
                    } else
                    {
                        var listMessages_1 = db.Messages.Where(m => m.FromUser.Id == message.FromUser.Id && m.ToUser.Id == message.ToUser.Id && m.Id != Id).ToList();

                        foreach (Message m in listMessages_1)
                        {
                            m.Stick = 0;
                        }

                        var listMessages_2= db.Messages.Where(m => m.FromUser.Id == message.ToUser.Id && m.ToUser.Id == message.FromUser.Id && m.Id != Id).ToList();

                        foreach (Message m in listMessages_2)
                        {
                            m.Stick = 0;
                        }
                    }
                    

                    message.Stick = 1;
                    db.SaveChanges();

                    var messageViewModel = Mapper.Map<Message, MessageViewModel>(message);
                    if (message.ToUser != null && message.ToUser.Id != "")
                    {
                        string userId;

                        if (_ConnectionsMap.TryGetValue(message.ToUser.UserName.ToString(), out userId))
                        {
                            Clients.Client(userId).pinMessage(messageViewModel);
                        }

                        if (_ConnectionsMap.TryGetValue(message.FromUser.UserName.ToString(), out userId))
                        {
                            Clients.Client(userId).pinMessage(messageViewModel);
                        }

                    } else
                    {
                        Clients.Group(message.ToRoom.Id.ToString()).pinMessage(messageViewModel);
                    }
                    
                }//using
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("Couldn't pin message in room: " + ex.Message);
            }
        }

        public void RemoveStickMess(int Id)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var message = db.Messages.Where(m => m.Id == Id).FirstOrDefault();
                    
                    message.Stick = 0;
                    db.SaveChanges();

                    var messageViewModel = Mapper.Map<Message, MessageViewModel>(message);

                    if (message.ToUser != null && message.ToUser.Id != "")
                    {
                        string userId;

                        if (_ConnectionsMap.TryGetValue(message.ToUser.UserName.ToString(), out userId))
                        {
                            Clients.Client(userId).unpinMessage(messageViewModel);
                        }

                        if (_ConnectionsMap.TryGetValue(message.FromUser.UserName.ToString(), out userId))
                        {
                            Clients.Client(userId).unpinMessage(messageViewModel);
                        }
                    }
                    else
                    {
                        Clients.Group(message.ToRoom.Id.ToString()).unpinMessage(messageViewModel);
                    }

                }//using
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("Couldn't remove pin message in room: " + ex.Message);
            }
        }
        public void DeleteRoom(int roomId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    // Delete from database
                    var room = db.Rooms.Where(r => r.Id == roomId && r.UserAccount.UserName == IdentityName).FirstOrDefault();
                    db.Rooms.Remove(room);
                    db.SaveChanges();

                    // Delete from list
                    var roomViewModel = _Rooms.First<RoomViewModel>(r => r.Id == roomId);
                    _Rooms.Remove(roomViewModel);

                    // Move users back to Lobby
                    Clients.Group(roomId.ToString()).onRoomDeleted(string.Format("Room {0} has been deleted.\nYou are now moved to the Lobby!", roomId));

                    // Tell all users to update their room list
                    Clients.All.removeChatRoom(roomViewModel);
                }
            }
            catch (Exception)
            {
                Clients.Caller.onError("Can't delete this chat room.");
            }
        }

        public IEnumerable<MessageViewModel> GetMessageHistory(int roomId, string fromUserId, string toUserId)
        {
            using (var db = new ApplicationDbContext())
            {
                if (roomId != 0 && roomId != null)
                {
                    var messageHistory = db.Messages.Where(m => m.ToRoom.Id == roomId)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(20)
                    .AsEnumerable()
                    .Reverse()
                    .ToList();
                    return Mapper.Map<IEnumerable<Message>, IEnumerable<MessageViewModel>>(messageHistory);
                }
                else if (fromUserId != "" && toUserId != "")
                {
                    var messageHistory = db.Messages.Where(m => (m.FromUserId == fromUserId && m.ToUserId == toUserId) || (m.FromUserId == toUserId && m.ToUserId == fromUserId))
                    .OrderByDescending(m => m.Timestamp)
                    .Take(20)
                    .AsEnumerable()
                    .Reverse()
                    .ToList();
                    return Mapper.Map<IEnumerable<Message>, IEnumerable<MessageViewModel>>(messageHistory);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public IEnumerable<UserRoomViewModel> GetRooms(string userId)
        {
            List<UserRoomViewModel> _UsersRoom = new List<UserRoomViewModel>();
            using (var db = new ApplicationDbContext())
            {
                // First run?
                if (_UsersRoom.Count == 0)
                {
                    foreach (UserRoom userRoom in db.UserRooms.ToList())
                    {
                        UserRoomViewModel UserRoomViewModel = Mapper.Map<UserRoom, UserRoomViewModel>(userRoom);
                        _UsersRoom.Add(UserRoomViewModel);
                    }
                }
            }
            return _UsersRoom.Where(u => u.UserId == userId);
        }

        public IEnumerable<UserViewModel> GetOnlineUsers()
        {
            return _Users;
        }

        public IEnumerable<UserRoomViewModel> GetUsersRoom(int roomId)
        {
            List<UserRoomViewModel> _UsersRoom = new List<UserRoomViewModel>();
            using (var db = new ApplicationDbContext())
            {
                // First run?
                if (_UsersRoom.Count == 0)
                {
                    foreach (UserRoom userRoom in db.UserRooms.ToList())
                    {
                        UserRoomViewModel UserRoomViewModel = Mapper.Map< UserRoom ,  UserRoomViewModel  >(userRoom);
                        _UsersRoom.Add(UserRoomViewModel);
                    }
                }
            }
            return _UsersRoom.Where(u => u.RoomId == roomId);
        }

        public IEnumerable<UserRoomViewModel> GetNotUsersRoom(int roomId)
        {
            List<UserRoomViewModel> _UsersRoom = new List<UserRoomViewModel>();
            using (var db = new ApplicationDbContext())
            {
                // First run?
                if (_UsersRoom.Count == 0)
                {
                    foreach (UserRoom userRoom in db.UserRooms.ToList())
                    {
                        UserRoomViewModel UserRoomViewModel = Mapper.Map<UserRoom, UserRoomViewModel>(userRoom);
                        _UsersRoom.Add(UserRoomViewModel);
                    }
                }
            }
            return _UsersRoom.Where(u => u.RoomId != roomId); ;
        }
        public void AddUserToRoom(string userId, int roomId, int role = 0)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    // Create and save chat room in database
                    // var user = db.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                    var user = new UserRoom()
                    {
                        UserId = userId,
                        RoomId = roomId,
                        Role = role,
                    };
                    db.UserRooms.Add(user);
                    db.SaveChanges();
                }//using
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("Couldn't create chat room: " + ex.Message);
            }
        }

        public void DeleteUserToRoom(string userId, int roomId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.UserRooms.Where(r => r.RoomId == roomId && r.UserId == userId).FirstOrDefault();
                    db.UserRooms.Remove(user);
                    db.SaveChanges();
                }//using
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("Couldn't delete in room: " + ex.Message);
            }
        }

        public IEnumerable<UserViewModel> GetAllUsers()
        {
            List<UserViewModel> _Users = new List<UserViewModel>();
            using (var db = new ApplicationDbContext())
            {
                //First run?
                if (_Users.Count == 0)
                {
                    foreach (var user in db.Users)
                    {
                        var userViewModel = Mapper.Map<ApplicationUser, UserViewModel>(user);
                        _Users.Add(userViewModel);
                    }
                }
            }

            return _Users.Where(u => u.UserName != IdentityName).ToList();
        }

        #region OnConnected/OnDisconnected
        public override Task OnConnected()
        {
            using (var db = new ApplicationDbContext())
            {
                // First run?
                if (_Users.Count == 0)
                {
                    foreach (ApplicationUser user in db.Users.ToList())
                    {
                        UserViewModel userViewModel = Mapper.Map<ApplicationUser, UserViewModel>(user);
                        _Users.Add(userViewModel);
                    }
                }
            }


            using (var db = new ApplicationDbContext())
            {
                try
                {
                    var user = db.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();

                    var userViewModel = Mapper.Map<ApplicationUser, UserViewModel>(user);
                    userViewModel.Device = GetDevice();
                    userViewModel.CurrentRoomId = 0;
                    userViewModel.CurrentRoomName = "";

                    var tempUser = _Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                    _Users.Remove(tempUser);

                    _Users.Add(userViewModel);

                    Clients.All.UpdateUser(userViewModel);
                    _Connections.Add(userViewModel);
                    _ConnectionsMap.Add(IdentityName, Context.ConnectionId);

                    Clients.Caller.getProfileInfo(user.Id, user.UserName, user.DisplayName, user.Avatar);
                }
                catch (Exception ex)
                {
                    Clients.Caller.onError("OnConnected:" + ex.Message);
                }
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            
            try
            {
                var tempUser = _Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                _Users.Remove(tempUser);

                tempUser.Device = "";
                _Users.Add(tempUser);
                Clients.All.UpdateUser(tempUser);
                var user = _Connections.Where(u => u.UserName == IdentityName).FirstOrDefault();
                _Connections.Remove(user);

                // Tell other users to remove you from their list
                Clients.OthersInGroup(user.CurrentRoomId.ToString()).removeUser(user);

                // Remove mapping
                _ConnectionsMap.Remove(user.UserName);
                
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("OnDisconnected: " + ex.Message);
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            //var tempUser = _Users.Where(u => u.Username == IdentityName).FirstOrDefault();
            //_Users.Remove(tempUser);

            var user = _Connections.Where(u => u.UserName == IdentityName).FirstOrDefault();
            Clients.Caller.getProfileInfo(user.Id, user.DisplayName, user.Avatar);


            //_Users.Add(user);
            return base.OnReconnected();
        }
        #endregion

        private string IdentityName
        {
            get { return Context.User.Identity.Name; }
        }

        private string GetDevice()
        {
            string device = Context.Headers.Get("Device");

            if (device != null && (device.Equals("Desktop") || device.Equals("Mobile")))
                return device;

            return "Web";
        }
    }
}