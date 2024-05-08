using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNTPlus.Models
{
    public class MessageData
    {
        public List<Message> messages;
        public DateTime LastTime;

        public MessageData()
        {
            messages = new List<Message>();
            LastTime = DateTime.Now;
        }
    }
}
