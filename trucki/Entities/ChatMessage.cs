using System;
using System.Collections.Generic;

namespace trucki.Entities
{
    public class ChatMessage : BaseClass
    {
        public string OrderId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string RecipientId { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public List<string> ImageUrls { get; set; }
        
        // Navigation property to CargoOrders
        public CargoOrders Order { get; set; }
    }
}