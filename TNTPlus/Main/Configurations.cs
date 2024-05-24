using System;
using System.Collections.Generic;
using Rocket.API;
using SDG.Unturned;
namespace TNTPlus.Config
{
    public class Configurations : IRocketPluginConfiguration, IDefaultable
    {
        public bool UseNotification { get; set; }
        public bool AutoLoadWorkShopMod { get; set; }
        public bool ShowReputationChangeNotification { get; set; }

        public void LoadDefaults()
        {
            AutoLoadWorkShopMod = true;
            UseNotification = true;
            ShowReputationChangeNotification = false;
        }
    }
}
