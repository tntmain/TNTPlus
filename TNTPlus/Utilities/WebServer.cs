using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNTPlus.Interfaces;

namespace TNTPlus.Utilities
{
    public class WebServer
    {
        private readonly HttpListener listener;
        private bool isRunning;
        private readonly Dictionary<string, IRequestHandler> handlers;
        private readonly string validApiKey;
        private readonly CancellationTokenSource cts;

        public WebServer(string prefix = "http://localhost:8080/", string apiKey = "YourSecretKey123")
        {
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            handlers = new Dictionary<string, IRequestHandler>();
            validApiKey = apiKey;
            cts = new CancellationTokenSource();
            isRunning = false;
        }

        public void Start()
        {
            if (!isRunning)
            {
                isRunning = true;
                listener.Start();
                Task.Run(() => HandleRequests(cts.Token));
                Rocket.Core.Logging.Logger.Log($"Веб-сервер запущен на {listener.Prefixes.FirstOrDefault()}");
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                isRunning = false;
                cts.Cancel();
                listener.Stop();
                listener.Close();
                cts.Dispose();
                Rocket.Core.Logging.Logger.Log("Веб-сервер остановлен.");
            }
        }

        public void RegisterHandler(IRequestHandler handler)
        {
            if (!handlers.ContainsKey(handler.Endpoint))
            {
                handlers[handler.Endpoint] = handler;
                Rocket.Core.Logging.Logger.Log($"Зарегистрирован обработчик для {handler.Endpoint}");
            }
            else
            {
                Rocket.Core.Logging.Logger.LogWarning($"Обработчик для {handler.Endpoint} уже существует.");
            }
        }

        private async Task HandleRequests(CancellationToken cancellationToken)
        {
            while (isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await listener.GetContextAsync().WithCancellation(cancellationToken);
                    var request = context.Request;
                    var response = context.Response;

                    Rocket.Core.Logging.Logger.Log($"Получен запрос: {request.HttpMethod} {request.Url}");

                    if (request.HttpMethod == "OPTIONS")
                    {
                        SendCorsResponse(response);
                        continue;
                    }

                    string path = request.Url.AbsolutePath;

                    if (request.HttpMethod == "GET" && path == "/")
                    {
                        string serverStartedResponse = "Server started";
                        byte[] buffer = Encoding.UTF8.GetBytes(serverStartedResponse);
                        response.StatusCode = 200;
                        response.ContentType = "text/plain";
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                        response.Close();
                        continue;
                    }

                    string apiKey = request.Headers["X-API-Key"];
                    if (string.IsNullOrEmpty(apiKey) || apiKey != validApiKey)
                    {
                        Rocket.Core.Logging.Logger.LogWarning($"Неверный или отсутствующий API-ключ: {apiKey ?? "не указан"}");
                        response.StatusCode = 403;
                        string errorResponse = JsonConvert.SerializeObject(new { Status = "Error", Message = "Неверный или отсутствующий API-ключ", Code = 403 });
                        byte[] errorBuffer = Encoding.UTF8.GetBytes(errorResponse);
                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        response.ContentType = "application/json";
                        await response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length, cancellationToken);
                        response.Close();
                        continue;
                    }

                    if (handlers.ContainsKey(path) && request.HttpMethod == "POST")
                    {
                        var handler = handlers[path];
                        var result = await handler.Handle(request);

                        string jsonResponse = JsonConvert.SerializeObject(result);
                        byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        response.ContentType = "application/json";
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                    }
                    else
                    {
                        Rocket.Core.Logging.Logger.LogWarning($"Неподдерживаемый запрос: {request.HttpMethod} {request.Url}");
                        response.StatusCode = 404;
                        byte[] notFoundBuffer = Encoding.UTF8.GetBytes("Not Found");
                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        await response.OutputStream.WriteAsync(notFoundBuffer, 0, notFoundBuffer.Length, cancellationToken);
                    }

                    response.Close();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Rocket.Core.Logging.Logger.LogError($"Ошибка веб-сервера: {ex.Message}");
                }
            }
        }

        private void SendCorsResponse(HttpListenerResponse response)
        {
            response.StatusCode = 200;
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-API-Key");
            response.Close();
        }
    }
}