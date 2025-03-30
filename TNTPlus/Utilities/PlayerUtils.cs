using Rocket.Unturned.Player;
using Steamworks;
using System;

namespace TNTPlus.Utilities
{
    public static class PlayerUtils
    {
        public static UnturnedPlayer GetUnturnedPlayer(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            UnturnedPlayer findPlayer = UnturnedPlayer.FromName(value);
            if (findPlayer != null)
            {
                return findPlayer;
            }

            if (ulong.TryParse(value, out ulong steamIdValue) && value.Length == 17)
            {
                CSteamID steamID = new CSteamID(steamIdValue);
                findPlayer = UnturnedPlayer.FromCSteamID(steamID);
                return findPlayer;
            }

            return null;
        }
    }
}