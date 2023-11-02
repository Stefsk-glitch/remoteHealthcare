using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doctor
{
    public class ChatMessage
    {
        public string MessageText { get; set; }
        public string Sender { get; set; }
        public DateTime Timestamp { get; set; }

        public ChatMessage(string messageText)
        {
            MessageText = messageText;
          
            Timestamp = DateTime.Now;
        }
    }

}
