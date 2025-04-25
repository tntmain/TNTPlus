using System;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using TNTPlus.Managers;
using TNTPlus.Config;
using TNTPlus.Utilities;
using TNTPlus.Interfaces;
using TNTPlus.RequestHandlers;

namespace TNTPlus.Main
{
    public class TNTPlus : RocketPlugin<Configurations>
    {
        public static TNTPlus Core;
        public static TplayersData tplayersData;

        private WebServer webServer;

        private MessageDataManager messageDataManager;
        public NavigationManager navigationManager;


        protected override void Load()
        {
            Core = this;
            var config = Configuration.Instance;

            SubscribeToEvents();
            ConfigureWorkshop(config);
            InitializeManagers(config);
            InitializeWebServer(config);
            StartUpdateManager();

            Rocket.Core.Logging.Logger.Log($"TNTPlus load success | version {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
            Rocket.Core.Logging.Logger.Log($"vk: https://vk.com/tntplugins");
        }

        protected override void Unload()
        {
            webServer?.Stop();
            UnsubscribeFromEvents();
            CleanupManagers();

            Core = null;
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
            tplayersData = new TplayersData();

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
            if (config.ApiKey == "SecretKey")
            {
                config.ApiKey = Guid.NewGuid().ToString();
                Configuration.Save();
            }
            if (config.WebServer)
            {
                try
                {
                    webServer = new WebServer(config.Host, config.ApiKey);
                    webServer.Start();
                    webServer.RegisterHandler(new SayHandler());
                }
                catch (Exception ex)
                {
                    Rocket.Core.Logging.Logger.LogError($"Ошибка запуска веб-сервера: {ex.Message}");
                }
            }
        }

        private void StartUpdateManager()
        {
            StartCoroutine(UpdateManager.SecondTickRoutine());
            UpdateManager.OnSecondTick += UpdateManager_OnSecondTick;
        }

        private void UnsubscribeFromEvents()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            UpdateManager.OnSecondTick -= UpdateManager_OnSecondTick;
        }
        private void CleanupManagers()
        {
            navigationManager?.Unload();
        }
        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            tplayersData.RegisterPlayer(player);
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