namespace Server;

class Program
{
    static void Main()
    {
        try
        {
            Database.Initialize();

            Console.WriteLine("Сервер запущено.");
            Console.WriteLine("Очікування підключень...");
            ChatServer.StartAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Помилка: " + ex.Message);
            
        }
    }
}