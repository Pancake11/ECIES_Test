using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

class SslTlsClient
{
    private const int BUFFER_SIZE = 2048;

    private static bool validateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        Console.WriteLine("Certificate Errors: {0}", sslPolicyErrors);
        return false;
    }

    public static void RunClient(string machineName, string serverName, string clientCertificatePath, string clientCertificatePwd)
    {
        TcpClient client = new TcpClient("127.0.0.1", 5000);

        SslStream sslStream = new SslStream(
            client.GetStream(),
            false,
            new RemoteCertificateValidationCallback(validateServerCertificate),
            null
            );

        try
        {
            X509Certificate clientCertificate = new X509Certificate(clientCertificatePath, clientCertificatePwd);
            X509CertificateCollection certificateCollection = new X509CertificateCollection();
            sslStream.AuthenticateAsClient(serverName, certificateCollection, SslProtocols.Tls12, false);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error {0}", e.Message);
            if (e.InnerException != null)
            {
                Console.WriteLine("InnerException: {0}", e.InnerException);
            }

            Console.WriteLine("Authentication failed - closing connection");
            client.Close();
        }

        byte[] message = Encoding.UTF8.GetBytes("Yo la zone");

        sslStream.Write(message);
        sslStream.Flush();

        string serverMessage = ReadMessage(sslStream);
        Console.WriteLine("Server Response: {0}", serverMessage);

        client.Close();
        Console.WriteLine("Client Closed");
    }

    static string ReadMessage(SslStream sslStream)
    {
        byte[] buffer = new byte[BUFFER_SIZE];
        StringBuilder message = new StringBuilder();
        int bytes = -1;

        do
        {
            bytes = sslStream.Read(buffer, 0, buffer.Length);

            Decoder decoder = Encoding.UTF8.GetDecoder();
            char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
            message.Append(chars);

            if (message.ToString().IndexOf("<EOF>") != -1)
                break;
        } while (bytes != 0);

        return message.ToString();
    }

    private static void usage()
    {
        Console.WriteLine("To start the client specify:");
        Console.WriteLine("./client machineName serverName certificatePath certificatePassword");
        Environment.Exit(1);
    }

    public static int Main(string[] args)
    {
        if (args.Length < 2)
            usage();

        string machineName = args[0];
        string serverCertifcateName = args[1];
        string certificatePath = args[2];
        string certificatePwd = args[3];

        SslTlsClient.RunClient(machineName, serverCertifcateName, certificatePath, certificatePwd);
        return 0;
    }
}

