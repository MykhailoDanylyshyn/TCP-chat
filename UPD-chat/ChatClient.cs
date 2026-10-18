
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace UPD_chat;

public class ChatClient
{
    private const string ServerIP = "127.0.0.1";
    private const int Port = 5000;

    public class Request
    {
        public string Action { get; set; } = "";
        public string Name { get; set; } = "";
        public string UserName { get; set; } = "";
        public int ChatId { get; set; }
        public string IP { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class ChatInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class Response
    {
        public string Message { get; set; } = "";
        public List<ChatInfo> Chats { get; set; } = new();
        public List<string> Participants { get; set; } = new();
        public List<string> Messages { get; set; } = new();
    }

    public static async Task<Response> SendRequest(
        string action,
        string name = "",
        int chatId = 0,
        string ip = "",
        string text = "",
        string userName = "")
    {
        using TcpClient client = new TcpClient();

        await client.ConnectAsync(ServerIP, Port);

        using NetworkStream stream = client.GetStream();

        using StreamReader reader = new StreamReader(
            stream, Encoding.UTF8, false, 1024, true);

        using StreamWriter writer = new StreamWriter(
            stream, new UTF8Encoding(false), 1024, true);

        Request request = new Request
        {
            Action = action,
            Name = name,
            UserName = userName,
            ChatId = chatId,
            IP = ip,
            Text = text
        };

        string json = JsonSerializer.Serialize(request);

        Console.WriteLine("Надсилаємо запит: " + json);

        await writer.WriteLineAsync(json);
        await writer.FlushAsync();

        string? responseJson = await reader.ReadLineAsync();

        Console.WriteLine("Відповідь сервера: " + responseJson);

        if (string.IsNullOrEmpty(responseJson))
            throw new Exception("Сервер не надіслав відповідь.");

        return JsonSerializer.Deserialize<Response>(responseJson)
            ?? throw new Exception("Некоректна відповідь сервера.");
    }
}
