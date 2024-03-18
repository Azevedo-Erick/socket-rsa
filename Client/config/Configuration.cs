using System.Net.Sockets;

namespace Client.config; 

public static  class Configuration
{
    public static  int Port { get; private set; } = 5000;
    public  static string Server { get; private set; } = "localhost"; 
    public  static  string KeysDirectory {get; private set;} = "./keys";
    public  static  string PublicKeysDirectory {get; private set;} = "./public-keys";


   
}
