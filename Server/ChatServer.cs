
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server;

public class ChatServer
{
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

    public static async Task StartAsync()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();

        Console.WriteLine("TCP-сервер запущено. Порт: 5000");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();

            Console.WriteLine("Клієнт підключився!");

            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            using NetworkStream stream = client.GetStream();

            using StreamReader reader = new StreamReader(
                stream, Encoding.UTF8, false, 1024, true);

            using StreamWriter writer = new StreamWriter(
                stream, new UTF8Encoding(false), 1024, true);

            try
            {
                Console.WriteLine("Очікуємо запит...");

                string? json = await reader.ReadLineAsync();

                Console.WriteLine("Отримано запит: " + json);

                if (string.IsNullOrEmpty(json))
                    return;

                Request request = JsonSerializer.Deserialize<Request>(json)
                    ?? throw new Exception("Некоректний запит.");

                Response response = new Response();

                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint!)
                    .Address.MapToIPv4().ToString();

                if (request.Action == "CreateChat")
                {
                    using SqlConnection connection =
                        new SqlConnection(Database.connectionString);

                    await connection.OpenAsync();

                    string sql = @"
                        INSERT INTO Chats (Name)
                        OUTPUT INSERTED.Id
                        VALUES (@Name)";

                    using SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@Name", request.Name);

                    int chatId = Convert.ToInt32(
                        await command.ExecuteScalarAsync());

                    int userId = await GetOrCreateUser(
                        connection, clientIP, request.UserName);

                    sql = @"
                        INSERT INTO ChatParticipants (ChatId, UserId)
                        VALUES (@ChatId, @UserId)";

                    using SqlCommand participantCommand =
                        new SqlCommand(sql, connection);

                    participantCommand.Parameters.AddWithValue("@ChatId", chatId);
                    participantCommand.Parameters.AddWithValue("@UserId", userId);

                    await participantCommand.ExecuteNonQueryAsync();

                    response.Message = "Чат створено.";
                }
                else if (request.Action == "GetChats")
                {
                    using SqlConnection connection =
                        new SqlConnection(Database.connectionString);

                    await connection.OpenAsync();

                    string sql = @"
                        SELECT DISTINCT c.Id, c.Name
                        FROM Chats c
                        JOIN ChatParticipants cp ON cp.ChatId = c.Id
                        JOIN Users u ON u.Id = cp.UserId
                        WHERE u.IP = @IP
                        ORDER BY c.Id";

                    using SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@IP", clientIP);

                    using SqlDataReader dbReader =
                        await command.ExecuteReaderAsync();

                    while (await dbReader.ReadAsync())
                    {
                        response.Chats.Add(new ChatInfo
                        {
                            Id = dbReader.GetInt32(0),
                            Name = dbReader.GetString(1)
                        });
                    }

                    response.Message = "Список чатів отримано.";
                }
                else if (request.Action == "GetChatData")
                {
                    using SqlConnection connection =
                        new SqlConnection(Database.connectionString);

                    await connection.OpenAsync();

                    string sql = @"
                        SELECT u.Name
                        FROM ChatParticipants cp
                        JOIN Users u ON u.Id = cp.UserId
                        WHERE cp.ChatId = @ChatId";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ChatId", request.ChatId);

                        using SqlDataReader dbReader =
                            await command.ExecuteReaderAsync();

                        while (await dbReader.ReadAsync())
                        {
                            response.Participants.Add(dbReader.GetString(0));
                        }
                    }

                    sql = @"
                        SELECT u.Name, m.Text, m.SentAt
                        FROM Messages m
                        JOIN Users u ON u.Id = m.UserId
                        WHERE m.ChatId = @ChatId
                        ORDER BY m.Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ChatId", request.ChatId);

                        using SqlDataReader dbReader =
                            await command.ExecuteReaderAsync();

                        while (await dbReader.ReadAsync())
                        {
                            response.Messages.Add(
                                $"[{dbReader.GetDateTime(2):dd.MM.yyyy HH:mm}] " +
                                $"{dbReader.GetString(0)}: {dbReader.GetString(1)}");
                        }
                    }

                    response.Message = "Дані чату отримано.";
                }
                else if (request.Action == "AddParticipant")
                {
                    if (!IPAddress.TryParse(request.IP, out _))
                    {
                        response.Message = "Некоректна IP-адреса.";
                    }
                    else
                    {
                        using SqlConnection connection =
                            new SqlConnection(Database.connectionString);

                        await connection.OpenAsync();

                        int userId = await GetOrCreateUser(
                            connection, request.IP, request.UserName);

                        string sql = @"
                            IF NOT EXISTS (
                                SELECT 1
                                FROM ChatParticipants
                                WHERE ChatId = @ChatId AND UserId = @UserId
                            )
                            INSERT INTO ChatParticipants (ChatId, UserId)
                            VALUES (@ChatId, @UserId)";

                        using SqlCommand command = new SqlCommand(sql, connection);
                        command.Parameters.AddWithValue("@ChatId", request.ChatId);
                        command.Parameters.AddWithValue("@UserId", userId);

                        await command.ExecuteNonQueryAsync();

                        response.Message = "Учасника додано до чату.";
                    }
                }
                else if (request.Action == "SendMessage")
                {
                    using SqlConnection connection =
                        new SqlConnection(Database.connectionString);

                    await connection.OpenAsync();

                    int userId = await GetOrCreateUser(
                        connection, clientIP, request.UserName);

                    string sql = @"
                        IF EXISTS (
                            SELECT 1
                            FROM ChatParticipants
                            WHERE ChatId = @ChatId AND UserId = @UserId
                        )
                        BEGIN
                            INSERT INTO Messages (ChatId, UserId, Text)
                            VALUES (@ChatId, @UserId, @Text)
                        END";

                    using SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@ChatId", request.ChatId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Text", request.Text);

                    int affected = await command.ExecuteNonQueryAsync();

                    response.Message = affected > 0
                        ? "Повідомлення надіслано."
                        : "Ви не є учасником цього чату.";
                }
                else
                {
                    response.Message = "Невідома команда.";
                }

                string responseJson = JsonSerializer.Serialize(response);

                Console.WriteLine("Відповідь: " + responseJson);

                await writer.WriteLineAsync(responseJson);
                await writer.FlushAsync();

                Console.WriteLine("Відповідь надіслано клієнту.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ПОМИЛКА: " + ex);

                try
                {
                    Response errorResponse = new Response
                    {
                        Message = "Помилка сервера: " + ex.Message
                    };

                    string errorJson = JsonSerializer.Serialize(errorResponse);

                    await writer.WriteLineAsync(errorJson);
                    await writer.FlushAsync();
                }
                catch (Exception sendEx)
                {
                    Console.WriteLine("Помилка відповіді: " + sendEx.Message);
                }
            }
        }
    }

    private static async Task<int> GetOrCreateUser(
        SqlConnection connection, string ip, string userName = "")
    {
        string sql = "SELECT Id FROM Users WHERE IP = @IP";

        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IP", ip);

        object? result = await command.ExecuteScalarAsync();

        if (result != null)
        {
            if (userName != "")
            {
                sql = "UPDATE Users SET Name = @Name WHERE IP = @IP";

                using SqlCommand update = new SqlCommand(sql, connection);
                update.Parameters.AddWithValue("@Name", userName);
                update.Parameters.AddWithValue("@IP", ip);

                await update.ExecuteNonQueryAsync();
            }

            return Convert.ToInt32(result);
        }

        sql = @"
            INSERT INTO Users (Name, IP)
            OUTPUT INSERTED.Id
            VALUES (@Name, @IP)";

        using SqlCommand insert = new SqlCommand(sql, connection);

        insert.Parameters.AddWithValue(
            "@Name", string.IsNullOrWhiteSpace(userName) ? ip : userName);

        insert.Parameters.AddWithValue("@IP", ip);

        return Convert.ToInt32(await insert.ExecuteScalarAsync());
    }
}
