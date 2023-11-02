using Newtonsoft.Json.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Server
{
    internal class Client
    {
        private ClientData? clientData;
        private readonly SslStream sslStream;
        private readonly object sendLock = new object();

        public int ClientId { get; private set; }

        public Client(TcpClient tcpClient)
        {
            ClientId = -1;
            sslStream = CreateSslStream(tcpClient);
            (new Thread(Listen)).Start();
        }

        private void Listen()
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(sslStream))
                {
                    while (true)
                    {
                        uint length = reader.ReadUInt32();

                        byte[] receivedBytes = new byte[length];
                        int receivedBuffer = 0;

                        while (receivedBuffer < length)
                        {
                            receivedBuffer += reader.Read(receivedBytes, (int)receivedBuffer, (int)(length - receivedBuffer));
                        }

                        byte messageType = receivedBytes[0];

                        //Console.WriteLine($"Received data: {data}");
                        Console.WriteLine($"Received messageType: {messageType}");

                        switch (messageType)
                        {
                            case 0:
                                if (ClientId == -1)
                                {
                                    ClientId = BitConverter.ToInt32(receivedBytes, 1);
                                    Console.WriteLine($"Client connected with ID: {ClientId}");

                                    Server.AddClient(this);
                                    clientData = Server.GetClientData(this);
                                }
                                break;
                            case 1:
                                if (clientData != null)
                                {
                                    long unixTime = BitConverter.ToInt64(receivedBytes, 5);
                                    DateTime time = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;

                                    clientData.AddHeartBeat(new HeartBeatData(BitConverter.ToUInt32(receivedBytes, 1), time));
                                    Console.WriteLine($"Added Heartbeat data for client {ClientId}: {BitConverter.ToUInt32(receivedBytes, 1)}");
                                }
                                break;
                            case 2:
                                Console.WriteLine(ClientId);
                                if (clientData != null)
                                {
                                    long unixTime = BitConverter.ToInt64(receivedBytes, 5);
                                    DateTime time = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;

                                    clientData.AddSpeed(new SpeedData(BitConverter.ToUInt32(receivedBytes, 1), time));
                                    Console.WriteLine($"Added Speed data for client {ClientId}: {BitConverter.ToUInt32(receivedBytes, 1)}");
                                }
                                break;
                            case 3:
                                Login(receivedBytes);
                                break;
                            case 4:
                                SendClientIDs();
                                break;
                            case 7:
                                SubscribeClient(receivedBytes);
                                break;

                            case 12:
                                int clientId = BitConverter.ToInt32(receivedBytes, 1);
                                string message = Encoding.UTF8.GetString(receivedBytes, 5, receivedBytes.Length - 5);

                                byte[] m = Encoding.UTF8.GetBytes(message);
                                Server.SendClientMessage(clientId, 13, m);
                                break;
                        }
                    }
                }
            }
            catch (IOException)
            {
                // Client disconnected, save data to a file
                Console.WriteLine("Test");
            }
            finally
            {
                Server.RemoveClient(this);
            }
            SerializeClientData(ClientId);
        }

        public void SendDoctorData(DataType type, int clientId, DateTime time, uint data)
        {
            byte[] message = new byte[16];
            BitConverter.GetBytes(clientId).CopyTo(message, 0);
            BitConverter.GetBytes(data).CopyTo(message, 4);
            BitConverter.GetBytes(((DateTimeOffset)time).ToUnixTimeSeconds()).CopyTo(message, 8);
            switch (type)
            {
                case DataType.HeartBeat:
                    SendMessage(8, message);
                    break;
                case DataType.Speed:
                    SendMessage(9, message);
                    break;
            }
        }

        private void SubscribeClient(byte[] receivedBytes)
        {
            int clientId = BitConverter.ToInt32(receivedBytes, 1);
            long time = BitConverter.ToInt64(receivedBytes, 5);

            DateTime timeLimit = DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime;

            ClientData data = Server.GetClientData(clientId);

            data.AddCallback(SendDoctorData);

            int speedLength = data.SpeedList.Count;
            int heartBeatLength = data.HeartbeatList.Count;

            byte[] speedMessage = new byte[4 + speedLength * 12];
            byte[] heartBeatMessage = new byte[4 + heartBeatLength * 12];

            BitConverter.GetBytes(clientId).CopyTo(speedMessage, 0);
            BitConverter.GetBytes(clientId).CopyTo(heartBeatMessage, 0);

            int j = 0;
            for (int i = 0; i < speedLength; i++)
            {
                SpeedData speedData = data.SpeedList[i];
                if (speedData.Time > timeLimit)
                {
                    BitConverter.GetBytes(speedData.Speed).CopyTo(speedMessage, 4 + 12 * j);
                    BitConverter.GetBytes(((DateTimeOffset)speedData.Time).ToUnixTimeSeconds()).CopyTo(speedMessage, 8 + 12 * j);
                    j++;
                }
            }
            speedLength = j;

            j = 0;
            for (int i = 0; i < heartBeatLength; i++)
            {
                HeartBeatData heartBeatData = data.HeartbeatList[i];
                if (heartBeatData.Time > timeLimit)
                {
                    BitConverter.GetBytes(heartBeatData.HeartBeat).CopyTo(heartBeatMessage, 4 + 12 * i);
                    BitConverter.GetBytes(((DateTimeOffset)heartBeatData.Time).ToUnixTimeSeconds()).CopyTo(heartBeatMessage, 8 + 12 * i);
                    j++;
                }
            }
            heartBeatLength = j;

            byte[] finalSpeedMessage = new byte[4 + speedLength * 12];
            byte[] finalHeartBeatMessage = new byte[4 + heartBeatLength * 12];
            Array.Copy(speedMessage, 0, finalSpeedMessage, 0, finalSpeedMessage.Length);
            Array.Copy(heartBeatMessage, 0, finalHeartBeatMessage, 0, finalHeartBeatMessage.Length);

            SendMessage(10, finalHeartBeatMessage);
            SendMessage(11, finalSpeedMessage);
        }

        void SerializeClientData(int clientId)
        {
            if (clientId != -1 && clientData != null)
            {
                string folderName = "DataFiles";
                Directory.CreateDirectory(folderName);
                string fileName = Path.Combine(folderName, $"{clientId}.dat");

                // Serialize the data to a memory stream first
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(memoryStream, clientData);

                    // Write the memory stream to the file, overwriting it
                    File.WriteAllBytes(fileName, memoryStream.ToArray());
                }
                Console.WriteLine($"Saving client data to file: {fileName}");
            }
        }

        private void Login(byte[] receivedBytes)
        {
            if (receivedBytes[1] == 1)
            {
            }

            int index = Array.IndexOf(receivedBytes, (byte)0);

            if (index < 0) return;

            string sendUser = Encoding.UTF8.GetString(receivedBytes, 2, index - 2);

            string sendPassword = Encoding.UTF8.GetString(receivedBytes, index + 1, receivedBytes.Length - index - 1);

            string projectDir = Directory.GetCurrentDirectory();
            Console.WriteLine(projectDir);

            string binDebugNetPath = System.IO.Path.Combine("bin", "Debug", "net6.0");
            Console.WriteLine(binDebugNetPath);

            string passDirPath = System.IO.Path.Combine(projectDir.Replace(binDebugNetPath, ""), "PassDir");
            Console.WriteLine(passDirPath);

            // Find all JSON files in PassDir
            string[] jsonFiles = Directory.GetFiles(passDirPath, "*.json");

            // Read the first JSON file found (assuming there's only one)
            string jsonFilePath = jsonFiles.First();
            string jsonContent = File.ReadAllText(jsonFilePath);

            // Parse JSON
            JObject credentials = JObject.Parse(jsonContent);

            // Get the username and password
            string? username = (string?)credentials["username"];
            string? password = (string?)credentials["password"];


            byte success;
            if (username == null || password == null) success = 0;
            else success = (byte)(username.Equals(sendUser) && password.Equals(sendPassword) ? 1 : 0);

            SendMessage(6, new byte[] { success });
        }

        private void SendClientIDs()
        {
            lock (sendLock)
            {
                int[] ids = GetAllClientID();
                uint len = (uint)ids.Length * 4 + 5;

                sslStream.Write(BitConverter.GetBytes(len));
                sslStream.WriteByte(5);
                sslStream.Write(BitConverter.GetBytes(ids.Length));
                foreach (int id in ids)
                {
                    sslStream.Write(BitConverter.GetBytes(id));
                }
                sslStream.Flush();
            }
        }

        static int[] GetAllClientID()
        {
            var clients = Server.GetClients();

            int[] list = new int[clients.Count];

            int i = 0;
            foreach (var client in clients)
            {
                list[i] = client.Key;
                i++;
            }

            return list;
        }

        public void SendMessage(byte type, byte[] message)
        {
            uint len = (uint)(message.Length + 1);
            lock (sendLock)
            {
                sslStream.Write(BitConverter.GetBytes(len));
                sslStream.WriteByte(type);
                sslStream.Write(message);
                sslStream.Flush();
            }
        }

        private SslStream CreateSslStream(TcpClient client)
        {
            // Load the server's SSL certificate
            X509Certificate2 serverCertificate = new X509Certificate2("server.pfx", "YourPassword");

            // Create an SSL stream and authenticate the client
            SslStream sslStream = new SslStream(client.GetStream(), false);
            sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.None, true);

            return sslStream;
        }
    }
}
