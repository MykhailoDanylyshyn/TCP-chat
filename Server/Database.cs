
using Microsoft.Data.SqlClient;

namespace Server;

public static class Database
{
    public static string connectionString =
        @"Server=DESKTOP-OVJTRN8\SQLEXPRESS;Database=ChatDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static void Initialize()
    {
        string masterConnection =
            @"Server=DESKTOP-OVJTRN8\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

        using (SqlConnection connection = new SqlConnection(masterConnection))
        {
            connection.Open();

            if (!DatabaseExists(connection))
            {
                using SqlCommand command = new SqlCommand(
                    "CREATE DATABASE ChatDB", connection);

                command.ExecuteNonQuery();
            }
        }

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string sql = @"
                IF OBJECT_ID('Chats', 'U') IS NULL
                CREATE TABLE Chats
                (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    CreatedAt DATETIME DEFAULT GETDATE()
                );

                IF OBJECT_ID('Users', 'U') IS NULL
                CREATE TABLE Users
                (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL DEFAULT N'Учасник',
                    IP NVARCHAR(45) NOT NULL UNIQUE
                );

                IF OBJECT_ID('ChatParticipants', 'U') IS NULL
                CREATE TABLE ChatParticipants
                (
                    ChatId INT NOT NULL,
                    UserId INT NOT NULL,
                    PRIMARY KEY (ChatId, UserId),
                    FOREIGN KEY (ChatId) REFERENCES Chats(Id),
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                IF OBJECT_ID('Messages', 'U') IS NULL
                CREATE TABLE Messages
                (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ChatId INT NOT NULL,
                    UserId INT NOT NULL,
                    Text NVARCHAR(MAX) NOT NULL,
                    SentAt DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY (ChatId) REFERENCES Chats(Id),
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );";

            using SqlCommand command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();

            using SqlCommand alterCommand = new SqlCommand(@"
                IF COL_LENGTH('Users', 'Name') IS NULL
                    ALTER TABLE Users ADD Name NVARCHAR(100)
                    NOT NULL DEFAULT N'Учасник';", connection);

            alterCommand.ExecuteNonQuery();
        }

        Console.WriteLine("Базу даних перевірено.");
    }

    private static bool DatabaseExists(SqlConnection connection)
    {
        using SqlCommand command = new SqlCommand(
            "SELECT DB_ID('ChatDB')", connection);

        return command.ExecuteScalar() != DBNull.Value;
    }
}