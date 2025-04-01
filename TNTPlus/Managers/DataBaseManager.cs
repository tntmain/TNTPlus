using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using TNTPlus.Models;

namespace TNTPlus.Managers
{
    public class DataBaseManager
    {
        private string DataBaseFilePath;
        private List<PlayerInfo> playerInfo;
        public static DataBaseManager dataBaseManager;

        public DataBaseManager()
        {
            dataBaseManager = this;
            DataBaseFilePath = Path.Combine(Environment.CurrentDirectory, "Players", "Players.json");
            LoadDataBase();
        }

        public void LoadDataBase()
        {
            var directoryPath = Path.GetDirectoryName(DataBaseFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (File.Exists(DataBaseFilePath))
            {
                string json = File.ReadAllText(DataBaseFilePath);
                playerInfo = JsonConvert.DeserializeObject<List<PlayerInfo>>(json) ?? new List<PlayerInfo>();
            }
            else
            {
                playerInfo = new List<PlayerInfo>();
            }
        }

        public void SaveDataBase()
        {
            string json = JsonConvert.SerializeObject(playerInfo, Formatting.Indented);
            File.WriteAllText(DataBaseFilePath, json);
        }

        public bool IsPlayerRegistered(CSteamID steamId)
        {
            return playerInfo.Exists(p => p.SteamId == steamId);
        }

        public void RegisterPlayer(UnturnedPlayer player)
        {
            if (!IsPlayerRegistered(player.CSteamID))
            {
                playerInfo.Add(new PlayerInfo
                {
                    SteamId = player.CSteamID,
                    PlayerName = player.DisplayName,
                    SteamName = player.SteamName,

                });
                SaveDataBase();
            }
        }

        public PlayerInfo GetPlayerInfo(CSteamID steamId)
        {
            return playerInfo.Find(p => p.SteamId == steamId);
        }

        public static List<PlayerInfo> GetAllPlayers()
        {
            return dataBaseManager.playerInfo;
        }
    }
}