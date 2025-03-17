using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using trucki.Entities;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices
{
    public interface IChatService
    {
        // Save a message to the database
        Task<ChatMessage> SaveMessageAsync(
            string orderId, 
            string senderId, 
            string recipientId, 
            string text, 
            string senderName = null,
            List<string> imageUrls = null);

        // Get chat history for an order
        Task<ApiResponseModel<List<ChatMessage>>> GetChatHistoryAsync(string orderId);

        // Mark a message as read
        Task<ChatMessage> MarkMessageAsReadAsync(string messageId);

        // Get unread message count for a user
        Task<ApiResponseModel<int>> GetUnreadMessageCountAsync(string userId);
    }
}