using System.Net.Sockets;
using ChatClient;

namespace ChatClient;

public class Client
{
    public async Task RunAsync(string host, int port)
    {
        using var client = new System.Net.Sockets.TcpClient();
        try
        {
            client.Connect(host, port);
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);

            string userName;
            while (true)
            {
                Console.WriteLine("Enter your name:");
                userName = Console.ReadLine();
                if (string.IsNullOrEmpty(userName)) continue;

                await writer.WriteLineAsync(userName);
                await writer.FlushAsync();

                string? response = await reader.ReadLineAsync();

                if (response == "Welcome")
                {
                    Console.WriteLine($"Welcome to the chat, {userName}!");
                    string? usersInChat = await reader.ReadLineAsync();
                    Console.WriteLine(usersInChat);
                    break;
                }
                else if (response == "NameTaken")
                {
                    Console.WriteLine("This name is already taken! Enter another name.");
                }
            }

            var receiveTask = ReceiveMessageAsync(reader);
            var sendTask = SendMessageAsync(writer, userName);
            await Task.WhenAny(receiveTask, sendTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            client.Close();
        }
    }

    private async Task SendMessageAsync(StreamWriter writer, string? userName)
    {
        Console.WriteLine("Enter your message:");
        while (true)
        {
            string? message = Console.ReadLine();
            if (string.IsNullOrEmpty(message)) continue;
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
        }
    }

    private async Task ReceiveMessageAsync(StreamReader reader)
    {
        while (true)
        {
            try
            {
                string? message = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(message)) continue;
                Console.WriteLine(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                break;
            }
        }
    }
}
