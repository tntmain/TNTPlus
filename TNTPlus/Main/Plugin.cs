using System;
using System.Collections.Generic;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using SDG.Unturned;
using UnityEngine;
using TNTPlus.Models;
using TNTPlus.Managers;
using TNTPlus.Config;
using TNTPlus.Utilities;
using TNTPlus.Interfaces;
using TNTPlus.RequestHandlers;

namespace TNTPlus.Main
{
    public class Plugin : RocketPlugin<Configurations>
    {
        public static Plugin Instance;
        public static DataBaseManager dataBaseManager;

        private WebServer webServer;

        private MessageDataManager messageDataManager;
        private NavigationManager navigationManager;
        private UpdateManager updateManager;


        protected override void Load()
        {
            Instance = this;
            var config = Configuration.Instance;

            SubscribeToEvents();
            ConfigureWorkshop(config);
            InitializeManagers(config);
            InitializeWebServer(config);
            StartUpdateManager();

            Rocket.Core.Logging.Logger.Log($"TNTPlugins load success | version {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
            Rocket.Core.Logging.Logger.Log($"vk: https://vk.com/tntplugins");
        }

        protected override void Unload()
        {
            webServer?.Stop();
            UnsubscribeFromEvents();
            CleanupManagers();

            Instance = null;
            Rocket.Core.Logging.Logger.Log("Плагин TNTPlus выгружен.");
        }

        private void SubscribeToEvents()
        {
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
        }

        private void ConfigureWorkshop(Configurations config)
        {
            if (config.AutoLoadWorkShopMod)
            {
                WorkshopDownloadConfig.getOrLoad().File_IDs.Add(3241782797);
            }
        }

        private void InitializeManagers(Configurations config)
        {
            dataBaseManager = new DataBaseManager();

            messageDataManager = new MessageDataManager();
            MessageManager.Initialize(messageDataManager);

            if (config.NavigationManager)
            {
                navigationManager = new NavigationManager();
                navigationManager.Initialize();
            }
        }

        private void InitializeWebServer(Configurations config)
        {
            if (config.WerbServer)
            {
                webServer = new WebServer("http://localhost:8080/", config.ApiKey);
                webServer.Start();
                webServer.RegisterHandler(new AddSayHandler());
            }
        }

        private void StartUpdateManager()
        {
            updateManager = new UpdateManager();
            StartCoroutine(updateManager.SecondTickRoutine());
            updateManager.OnSecondTick += UpdateManager_OnSecondTick;
        }

        private void UnsubscribeFromEvents()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            updateManager.OnSecondTick -= UpdateManager_OnSecondTick;
        }
        private void CleanupManagers()
        {
            navigationManager?.Unload();
        }
        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            dataBaseManager.RegisterPlayer(player);
        }
        private void UpdateManager_OnSecondTick()
        {
            foreach (var item in Provider.clients)
            {
                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(item);
                messageDataManager.UpdateMessages(unturnedPlayer);
            }
        }
        public void RegisterWebHandler(IRequestHandler handler)
        {
            webServer.RegisterHandler(handler);
        }
    }
}