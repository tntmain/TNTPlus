using System;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using TNTPlus.Models;
using TNTPlus.Managers;

namespace TNTPlus.Managers
{
    public class MessageDataManager
    {
        private readonly Dictionary<UnturnedPlayer, MessageData> messageData = new Dictionary<UnturnedPlayer, MessageData>();

        public void AddMessage(UnturnedPlayer player, Message message)
        {
            if (!messageData.ContainsKey(player))
            {
                messageData[player] = new MessageData();
            }

            if (messageData[player].messages.Count >= 5)
            {
                messageData[player].messages.RemoveAt(0);
            }

            messageData[player].messages.Add(message);
            messageData[player].LastTime = DateTime.Now;
        }

        public void UpdateMessages(UnturnedPlayer player)
        {
            if (messageData.ContainsKey(player) && messageData[player].messages.Count != 0)
            {
                int messageCount = messageData[player].messages.Count;
                double timeToDisappear = Math.Max(6 - messageCount, 2);

                if ((DateTime.Now - messageData[player].LastTime).TotalSeconds >= timeToDisappear)
                {
                    messageData[player].LastTime = DateTime.Now;
                    messageData[player].messages.RemoveAt(0);
                    MessageManager.RemoveMessage(player);
                }
            }
        }

        public List<Message> GetMessages(UnturnedPlayer player)
        {
            if (messageData.ContainsKey(player))
            {
                return messageData[player].messages;
            }
            return new List<Message>();
        }
    }
}