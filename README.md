# TNTPlus - Расширенная платформа для плагинов Unturned

TNTPlus — это удобный плагин для Unturned, предоставляющий разработчикам полезные инструменты для работы с навигацией, сообщениями, обновлениями и взаимодействием с внешними сервисами.

## 📌 Основные возможности

### 🚀 Система навигации (NavigationManager)
Позволяет строить маршруты между двумя точками на карте с использованием алгоритма A*.

🔹 Как это работает:
- Плагин анализирует дороги на карте и создаёт сетку точек.
- Маршрут прокладывается от ближайшей точки старта до ближайшей точки финиша.
- Игроку отображаются ближайшие 3 точки маршрута, которые обновляются по мере движения.

💡 **Пример использования**:
```csharp
public void StartRoute(UnturnedPlayer player, Vector3 endPoint)
{
    Vector3 start = player.Position;
    TNTPlus.Core.navigationManager.BuildRoute(player, start, endPoint, (success, message) => {
        if (success)
        {
            UnturnedChat.Say(player, "Маршрут готов!", Color.green);
        }
        else
        {
            UnturnedChat.Say(player, "Ошибка: " + message, Color.red);
        }
    });
}
```

### ✉️ Система сообщений (MessageManager)
Позволяет отправлять сообщения игрокам с различными типами и цветами:
- 🟢 **Success** – Успешное действие (зелёный)
- 🔴 **Error** – Ошибка (красный)
- 🔵 **Notification** – Уведомление (синий)
- 🟡 **Warning** – Предупреждение (жёлтый)
- ⚫ **Default** – Обычное сообщение (чёрный)

💡 **Пример использования**:
```csharp
MessageManager.Say(player, "Всё получилось!", EMessageType.Success);
```

### ⏳ Менеджер обновлений (UpdateManager)
Позволяет выполнять действия каждую секунду с помощью события `OnSecondTick`.

💡 **Пример использования**:
```csharp
TNTPlus.Core.updateManager.OnSecondTick += TickEverySecond;

private void TickEverySecond()
{
    UnturnedChat.Say("Прошла секунда!");
}
```

### 🌍 Веб-сервер (WebServer)
Встроенный HTTP-сервер, позволяющий взаимодействовать с сервером Unturned через API. Поддерживает аутентификацию через API-ключи и возможность добавления кастомных обработчиков.

🔹 **Проверка работы веб-сервера**:
```bash
curl http://localhost:8080/
```
Ответ: `"Server started"`

🔹 **Отправка сообщения через API**:
```bash
curl -X POST http://localhost:8080/say \
-H "Content-Type: application/json" \
-H "X-API-Key: YourSecretKey" \
-d '{"PlayerId": "all", "Text": "Всем привет!"}'
```

### 🎁 Пример кастомного эндпоинта (выдача предметов)
```csharp
public class GiveItemHandler : IRequestHandler
{
    public string Endpoint => "/giveitem";

    public async Task<object> Handle(HttpListenerRequest request)
    {
        var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<GiveItemRequest>(body);

        if (!string.IsNullOrEmpty(data?.PlayerId) && data.ItemId > 0)
        {
            UnturnedPlayer player = PlayerUtils.GetUnturnedPlayer(data.PlayerId);
            if (player != null)
            {
                Item item = new Item(data.ItemId, true);
                TaskDispatcher.QueueOnMainThread(() => player.GiveItem(item, data.Amount));
                return new { Status = "Success", Message = $"Предмет {data.ItemId} выдан игроку {data.PlayerId}." };
            }
            return new { Status = "Error", Message = "Игрок не найден." };
        }
        return new { Status = "Error", Message = "Некорректные данные." };
    }
}
```

🔹 **Тестовый запрос**:
```bash
curl -X POST http://localhost:8080/giveitem \
-H "Content-Type: application/json" \
-H "X-API-Key: YourSecretKey" \
-d '{"PlayerId": "76561198000000000", "ItemId": 14, "Amount": 3}'
```

## 🔧 Установка
1. Скачайте последнюю версию TNTPlus из [релизов](https://github.com/YourRepo/TNTPlus/releases).
2. Поместите `.dll` в папку `Plugins` на вашем сервере.
3. Перезапустите сервер Unturned.

## 🎯 Заключение
TNTPlus — это удобный инструмент, который помогает реализовывать сложные игровые механики. Навигация, система сообщений, обновления и API-взаимодействие позволяют расширять возможности плагинов без лишних сложностей.

