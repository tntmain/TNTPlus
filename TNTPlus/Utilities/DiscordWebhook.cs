using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TNTPlus.Utilities
{
    public class DiscordWebhook
    {
        private readonly string _webhookUrl;

        public DiscordWebhook(string webhookUrl)
        {
            _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl), "Webhook URL cannot be null or empty.");
        }

        public async Task SendMessageAsync(string message, string username = null, string avatarUrl = null, object embed = null)
        {
            var payload = new
            {
                content = message,
                username = username,
                avatar_url = avatarUrl,
                embeds = embed != null ? new[] { embed } : null
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var data = Encoding.UTF8.GetBytes(jsonPayload);

            var request = (HttpWebRequest)WebRequest.Create(_webhookUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            try
            {
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(data, 0, data.Length);
                }

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (var responseStream = new StreamReader(response.GetResponseStream()))
                    {
                        string result = await responseStream.ReadToEndAsync();
                        //Console.WriteLine($"Response from Discord: {result}");
                    }
                }
            }
            catch (WebException e)
            {
                using (var errorResponse = (HttpWebResponse)e.Response)
                {
                    using (var responseStream = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        string errorText = await responseStream.ReadToEndAsync();
                        //Console.WriteLine($"Error sending message to Discord Webhook: {errorText}");
                    }
                }
            }
        }

        private static async Task SendDiscordWebhook(string message, string url, string username = null, string avatarUrl = null, object embed = null)
        {
            DiscordWebhook discordWebhook = new DiscordWebhook(url);

            await discordWebhook.SendMessageAsync(message, username, avatarUrl, embed);

            //Console.WriteLine("Message sent to Discord Webhook.");
        }

        public static void SendMessageDiscord()
        {
            Task.Run(async () =>
            {
                var embed = new
                {
                    title = "Test Embed",
                    description = "This is a test embed",
                    color = 16711680, // Red color in decimal
                    fields = new[] 
                    {
                        new 
                        { 
                            name = "Field 1", 
                            value = "Value 1", 
                            inline = true 
                        },
                        new 
                        { 
                            name = "Field 2", 
                            value = "Value 2", 
                            inline = true 
                        } 
                    }
                };

                await SendDiscordWebhook("test", 
                    "https://discord.com/api/webhooks/", 
                    "Test Bot", 
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQgUXCd6lQ5wcVj71RMJ2E8OQgCwQxuzH2ow9H8K1uXbA&s]",
                    embed);
            });
        }
    }
}

