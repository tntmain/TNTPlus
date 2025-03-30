using System;
using System.Collections.Generic;
using Rocket.API;
using SDG.Unturned;
using TNTPlus.Managers;
using TNTPlus.Utilities;
namespace TNTPlus.Config
{
    public class Configurations : IRocketPluginConfiguration, IDefaultable
    {
        public bool AutoLoadWorkShopMod { get; set; }
        public bool UseNotification { get; set; }
        public bool NavigationManager { get; set; }
        public List<string> RoadNames { get; set; }
        public bool WerbServer { get; set; }
        public string ApiKey { get; set; }


        public void LoadDefaults()
        {
            AutoLoadWorkShopMod = true;
            UseNotification = true;
            NavigationManager = false;
            RoadNames = new List<string> { "Segment_", "Road_Line_", "Road_Tee_" };
            WerbServer = true;
            ApiKey = "0e73ca83-3b05-4e03-9bfd-178b16034990";
        }
    }
}
