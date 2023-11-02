using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Server
{
    internal class Server
    {
        static readonly Dictionary<int, ClientData> clients = new Dictionary<int, ClientData>();
        static readonly Dictionary<int, Client> connected = new Dictionary<int, Client>();

        public static void Main(string[] args)
        {
            string ipAddress = "127.0.0.1"; // "145.49.43.10"
            int port = 8000;

            LoadClientData(); // Load existing client data from files on server startup

            TcpListener listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            listener.Start();
            Console.WriteLine("Server started on {0}:{1}", ipAddress, port);

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                TcpClient client = listener.AcceptTcpClient();

                Client c = new Client(client);
            }
        }

        public static void AddClient(Client client)
        {
            connected.Add(client.ClientId, client);
        }

        public static void RemoveClient(Client client)
        {
            connected.Remove(client.ClientId);
        }

        public static Client? GetClient(int clientId)
        {
            if (connected.ContainsKey(clientId)) return connected[clientId];
            else return null;
        }

        public static void SendClientMessage(int clientId, byte type, byte[] payload)
        {
            if (!connected.ContainsKey(clientId)) return;
            connected[clientId].SendMessage(type, payload);
        }

        public static ClientData GetClientData(Client client)
        {
            // Create a new client if it doesn't exist
            if (!clients.ContainsKey(client.ClientId))
            {
                clients[client.ClientId] = new ClientData { ID = client.ClientId };
            }
            return clients[client.ClientId];
        }

        public static ClientData GetClientData(int clientId)
        {
            // Create a new client if it doesn't exist
            if (!clients.ContainsKey(clientId))
            {
                clients[clientId] = new ClientData { ID = clientId };
            }
            return clients[clientId];
        }

        public static Dictionary<int, ClientData> GetClients()
        {
            return clients;
        }

        static void LoadClientData()
        {
            string folderPath = "DataFiles";

            try
            {
                // Load existing client data from files in the subfolder
                foreach (string file in Directory.GetFiles(folderPath, "*.dat"))
                {
                    int clientId = int.Parse(Path.GetFileNameWithoutExtension(file));
                    using (FileStream fs = new FileStream(file, FileMode.Open))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        ClientData clientData = (ClientData)formatter.Deserialize(fs);
                        clients[clientId] = clientData;

                        // Display the loaded data for this client
                        Console.WriteLine($"Loaded data for client ID: {clientData.ID}");
                        Console.WriteLine("Heartbeat List:");
                        foreach (HeartBeatData heartbeatData in clientData.HeartbeatList)
                        {
                            Console.WriteLine(heartbeatData);
                        }
                        Console.WriteLine("Speed List:");
                        foreach (SpeedData speedData in clientData.SpeedList)
                        {
                            Console.WriteLine(speedData);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading client data: {e.Message}");
            }
        }
    }
}
