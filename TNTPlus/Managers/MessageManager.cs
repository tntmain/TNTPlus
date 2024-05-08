using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using TNTPlus.Main;
using TNTPlus.Models;
using UnityEngine;

namespace TNTPlus.Managers
{
    public class MessageManager
    {
        public static void Say(UnturnedPlayer player, string text)
        {
            ChatManager.serverSendMessage($"{text.Replace("{", "<").Replace("}", ">")}", Color.white, null, player.SteamPlayer(), EChatMode.SAY, null, true);
        }
        public static void Send(UnturnedPlayer player, string text, EMessageType eMessageType)
        {
            var msgData = Plugin.Instance.messageData;

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

            var msgData = Plugin.Instance.messageData;

            EffectManager.sendUIEffect(7771, 7771, Tplayer, true);

            for (int i = msgData[player].messages.Count - 1; i >= 0; i--)
            {
                int reversedIndex = msgData[player].messages.Count - 1 - i;

                EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.message.{reversedIndex}", true);
                EffectManager.sendUIEffectText(7771, Tplayer, true, $"tnt.messenger.text.{reversedIndex}", $"{msgData[player].messages[i].message}");

                switch (msgData[player].messages[i].eMessage)
                {
                    case EMessageType.Unknown:
                        break;
                    case EMessageType.Error:
                        EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.error.{reversedIndex}", true);
                        break;
                    case EMessageType.Succes:
                        EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.messenger.succes.{reversedIndex}", true);
                        break;
                    case EMessageType.Notification:
                        EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.notification.error.{reversedIndex}", true);
                        break;
                    case EMessageType.Warning:
                        EffectManager.sendUIEffectVisibility(7771, Tplayer, true, $"tnt.warning.error.{reversedIndex}", true);
                        break;
                    default:
                        break;
                }
            }
        }
        public static void RemoveMessage(UnturnedPlayer player)
        {
            var msgData = Plugin.Instance.messageData;

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
