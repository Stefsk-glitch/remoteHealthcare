using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static client.Program;

namespace client
{
    internal class RemoteServer
    {
        private List<chatCallback> chatCallbacks;
        private List<resistanceCallback> resistanceCallbacks;
        private runningCallback runningCallback;
        private speedCallback speedCallback;
        private readonly SslStream stream;
        private readonly int clientId = 7316;
        public RemoteServer(string IP)
        {
            TcpClient client = new TcpClient(IP, 8000);

            chatCallbacks = new List<chatCallback>();
            resistanceCallbacks = new List<resistanceCallback>();
            stream = new SslStream(client.GetStream(), false, CertificateValidation);
            stream.AuthenticateAsClient("localhost", null, SslProtocols.None, true);

            SendData(BitConverter.GetBytes(clientId), 0);//send User ID

            Console.WriteLine("Client connected: " + client.Connected);
        }

        private static bool CertificateValidation(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
        {
            if (cert == null) return false;
            if (cert.GetCertHashString().Equals("A9C26B7E4FCA6974C3B7BA3C5ADEA1C7F35C259B")) return true;
            return false;
        }

        /// <summary>
        /// 4 bytes lengte (uint32, big endian), 1 byte message type.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="messageType"></param>
        public void SendData(byte[] dataBytes, byte messageType)
        {
            byte[] message = new byte[dataBytes.Length + 1];
            uint lengte = (uint)(message.Length);

            // Copy dataBytes to message
            Array.Copy(dataBytes, 0, message, 1, dataBytes.Length);
            message[0] = messageType;
            stream.Write(BitConverter.GetBytes(lengte));
            stream.Write(message, 0, message.Length);
            stream.Flush();
        }

        public void Run()
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (true)
                {
                    uint length = reader.ReadUInt32();
                    string message = "";

                    byte[] receivedBytes = new byte[length];
                    int receivedBuffer = 0;

                    while (receivedBuffer < length)
                    {
                        receivedBuffer += reader.Read(receivedBytes, (int)receivedBuffer, (int)(length - receivedBuffer));
                    }

                    byte messageType = receivedBytes[0];

                    switch (messageType)
                    {
                        case 13:
                            message = Encoding.UTF8.GetString(receivedBytes, 1, receivedBytes.Length - 1);
                            Console.WriteLine(message);
                            ProcessChatMessage(message);
                            break;
                    }
                }
            }
        }

        public void AddSpeedCallback(speedCallback callback)
        {
            speedCallback = callback;
        }

        private void ProcessChatMessage(string message)
        {
            if (message == "/start")
            {
                foreach (chatCallback callback in chatCallbacks)
                {
                    callback.Invoke("SESSION STARTED");
                    runningCallback.Invoke(true);
                }
            }
            else if (message == "/stop")
            {
                foreach (chatCallback callback in chatCallbacks)
                {
                    callback.Invoke("SESSION STOPPED");
                }
            }
            else if (message == "/estop")
            {
                foreach (chatCallback callback in chatCallbacks)
                {
                    callback.Invoke("SESSION EMERGENCY STOPPED");
                    runningCallback.Invoke(false);
                    SpeedCallback(0);
                    speedCallback.Invoke(0);
                }
            }
            else if (message.StartsWith("/resistance"))
            {
                byte res = byte.Parse(message.Substring(12));
                foreach (resistanceCallback callback in resistanceCallbacks)
                {
                    callback.Invoke(clientId, (byte)(res * 2));
                }
            }
            else
            {
                foreach (chatCallback callback in chatCallbacks)
                {
                    callback.Invoke(message);
                }
            }
        }

        public void AddChatCallback(chatCallback callback)
        {
            chatCallbacks.Add(callback);
        }

        public void AddRunningCallback(runningCallback callback)
        {
            runningCallback = callback;
        }

        public void SpeedCallback(double speed)
        {
            byte[] dataBytes = new byte[12];
            long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            BitConverter.GetBytes((uint)speed).CopyTo(dataBytes, 0);
            BitConverter.GetBytes(time).CopyTo(dataBytes, 4);
            SendData(dataBytes, 2);
        }

        internal void AddResistanceCallback(resistanceCallback callback)
        {
            resistanceCallbacks.Add(callback);
        }
    }
}