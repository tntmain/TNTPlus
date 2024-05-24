using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNTPlus.Managers
{
    public class PlayerManager
    {
        public static UnturnedPlayer GetUnturnedPlayer(string value)
        {
            UnturnedPlayer FindPlayer = UnturnedPlayer.FromName(value);
            if (FindPlayer == null)
            {
                CSteamID steamID = new CSteamID(Convert.ToUInt64(value));
                FindPlayer = UnturnedPlayer.FromCSteamID(steamID);
                return FindPlayer;
            }
            return FindPlayer;
        }
    }
}
