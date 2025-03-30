using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using UnityEngine;
using TNTPlus.Config;
using TNTPlus.Models;

namespace TNTPlus.Managers
{
    public static class MessageManager
    {
        private static MessageDataManager messageDataManager;

        public static void Initialize(MessageDataManager dataManager)
        {
            messageDataManager = dataManager;
        }

        public static void Say(UnturnedPlayer player, string text, EMessageType eMessageType)
        {
            var config = Main.Plugin.Instance.Configuration.Instance;

            if (config.UseNotification)
            {
                Send(player, text, eMessageType);
                return;
            }
            Say(player, text);
        }

        public static void Say(UnturnedPlayer player, string text)
        {
            ChatManager.serverSendMessage($"{text.Replace("{", "<").Replace("}", ">")}", Color.white, null, player.SteamPlayer(), EChatMode.SAY, null, true);
        }

        public static void SayGlobal(string text)
        {
            ChatManager.serverSendMessage($"{text.Replace("{", "<").Replace("}", ">")}", Color.white, null, null, EChatMode.GLOBAL, null, true);
        }

        public static void Send(UnturnedPlayer player, string text, EMessageType eMessageType)
        {
            messageDataManager.AddMessage(player, new Message { message = text, eMessage = eMessageType });
            UpdateMessage(player);
        }

        private static void UpdateMessage(UnturnedPlayer player)
        {
            ITransportConnection tplayer = player.Player.channel.GetOwnerTransportConnection();
            var messages = messageDataManager.GetMessages(player);

            EffectManager.sendUIEffect(7771, 7771, tplayer, true);

            for (int i = messages.Count - 1; i >= 0; i--)
            {
                int reversedIndex = messages.Count - 1 - i;
                UpdateMessageUI(tplayer, reversedIndex, messages[i]);
            }
        }

        public static void RemoveMessage(UnturnedPlayer player)
        {
            ITransportConnection tplayer = player.Player.channel.GetOwnerTransportConnection();
            var messages = messageDataManager.GetMessages(player);

            for (int i = 0; i < 5; i++)
            {
                EffectManager.sendUIEffectVisibility(7771, tplayer, true, $"tnt.messenger.message.{i}", false);
            }

            for (int i = messages.Count - 1; i >= 0; i--)
            {
                int reversedIndex = messages.Count - 1 - i;
                UpdateMessageUI(tplayer, reversedIndex, messages[i]);
            }
        }

        private static void UpdateMessageUI(ITransportConnection tplayer, int index, Message message)
        {
            EffectManager.sendUIEffectVisibility(7771, tplayer, true, $"tnt.messenger.message.{index}", true);
            EffectManager.sendUIEffectText(7771, tplayer, true, $"tnt.messenger.text.{index}", $"{message.message}");

            switch (message.eMessage)
            {
                case EMessageType.Default:
                    break;
                case EMessageType.Error:
                    EffectManager.sendUIEffectVisibility(7771, tplayer, true, $"tnt.messenger.error.{index}", true);
                    break;
                case EMessageType.Succes:
                    EffectManager.sendUIEffectVisibility(7771, tplayer, true, $"tnt.messenger.succes.{index}", true);
                    break;
                case EMessageType.Notification:
                    EffectManager.sendUIEffectVisibility(7771, tplayer, true, $"tnt.messenger.notification.{index}", true);
                    break;
                case EMessageType.Warning:
                    EffectManager.sendUIEffectVisibility(7771, tplayer, true, $"tnt.messenger.Warning.{index}", true);
                    break;
            }
        }
    }
}