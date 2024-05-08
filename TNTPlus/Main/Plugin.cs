using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Rocket.Core.Serialization;
using Rocket.Core.Steam;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Reflection;
using Rocket.Unturned.Events;
using SDG.NetTransport;
using Rocket.Unturned;
using UnityEngine;
using System.Collections;
using TNTPlus.Models;
using TNTPlus.Managers;

namespace TNTPlus.Main
{
    public class Plugin : RocketPlugin
    {
        public static Plugin Instance;

        public Dictionary<UnturnedPlayer, MessageData> messageData = new Dictionary<UnturnedPlayer, MessageData>();
        protected override void Load()
        {
            Instance = this;
            WorkshopDownloadConfig.getOrLoad().File_IDs.Add(3241782797);
            StartCoroutine(UpdateManager.SecondTickRoutine());
            UnturnedPlayerEvents.OnPlayerChatted += UnturnedPlayerEvents_OnPlayerChatted;
            UpdateManager.OnSecondTick += UpdateManager_OnSecondTick;
        }
        protected override void Unload()
        {
            Instance = null;
            StopCoroutine(UpdateManager.SecondTickRoutine());
            UpdateManager.OnSecondTick -= UpdateManager_OnSecondTick;
        }
        private void UnturnedPlayerEvents_OnPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        {
            MessageManager.Send(player, message, EMessageType.Succes);
        }
        private void UpdateManager_OnSecondTick()
        {
            foreach (var item in Provider.clients)
            {
                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(item);

                if (messageData.ContainsKey(unturnedPlayer))
                {
                    if (messageData[unturnedPlayer].messages.Count != 0 && (DateTime.Now - messageData[unturnedPlayer].LastTime).TotalSeconds >= 5)
                    {
                        messageData[unturnedPlayer].LastTime = DateTime.Now;
                        messageData[unturnedPlayer].messages.RemoveAt(0);
                        MessageManager.RemoveMessage(unturnedPlayer);
                    }
                }
            }
        }
    }
}
