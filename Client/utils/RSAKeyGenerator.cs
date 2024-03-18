using System.Diagnostics;

namespace Client.utils; 

public class RSAKeyGenerator
{
     public static void GenerateKeysForClient(string clientName)
    {
        string keysDirectory = "./keys";
        string privateKeyPath = $"{keysDirectory}/{clientName}.key.private.pem";
        string publicKeyPath = $"{keysDirectory}/{clientName}.key.public.pem";

        Directory.CreateDirectory(keysDirectory);

        string genPrivateKeyCommand = $"genrsa -out {privateKeyPath} 2048";
        string genPublicKeyCommand = $"rsa -pubout -in {privateKeyPath} -out {publicKeyPath}";

        ExecuteOpenSSLCommand(genPrivateKeyCommand);
        ExecuteOpenSSLCommand(genPublicKeyCommand);
    }

    private static void ExecuteOpenSSLCommand(string command)
    {
        ProcessStartInfo procStartInfo = new ProcessStartInfo("openssl", command)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process())
        {
            process.StartInfo = procStartInfo;
            process.Start();

            string result = process.StandardOutput.ReadToEnd();
            Console.WriteLine(result);

            process.WaitForExit();
        }
    }
}
