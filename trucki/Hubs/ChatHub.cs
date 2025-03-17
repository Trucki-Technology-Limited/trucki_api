using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using trucki.Entities;
using trucki.Interfaces.IServices;

namespace trucki.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        // Join a group (chat room) for an order
        public async Task JoinGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
            await Clients.Caller.SendAsync("JoinedGroup", orderId);
        }

        // Leave a group
        public async Task LeaveGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderId);
        }

        // Send a message to a group
        public async Task SendMessage(string messageJson)
        {
            // Get the current user ID
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            try
            {
                // Parse the message
                var messageData = JsonConvert.DeserializeObject<Dictionary<string, object>>(messageJson);

                // Extract message details
                string orderId = messageData["orderId"].ToString();
                string recipientId = messageData["recipientId"].ToString();
                string text = messageData["text"].ToString();
                string senderName = messageData.ContainsKey("senderName") ? messageData["senderName"].ToString() : "";

                // Optional fields
                List<string> imageUrls = null;

                if (messageData.ContainsKey("imageUrls") && messageData["imageUrls"] != null)
                {
                    imageUrls = JsonConvert.DeserializeObject<List<string>>(messageData["imageUrls"].ToString());
                }

                // Save message to database
                var savedMessage = await _chatService.SaveMessageAsync(
                    orderId,
                    userId,
                    recipientId,
                    text,
                    senderName,
                    imageUrls);

                // Create response message
                var responseMessage = new
                {
                    id = savedMessage.Id,
                    orderId = savedMessage.OrderId,
                    senderId = savedMessage.SenderId,
                    senderName = savedMessage.SenderName,
                    recipientId = savedMessage.RecipientId,
                    text = savedMessage.Text,
                    timestamp = savedMessage.Timestamp,
                    isRead = savedMessage.IsRead,
                    imageUrls = savedMessage.ImageUrls
                };

                // Convert to JSON
                string responseJson = JsonConvert.SerializeObject(responseMessage);

                // Send to the group (chat room)
                await Clients.Group(orderId).SendAsync("ReceiveMessage", responseJson);
            }
            catch (Exception ex)
            {
                throw new HubException($"Error sending message: {ex.Message}");
            }
        }

        // Mark a message as read
        public async Task MarkMessageAsRead(string messageId)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            try
            {
                // Update the message in the database
                var updatedMessage = await _chatService.MarkMessageAsReadAsync(messageId);
                if (updatedMessage != null)
                {
                    // Notify the sender that the message was read
                    var readStatus = new
                    {
                        messageId = updatedMessage.Id,
                        orderId = updatedMessage.OrderId,
                        isRead = true,
                        readBy = userId,
                        readAt = DateTime.UtcNow
                    };

                    // Convert to JSON
                    string statusJson = JsonConvert.SerializeObject(readStatus);

                    // Send to the group (chat room)
                    await Clients.Group(updatedMessage.OrderId).SendAsync("MessageReadStatus", statusJson);
                }
            }
            catch (Exception ex)
            {
                throw new HubException($"Error marking message as read: {ex.Message}");
            }
        }

        // Handle connection events
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User {userId} connected with connection ID: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User {userId} disconnected. Connection ID: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}