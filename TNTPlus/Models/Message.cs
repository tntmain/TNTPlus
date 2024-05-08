using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TNTPlus.Main.Plugin;

namespace TNTPlus.Models
{
    public class Message
    {
        public string message;
        public EMessageType eMessage;

        public Message()
        {
            message = "";
            eMessage = EMessageType.Unknown;
        }
    }
}
