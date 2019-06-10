using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot.Models
{
    public class ConversationData
    {
        public string ChannelId { get; set; }
        public string Timestamp { get; set; }
        public bool PromptedUserForName { get; set; }
    }
}
