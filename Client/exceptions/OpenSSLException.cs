namespace Client.exceptions; 

public class OpenSSLException : Exception
{
    public OpenSSLException(string message, Exception innerException) : base(message, innerException)
    {
    }

public OpenSSLException(string message) : base(message)
    {
    }

}
