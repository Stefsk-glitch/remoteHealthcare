using System.Net.Sockets;
using System.Text;
using static client.Program;

namespace client
{
    internal class HardwareServer
    {
        private readonly NetworkStream stream;
        private readonly List<speedCallback> speedCallbacks;
        private bool isRunning = false;
        public bool IsRunning { get { return isRunning; } private set{}}
        internal HardwareServer()
        {
            string serverIpAddress = "127.0.0.1";
            int serverPort = 9000;

            speedCallbacks = new List<speedCallback>();

            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(serverIpAddress, serverPort);
            stream = tcpClient.GetStream();
        }

        internal void AddSpeedCallback(speedCallback speedCallback)
        {
            speedCallbacks.Add(speedCallback);
        }

        internal void Run()
        {
            while (true)
            {
                byte[] responseBytes = new byte[4];
                stream.Read(responseBytes, 0, responseBytes.Length);
                uint responseSize = BitConverter.ToUInt32(responseBytes, 0);

                responseBytes = new byte[responseSize];
                int receivedBuffer = 0;

                while (receivedBuffer < responseSize)
                {
                    receivedBuffer += stream.Read(responseBytes, receivedBuffer, (int)(responseSize - receivedBuffer));
                }

                int index = Array.IndexOf(responseBytes, (byte)0);
                if (index < 0) continue;

                string service = Encoding.UTF8.GetString(responseBytes, 0, index);

                byte[] dataBytes = new byte[responseSize - index - 1];
                Array.Copy(responseBytes, index + 1, dataBytes, 0, responseSize - index - 1);

                if (dataBytes[4] != 16) continue;

                double convertedData = GetSpeed(dataBytes);
                Console.WriteLine("{0}: {1}", service, convertedData);

                if(IsRunning){
                    foreach (var callback in speedCallbacks)
                    {
                        callback.Invoke(convertedData);
                    }
                }
            }
        }

        public static double GetSpeed(byte[] data)
        {
            return BitConverter.ToUInt16(data, 8) * 0.001 * 3.6;
        }

        public void ResistanceCallback(int clientId, byte resistance)
        {
            stream.Write(BitConverter.GetBytes((uint) 6));
            stream.WriteByte(14);
            stream.Write(BitConverter.GetBytes(clientId));
            stream.WriteByte(resistance);
            stream.Flush();
        }
        public void SetRunning(bool value){isRunning = value;}
    }

}