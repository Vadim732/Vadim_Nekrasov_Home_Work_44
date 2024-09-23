using System.Net.Sockets;
using ChatClient;

namespace ChatClient;

public class Client
{
    public async Task ConnectServerAsync()
    {
        while (true)
        {
            Console.WriteLine("Enter the server IP-address:");
            string? ip = Console.ReadLine();

            Console.WriteLine("Enter server port:");
            string? connectionPort = Console.ReadLine();

            if (int.TryParse(connectionPort, out int port))
            {
                bool connected = await CheckConnectAsync(ip, port);

                if (connected)
                {
                    Console.WriteLine("Successful connection to the server! c:");
                    await RunAsync(ip, port);
                    return;
                }
                else
                {
                    Console.WriteLine("Failed to connect to the server! :с \nIf you want to exit, type \"stop\". \nIf you want to try connecting again, type \"more\".");
                    string? gg = Console.ReadLine();
                    if (gg?.ToLower() == "stop")
                    {
                        return;
                    }
                    else if (gg?.ToLower() == "more")
                    {
                        continue;
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid IP address or port! Be more attentive =з");
            }
        }
    }

    private async Task<bool> CheckConnectAsync(string? host, int port)
    {
        using var client = new System.Net.Sockets.TcpClient();
        try
        {
            await client.ConnectAsync(host, port);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task RunAsync(string host, int port)
    {
        using var client = new System.Net.Sockets.TcpClient();
        try
        {
            await client.ConnectAsync(host, port);
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
