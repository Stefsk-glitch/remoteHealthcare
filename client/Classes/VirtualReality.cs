using JsonParser;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using vr;

namespace client.Classes
{
    public class VirtualReality
    {
        private string tunnelID;
        private NetworkStream stream;
        private List<string> modelList;
        private string routeID;

        public VirtualReality(string userNameToFind)
        {
            this.modelList = new List<String>();
            TcpClient tcpVr = new TcpClient("145.48.6.10", 6666);
            this.stream = tcpVr.GetStream();

            byte[] jsonInBytes = SendAndReceiveMessage("{\"id\":\"session/list\"}");

            string jsonString = Encoding.ASCII.GetString(jsonInBytes);

            List<DataItem> userItems = JsonDataParser.FindUserItems(jsonString, userNameToFind);

            if (userItems.Count > 0)
            {
                foreach (var item in userItems)
                {
                    Console.WriteLine($"User: {item.clientinfo.user}, ID: {item.id}");

                    Console.WriteLine(JsonDataParser.CreateTunnel(item.id));

                    byte[] response = SendAndReceiveMessage(JsonDataParser.CreateTunnel(item.id));
                    JObject responseObject = JObject.Parse(Encoding.ASCII.GetString(response));
                    Console.WriteLine(responseObject);
                    this.tunnelID = (string)responseObject["data"]["id"];
                }
            }
            else
            {
                Console.WriteLine($"No user with the name '{userNameToFind}' found.");
            }
        }

        public string getTunnelID()
        {
            return this.tunnelID;
        }

        public NetworkStream GetNetworkStream()
        {
            return this.stream;
        }

        public void CreateTerrain(Terrain terrain)
        {
            SendAndReceiveMessage(terrain.CreateTerrainJson(this.tunnelID));
            byte[] response = SendAndReceiveMessage(Node.AddTerrain(this.tunnelID));
            JObject responseObject = JObject.Parse(Encoding.ASCII.GetString(response));
            string terrainID = (string)responseObject["data"]["data"]["data"]["uuid"];
            SendAndReceiveMessage(Node.AddTerrainLayer(this.tunnelID, terrainID));

        }

        public void UpdateCamera(string nodeID, string parentID){
            byte[] response = SendAndReceiveMessage(Node.UpdateNode(this.tunnelID, nodeID, parentID));

            JObject responseObject = JObject.Parse(Encoding.ASCII.GetString(response));

            Console.WriteLine(responseObject.ToString());
            
        }

        public string AddPanel(string text, string cameraID, string name, int x, int y, int z)
        {
            byte[] response = SendAndReceiveMessage(Node.AddPanel(this.tunnelID, cameraID,x,y,z, name));

            JObject responseObject = JObject.Parse(Encoding.ASCII.GetString(response));
            
            string panelID = (string)responseObject["data"]["data"]["data"]["uuid"];
            SendAndReceiveMessage(Panel.ClearPanelBuffer(this.tunnelID, panelID));

            SendAndReceiveMessage(Panel.DrawText(this.tunnelID, panelID, text));

            SendAndReceiveMessage(Panel.SwapPanelBuffer(this.tunnelID, panelID));

            return panelID;
        }

        public void RefreshPanel(string text, string panelID)
        {
            SendAndReceiveMessage(Panel.ClearPanelBuffer(this.tunnelID, panelID));

            SendAndReceiveMessage(Panel.DrawText(this.tunnelID, panelID, text));

            SendAndReceiveMessage(Panel.SwapPanelBuffer(this.tunnelID, panelID));
        }

        public void GetScene(){
            byte[] response = SendAndReceiveMessage(Node.GetScene(this.tunnelID));

            JObject responseObject = JObject.Parse(Encoding.ASCII.GetString(response));
            Console.WriteLine(responseObject.ToString());
        }

        public void AddRoute()
        {
            byte[] response = SendAndReceiveMessage(Route.AddRoute(this.tunnelID));

            JObject responseObject = JObject.Parse(Encoding.ASCII.GetString(response));

            this.routeID = (string)responseObject["data"]["data"]["data"]["uuid"];

            SendAndReceiveMessage(Road.AddRoad(this.tunnelID, this.routeID));
        }

        public void AddModel(string modelDir, int x, int y, int z, int scale, int rotX, int rotY, int rotZ, string name)
        {   
            byte[] response = SendAndReceiveMessage(Node.AddModel(tunnelID, modelDir, x, y, z, scale, rotX, rotY, rotZ, name));
            JObject responseObject = JObject.Parse(Encoding.ASCII.GetString(response));

            this.modelList.Add((string)responseObject["data"]["data"]["data"]["uuid"]);
        }

        public void FollowRoute(string bikeID)
        {
            SendAndReceiveMessage(Route.FollowRoute(this.tunnelID, this.routeID, bikeID));
        }

        public void FollowRouteSpeed(string bikeID, double speed)
        {
            SendAndReceiveMessage(Route.FollowSpeed(tunnelID, speed, bikeID));
        }

        object sendLock = new object();
        private byte[] SendAndReceiveMessage(string msg)
        {
            //encode message to send to server
            byte[] encodedMsg = Encoding.ASCII.GetBytes(msg);
            //Console.WriteLine(Encoding.ASCII.GetString(encodedMsg));

            //create the buffer to send to server
            byte[] outputMessage = new byte[4 + encodedMsg.Length];

            byte[] outputSize = BitConverter.GetBytes(encodedMsg.Length).ToArray();

            //fill buffer with buffer size and message
            Array.Copy(outputSize, outputMessage, outputSize.Length);
            Array.Copy(encodedMsg, 0, outputMessage, 4, encodedMsg.Length);

            byte[] responseBytes;
            lock (sendLock)
            {
                //send message
                stream.Write(outputMessage, 0, outputMessage.Length);

                //read received response 
                responseBytes = new byte[4];
                stream.Read(responseBytes, 0, responseBytes.Length);
                int responseSize = BitConverter.ToInt32(responseBytes, 0);

                //Console.WriteLine(responseSize);

                responseBytes = new byte[responseSize];
                int receivedBuffer = 0;

                while (receivedBuffer < responseSize)
                {
                    receivedBuffer += stream.Read(responseBytes, receivedBuffer, responseSize - receivedBuffer);
                }
            }

            //Console.WriteLine(Encoding.ASCII.GetString(responseString));
            return responseBytes;
        }

        public string GetNodeID(string nodeName)
        {
            byte[] response = SendAndReceiveMessage(Node.GetNode(tunnelID, nodeName));

            JObject responseObject = JObject.Parse(Encoding.ASCII.GetString(response));


            //For debugging 
            //Console.WriteLine(responseObject.ToString());

            return (string)responseObject["data"]["data"]["data"][0]["uuid"];

        }

        public List<string> GetModel(){
            return modelList;
        }
    }
    
}
