using ExploreCalifornia.Models;
using ExploreCalifornia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExploreCalifornia
{
    public class ChatHub : Hub
    {
        private readonly IChatRoomService _chatRoomService;
        public ChatHub(IChatRoomService chatRoomService)
        {
            _chatRoomService = chatRoomService;
        }
        public override async Task OnConnectedAsync()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                //Authenticated agents don't need a room
                await base.OnConnectedAsync();
                return;
            }
            //To add a connection to a group you need to specify the connection ID and a group name as a string. 
            //The connection ID is just Context.ConnectionId for the current connection ID
            //Since SignalR doesn't persist or keep track of the groups themselves, 
            //we'll need to add a service that can remember the groups that have been created. 
            //Since this is a chat application, we'll call each group a chat room and create a chat room service. 
            var roomId = await _chatRoomService.CreateRoom(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            //Will sent to the ckient that was connected only
            await Clients.Caller.SendAsync(
                "RecieveMessage", 
                "Explore California Team", 
                DateTimeOffset.UtcNow, 
                "Hello! What can we help you with ?");
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(string name, string text)
        {
            var roomId = await _chatRoomService.GetRoomForConnectionId(Context.ConnectionId);
            var message = new ChatMessage
            {
                SenderName = name,
                Text = text,
                SentAt = DateTimeOffset.UtcNow
            };
            await _chatRoomService.AddMessage(roomId, message);

            // we need to specify the name of the function that we're invoking on the Client. We'll create a function called ReceiveMessage, and then we'll pass any parameters we want to send to the Client.
            //Broadcast to all clients
            await Clients.Group(roomId.ToString()).SendAsync("RecieveMessage", message.SenderName, message.SentAt, message.Text);
        }

        public async Task SetRoomName(string visitorName)
        {
            var roomName = $"Chat with {visitorName} from web";
            var roomId = await _chatRoomService.GetRoomForConnectionId(Context.ConnectionId);
            await _chatRoomService.SetRoomName(roomId, roomName);
        }

        [Authorize]
        public async Task JoinRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
                throw new ArgumentException("Invalid room Id");
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        }

        [Authorize]
        public async Task LeaveRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
                throw new ArgumentException("Invalid room Id");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        }
    }
}
