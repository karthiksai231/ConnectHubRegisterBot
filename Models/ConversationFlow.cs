using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot.Models
{
    public class ConversationFlow
    {
        public Question LastQuestionAsked { get; set; } = Question.None;

        public enum Question
        {
            Name,
            DateOfBirth,
            Date,
            Registered,
            None, // Our last action did not involve a question.
        }
    }
}
