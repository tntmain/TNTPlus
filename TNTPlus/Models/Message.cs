namespace TNTPlus.Models
{
    public class Message
    {
        public string message;
        public EMessageType eMessage;

        public Message()
        {
            message = "";
            eMessage = EMessageType.Default;
        }
    }
}
