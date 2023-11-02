using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace client.Classes;

public class CommandSender
{
    private readonly TcpClient client;
    private readonly SslStream? sslStream;

    public CommandSender(string ip)
    {
        client = new TcpClient();
        Console.WriteLine("going into try");
        try
        {
            client.Connect(ip, 8000);
            Console.WriteLine("after connect");
            Stream stream = client.GetStream();
            Console.WriteLine("after stream");

            sslStream = new SslStream(stream, false, CertificateValidation);
            Console.WriteLine("after init ssl ");
            sslStream.AuthenticateAsClient("localhost");

            Console.WriteLine("Client connected: " + client.Connected);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Error connecting to the server: " + ex.Message);
        }
    }
    private static bool CertificateValidation(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
    {
        return cert != null && cert.GetCertHashString().Equals("A9C26B7E4FCA6974C3B7BA3C5ADEA1C7F35C259B");
    }

    /// <summary>
    /// 4 bytes length (uint32, big endian), 1 byte message type.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="messageType"></param>
    public void SendData(uint data, byte messageType)
    {
        byte[] dataBytes = BitConverter.GetBytes(data);

        byte[] message = new byte[dataBytes.Length + 1];

        // Copy dataBytes to message
        Array.Copy(dataBytes, 0, message, 0, dataBytes.Length);
        message[dataBytes.Length] = messageType;
        sslStream?.Write(message, 0, message.Length);
    }

    public TcpClient GetClient()
    {
        return client;
    }

    public void CloseConnection()
    {
        client.Close();
    }

}