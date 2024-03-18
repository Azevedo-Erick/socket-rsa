using System.Diagnostics;
using Client.exceptions;

namespace Client.utils; 

public class RSAUtils
{
     public static string EncryptMessage(string message, string publicKeyPath)
    {
        string tempMessagePath = Path.GetTempFileName();

        File.WriteAllText(tempMessagePath, message);
        string encryptedFilePath = Path.GetTempFileName();

        try
        {
            var psi = new ProcessStartInfo("openssl", $"rsautl -encrypt -in {tempMessagePath} -out {encryptedFilePath} -pubin -inkey {publicKeyPath}")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = Process.Start(psi))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    throw new InvalidOperationException($"OpenSSL error: {error}");
                }
            }

            byte[] encryptedBytes = File.ReadAllBytes(encryptedFilePath);
            return Convert.ToBase64String(encryptedBytes);
        }
        finally
        {
            File.Delete(tempMessagePath);
            File.Delete(encryptedFilePath);
        }

    }

     public static string DecryptMessage(string encryptedMessageBase64, string privateKeyPath)
    {
       
        string tempEncryptedMessagePath = Path.GetTempFileName();
        byte[] encryptedBytes = Convert.FromBase64String(encryptedMessageBase64);
        File.WriteAllBytes(tempEncryptedMessagePath, encryptedBytes);

        string decryptedMessagePath = Path.GetTempFileName();

        try
        {
            var psi = new ProcessStartInfo("openssl", $"rsautl -decrypt -in {tempEncryptedMessagePath} -out {decryptedMessagePath} -inkey {privateKeyPath}")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(psi))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    throw new OpenSSLException($"Erro: {error}");
                }
            }
            string decryptedMessage = File.ReadAllText(decryptedMessagePath);
            return decryptedMessage;
        }
        finally
        {
            File.Delete(tempEncryptedMessagePath);
            File.Delete(decryptedMessagePath);
        }
    }
}
