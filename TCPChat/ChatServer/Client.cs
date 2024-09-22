﻿using System.Net.Sockets;

namespace ChatServer;

public class Client
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal string UserName { get; private set; } = string.Empty;
    protected internal StreamReader Reader { get; }
    protected internal StreamWriter Writer { get; }

    private TcpClient _client;
    private Server _server;

    public Client(TcpClient client, Server server)
    {
        _client = client;
        _server = server;
        var stream = client.GetStream();
        Writer = new StreamWriter(stream);
        Reader = new StreamReader(stream);
        server.AddClient(this);
    }

    public async Task ProcessAsync()
    {
        try
        {
            while (true)
            {
                string? proposedUserName = await Reader.ReadLineAsync();
                if (proposedUserName == null) continue;

                if (_server.UserNameTaken(proposedUserName))
                {
                    await Writer.WriteLineAsync("NameTaken");
                    await Writer.FlushAsync();
                }
                else
                {
                    UserName = proposedUserName;
                    await Writer.WriteLineAsync("Welcome");
                    await Writer.FlushAsync();
                    string otherUsers = string.Join(", ", _server.Clients.Where(c => c.Id != Id).Select(c => c.UserName));
                    await Writer.WriteLineAsync(string.IsNullOrEmpty(otherUsers) ? "There is no one in the chat :C" : $"Users in chat: {otherUsers}");
                    await Writer.FlushAsync();

                    break;
                }
            }

            string? message = $"{UserName} joined the chat!";
            await _server.BroadCastMessage(message, Id);
            Console.WriteLine(message);

            try
            {
                while (true)
                {
                    message = await Reader.ReadLineAsync();
                    if (message == null) continue;

                    if (message.StartsWith(">"))
                    {
                        int colonIndex = message.IndexOf(':');
                        if (colonIndex > 1)
                        {
                            string recipientName = message.Substring(1, colonIndex - 1).Trim();
                            string privateMessage = message.Substring(colonIndex + 1).Trim();
                            Client? recipient = _server.GetClientName(recipientName);
                            if (recipient != null)
                            {
                                await recipient.Writer.WriteLineAsync($"Private message from {UserName}: {privateMessage}");
                                await recipient.Writer.FlushAsync();
                                await Writer.WriteLineAsync($"Private message to {recipientName}: {privateMessage}");
                                await Writer.FlushAsync();
                            }
                            else
                            {
                                await Writer.WriteLineAsync($"User '{recipientName}' not found.");
                                await Writer.FlushAsync();
                            }
                        }
                        else
                        {
                            await Writer.WriteLineAsync("Invalid private message format. Use \"> Name : message\"");
                            await Writer.FlushAsync();
                        }
                    
                    }
                    else
                    {
                        message = $"{UserName}: {message}";
                        Console.WriteLine(message);
                        await _server.BroadCastMessage(message, Id);
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"{UserName} left the chat";
                Console.WriteLine(message);
                await _server.BroadCastMessage(message, Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _server.RemoveClient(Id);
        }
    }

    public void Close()
    {
        Writer.Close();
        Reader.Close();
        _client.Close();
    }
}