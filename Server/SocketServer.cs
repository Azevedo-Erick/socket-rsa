using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
namespace Server;
public class SocketServer
{
    private static ConcurrentDictionary<string, List<Message>> messages = new ConcurrentDictionary<string, List<Message>>();

    public static async Task Main(string[] args)
    {
        await StartServer(5000);
    }

    public static async Task StartServer(int port)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Servidor iniciando e aguardando conexões na porta {port}.");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        var stream = client.GetStream();
        var reader = new StreamReader(stream);
        var writer = new StreamWriter(stream) { AutoFlush = true };

        try
        {


            while (client.Connected)
            {
                var message = await reader.ReadLineAsync();

                if (message != null)
                {
                    var separator = "::";
                    Console.WriteLine(message);
                    var prefix = message.Split(separator)[0];
                    var clientId = message.Split(separator)[1];
                    var destino = message.Split(separator)[2];
                    message = message.Split(separator)[3];
                    if (prefix == "nova-mensagem")
                    {
                        await EnviarMensagem(clientId, message, destino);
                        continue;
                    }

                    if (prefix == "historico")
                    {
                        await SendChatHistoryToClient(clientId, destino, writer);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine($"Erro: {e.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private static async Task EnviarMensagem(string? clientId, string? message, string destino)
    {
        var mensagem = new Message
        {
            Mensagem = message,
            Destino = destino,
            Autor = clientId,
            Data = DateTime.Now
        };


        string[] participants = { mensagem.Autor, mensagem.Destino };
        Array.Sort(participants);
        string chatKey = $"{participants[0]}-{participants[1]}";
        if (messages.ContainsKey(chatKey))
        {
            messages[chatKey].Add(mensagem);
        }
        else
        {
            messages[chatKey] = new List<Message> { mensagem };
        }
    }

    private static async Task SendChatHistoryToClient(string clientId, string otherClientId, StreamWriter writer)
    {
        string[] participants = { clientId, otherClientId };
        Array.Sort(participants);
        var chatKey = $"{participants[0]}-{participants[1]}";
        if (messages.TryGetValue(chatKey, out var chatMessages))
        {
            var data = new List<string>();
            foreach (var msg in chatMessages)
            {
                var messageJson = JsonSerializer.Serialize(new
                {
                    Author = msg.Autor,
                    Message = msg.Mensagem,
                    Timestamp = msg.Data.ToString("o")
                });
                data.Add(messageJson);
            }
            
            await writer.WriteLineAsync(string.Join(Environment.NewLine, data));
            writer.Close();

        }
    }
}