using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNTPlus.Models
{
    public class PlayerInfo
    {
        public CSteamID SteamId { get; set; }
        public string PlayerName { get; set; }
        public string CharacterName { get; set; }
        public string SteamName { get; set; }
    }
}
