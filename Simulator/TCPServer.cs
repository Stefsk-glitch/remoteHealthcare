using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Simulator
{
    internal class TCPServer
    {
        NetworkStream stream;

        public TCPServer()
        {
            string ipAddress = "127.0.0.1"; // "145.49.43.10"
            int port = 9000;

            TcpListener listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            listener.Start();
            Console.WriteLine("Server started on {0}:{1}", ipAddress, port);

            TcpClient client = listener.AcceptTcpClient();
            stream = client.GetStream();
        }

        public void SendDataToClient(byte[] data, String serviceName)
        {
            byte[] s = Encoding.UTF8.GetBytes(serviceName);

            uint len = (uint)(s.Length + data.Length + 1);
            stream.Write(BitConverter.GetBytes(len));
            stream.Write(s);
            stream.Write(new byte[] { 0 });
            stream.Write(data);
            stream.Flush();
        }

        public static byte CalculateChecksum(byte[] bytes)
        {
            byte sum = 0;
            for (int i = 0; i < bytes.Length - 1; ++i)
            {
                sum ^= bytes[i];
            }
            return sum;
        }
    }
}
