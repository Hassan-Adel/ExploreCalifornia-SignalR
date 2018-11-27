using ExploreCalifornia.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExploreCalifornia
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
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
            var message = new ChatMessage
            {
                SenderName = name,
                Text = text,
                SentAt = DateTimeOffset.UtcNow
            };

            // we need to specify the name of the function that we're invoking on the Client. We'll create a function called ReceiveMessage, and then we'll pass any parameters we want to send to the Client.
            //Broadcast to all clients
            await Clients.All.SendAsync("RecieveMessage", message.SenderName, message.SentAt, message.Text);
        }
    }
}
