using Newtonsoft.Json;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Net;
using System.Threading.Tasks;
using TNTPlus.Interfaces;
using TNTPlus.Models;
using TNTPlus.Utilities;
using UnityEngine;

namespace TNTPlus.RequestHandlers
{
    public class SayHandler : IRequestHandler
    {
        public string Endpoint => "/say";

        public async Task<object> Handle(HttpListenerRequest request)
        {
            var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding);
            string body = await reader.ReadToEndAsync();
            Rocket.Core.Logging.Logger.Log($"Тело запроса на /say: {body}");

            var data = JsonConvert.DeserializeObject<PlayerRequest>(body);
            string playerId = data?.PlayerId?.ToLower();
            string text = data?.Text;

            if (!string.IsNullOrEmpty(playerId) && !string.IsNullOrEmpty(text))
            {
                if (playerId == "all")
                {
                    TaskDispatcher.QueueOnMainThread(() =>
                    {
                        UnturnedChat.Say(text, Color.white);
                        Rocket.Core.Logging.Logger.Log($"Сообщение '{text}' отправлено всем игрокам");
                    });
                    return new { Status = "Success", Message = $"Сообщение '{text}' отправлено всем игрокам!" };
                }
                else
                {
                    UnturnedPlayer player = null;
                    try
                    {
                        player = PlayerUtils.GetUnturnedPlayer(playerId);
                    }
                    catch (Exception ex)
                    {
                        Rocket.Core.Logging.Logger.LogWarning($"Ошибка при поиске игрока {playerId}: {ex.Message}");
                        return new { Status = "Error", Message = $"Некорректный playerId: {playerId}" };
                    }

                    if (player != null)
                    {
                        TaskDispatcher.QueueOnMainThread(() =>
                        {
                            UnturnedChat.Say(player, text, Color.white);
                            Rocket.Core.Logging.Logger.Log($"Сообщение '{text}' отправлено игроку {playerId}");
                        });
                        return new { Status = "Success", Message = $"Сообщение '{text}' отправлено игроку {playerId}!" };
                    }
                    else
                    {
                        Rocket.Core.Logging.Logger.LogWarning($"Игрок с playerId {playerId} не найден.");
                        return new { Status = "Error", Message = $"Игрок с playerId {playerId} не найден!" };
                    }
                }
            }
            else
            {
                return new { Status = "Error", Message = "Укажите playerId и text!" };
            }
        }
    }
}

