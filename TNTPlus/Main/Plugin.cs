using System;
using System.Collections.Generic;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Rocket.Unturned.Events;
using UnityEngine;
using TNTPlus.Models;
using TNTPlus.Managers;
using TNTPlus.Config;
using Rocket.Unturned;
using TNTPlus.Utilities;
using System.Threading.Tasks;
namespace TNTPlus.Main
{
    public class Plugin : RocketPlugin<Configurations>
    {
        public static Plugin Instance;
        public Dictionary<UnturnedPlayer, MessageData> messageData = new Dictionary<UnturnedPlayer, MessageData>();
        protected override void Load()
        {
            Instance = this;
            if (Configuration.Instance.AutoLoadWorkShopMod)
            {
                WorkshopDownloadConfig.getOrLoad().File_IDs.Add(3241782797);
            }
            if (Configuration.Instance.ShowReputationChangeNotification)
            {
                U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            }
            StartCoroutine(UpdateManager.SecondTickRoutine());
            UpdateManager.OnSecondTick += UpdateManager_OnSecondTick;
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            //player.Player.setPluginWidgetFlag(EPluginWidgetFlags., false);
        }

        protected override void Unload()
        {
            Instance = null;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            StopCoroutine(UpdateManager.SecondTickRoutine());
            UpdateManager.OnSecondTick -= UpdateManager_OnSecondTick;
        }
        private void UpdateManager_OnSecondTick()
        {
            foreach (var item in Provider.clients)
            {
                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(item);

                if (messageData.ContainsKey(unturnedPlayer) && messageData[unturnedPlayer].messages.Count != 0)
                {
                    int messageCount = messageData[unturnedPlayer].messages.Count;

                    double timeToDisappear = Math.Max(6 - messageCount, 2);

                    if ((DateTime.Now - messageData[unturnedPlayer].LastTime).TotalSeconds >= timeToDisappear)
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
