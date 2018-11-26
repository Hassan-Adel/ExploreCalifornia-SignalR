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
