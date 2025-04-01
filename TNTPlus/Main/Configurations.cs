using System.Collections.Generic;
using Rocket.API;
namespace TNTPlus.Config
{
    public class Configurations : IRocketPluginConfiguration, IDefaultable
    {
        public bool AutoLoadWorkShopMod { get; set; }
        public bool UseNotification { get; set; }
        public bool NavigationManager { get; set; }
        public List<string> RoadNames { get; set; }
        public bool WebServer { get; set; }
        public string ApiKey { get; set; }
        public string Host { get; set; }


        public void LoadDefaults()
        {
            AutoLoadWorkShopMod = true;
            UseNotification = true;
            NavigationManager = false;
            RoadNames = new List<string> { "Segment_", "Road_Line_", "Road_Tee_" };
            WebServer = true;
            ApiKey = "SecretKey";
            Host = "http://localhost:8080/";
        }
    }
}
