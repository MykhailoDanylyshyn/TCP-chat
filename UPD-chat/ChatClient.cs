
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace UPD_chat;

public class ChatClient
{
    private const string ServerIP = "26.176.184.159";
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
        using UdpClient client = new UdpClient();

        Request request = new Request
        {
            Action = action,
            Name = name,
            UserName = userName,
            ChatId = chatId,
            IP = ip,
            Text = text
        };

        byte[] data = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(request));

        await client.SendAsync(data, data.Length, ServerIP, Port);

        using CancellationTokenSource timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(2));

        UdpReceiveResult result =
            await client.ReceiveAsync(timeout.Token);

        string json = Encoding.UTF8.GetString(result.Buffer);

        return JsonSerializer.Deserialize<Response>(json)
            ?? throw new Exception("Некоректна відповідь сервера.");
    }
}