using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Client.config;
using Client.models;
using Client.utils;

namespace Client;

public class ChatService
{
    private TcpClient TcpClient;
    private NetworkStream Stream;
    private StreamWriter Writer;
    private StreamReader Reader;
    private string separador = "::";


    public  bool Connect(string clientId, string server, int port)
    {
        TcpClient = new TcpClient();
        TcpClient.Connect(server, port);
        Stream = TcpClient.GetStream();
        Writer = new StreamWriter(Stream) { AutoFlush = true };
        Reader = new StreamReader(Stream);
        Writer.WriteLine($"::clientId::::");
        return true;
    }

    public  bool Reconnect(string clientId, string server, int port)
    {
        TcpClient = new TcpClient();
        TcpClient.Connect(server, port);
        Stream = TcpClient.GetStream();
        Writer = new StreamWriter(Stream) { AutoFlush = true };
        Reader = new StreamReader(Stream);
        Writer.WriteLine($"::clientId::::");
        return true;
    }

    public  bool SendMessage(string clientId, string recipientPublicKeyPath, string message)
    {
        var encryptedMessage = RSAUtils.EncryptMessage(message, recipientPublicKeyPath);
        var nomeDestino = recipientPublicKeyPath.Split("/").Last().Split(".")[0];
     
        var request = montarRequest("nova-mensagem", clientId, nomeDestino, encryptedMessage);
        Writer.WriteLine(request);
        return true;
    }

    public  List<Mensagem> history(string clientId, string recipientPublicKeyPath)
    {
        var nomeDestino = recipientPublicKeyPath.Split("/").Last().Split(".")[0];

        var request = montarRequest("historico", clientId, nomeDestino, "");
        Writer.WriteLine(request);
        Console.WriteLine("Histórico de mensagens");
        string line;
        var mensagens = new List<Mensagem>();
        while ((line = Reader.ReadLine()) != null)
        {
            var item = System.Text.Json.JsonSerializer.Deserialize<Mensagem>(line);
            var decryptedMessage = RSAUtils.DecryptMessage(item.Message, getPrivateKeyPath(clientId));
            Console.WriteLine(decryptedMessage);
            Console.WriteLine("\n\n");

            mensagens.Add(item);
            
        }

        Reconnect(clientId, Configuration.Server, Configuration.Port);

        return mensagens;
    }

    private string getPrivateKeyPath(string clientId)
    {
        return $"{Configuration.KeysDirectory}/{clientId}.key.private.pem";
    }

    public  List<Destinatario> GetDestinatarios(string v)
    {
        List<Destinatario> destinatarios = new List<Destinatario>();
        DirectoryInfo di = new DirectoryInfo(v);
        FileInfo[] fiArr = di.GetFiles();
        foreach (FileInfo f in fiArr)
        {
            var nome = f.Name.Split(".")[0];
            destinatarios.Add(new Destinatario { Nome = nome, FileName = f.FullName });
        }
        return destinatarios;
    }

    private string montarRequest(string prefixo, string clientId, string destino, string mensagem)
    {
        return $"{prefixo}{separador}{clientId}{separador}{destino}{separador}{mensagem}";
    }
   
}
