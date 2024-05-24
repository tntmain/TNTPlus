using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using TNTPlus.Config;
using TNTPlus.Main;
using TNTPlus.Models;
using UnityEngine;
using static TNTPlus.Main.Plugin;

namespace TNTPlus.Managers
{
    public class MessageManager
    {
        public static void Say(UnturnedPlayer player, string text, EMessageType eMessageType)
        {
            var Config = Instance.Configuration.Instance;

            if (Config.UseNotification)
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

        public static void Send(UnturnedPlayer player, string text, EMessageType eMessageType)
        {
            var msgData = Instance.messageData;

            if (!msgData.ContainsKey(player))
            {
                msgData[player] = new MessageData();
            }

            if (msgData[player].messages.Count >= 5)
            {
                msgData[player].messages.RemoveAt(0);
            }

            msgData[player].messages.Add(new Message() { message = text, eMessage = eMessageType });
            msgData[player].LastTime = DateTime.Now;

            UpdateMessage(player);

        }

        private static void UpdateMessage(UnturnedPlayer player)
        {
            ITransportConnection Tplayer = player.Player.channel.GetOwnerTransportConnection();

            var msgData = Instance.messageData;

            EffectManager.sendUIEffect(7771, 7771, Tplayer, true);

            for (int i = msgData[player].messages.Count - 1; i >= 0; i--)
            {
                int reversedIndex = msgData[player].messages.Count - 1 - i;

                EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.message.{reversedIndex}", true);
                EffectManager.sendUIEffectText(7771, Tplayer, true, $"tnt.messenger.text.{reversedIndex}", $"{msgData[player].messages[i].message}");

                switch (msgData[player].messages[i].eMessage)
                {
                    case EMessageType.Default:
                        break;
                    case EMessageType.Error:
                        EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.error.{reversedIndex}", true);
                        break;
                    case EMessageType.Succes:
                        EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.succes.{reversedIndex}", true);
                        break;
                    case EMessageType.Notification:
                        EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.notification.{reversedIndex}", true);
                        break;
                    case EMessageType.Warning:
                        EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.Warning.{reversedIndex}", true);
                        break;
                    default:
                        break;
                }
            }
        }
        public static void RemoveMessage(UnturnedPlayer player)
        {
            var msgData = Instance.messageData;

            ITransportConnection Tplayer = player.Player.channel.GetOwnerTransportConnection();
            for (int i = 0; i < 5; i++)
            {
                EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.message.{i}", false);
            }
            for (int i = msgData[player].messages.Count - 1; i >= 0; i--)
            {
                int reversedIndex = msgData[player].messages.Count - 1 - i;
                EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.message.{reversedIndex}", true);
                EffectManager.sendUIEffectText(7771, Tplayer, true, $"tnt.messenger.text.{reversedIndex}", $"{msgData[player].messages[i].message}");
            }
        }
    }
}
