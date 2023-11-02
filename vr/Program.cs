using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using JsonParser;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace vr
{
    class Program
    {
        static void Main()
        {
            string tunnelID = "";
            List<string> modelList = new List<String>();
            string routeID = "";
            TcpClient tcpClient = new TcpClient("145.48.6.10", 6666);
            NetworkStream stream = tcpClient.GetStream();
            Terrain terrain = new Terrain(256,256);
            terrain.CreateTerrainData();
            Node nodeBuilder = new Node();

            byte[] jsonInBytes = SendAndReceiveMessage("{\"id\":\"session/list\"}", stream);

            string jsonString = Encoding.ASCII.GetString(jsonInBytes);

            string userNameToFind = "Matheus";

            List<DataItem> userItems = JsonDataParser.FindUserItems(jsonString, userNameToFind);

            if (userItems.Count > 0)
            {
                foreach (var item in userItems)
                {
                    // creating tunnel with tunnel id
                    Console.WriteLine($"User: {item.clientinfo.user}, ID: {item.id}");
                    Console.WriteLine(JsonDataParser.CreateTunnel(item.id));
                    byte[] response = SendAndReceiveMessage(JsonDataParser.CreateTunnel(item.id), stream);
                    Console.WriteLine(response);

                    // for some reason we get the tunnel ID. Isn't this already item.id?
                    JObject responseObject = JObject.Parse(Encoding.ASCII.GetString(response));
                    tunnelID = (string)responseObject["data"]["id"];


                    response = SendAndReceiveMessage(terrain.CreateTerrainJson(tunnelID), stream);
                    response = SendAndReceiveMessage(Node.AddTerrain(tunnelID), stream);
                    response = SendAndReceiveMessage(Route.AddRoute(tunnelID), stream);
                    
                    responseObject = JObject.Parse(Encoding.ASCII.GetString(response));
                    Console.WriteLine(responseObject.ToString());
                    routeID = (string)responseObject["data"]["data"]["data"]["uuid"];
                    
                    response = SendAndReceiveMessage(Road.AddRoad(tunnelID, routeID), stream);

                    response = SendAndReceiveMessage(Node.AddModel(tunnelID, "data/NetworkEngine/models/trees/fantasy/tree3.obj"), stream);
                    responseObject = JObject.Parse(Encoding.ASCII.GetString(response));
                    Console.WriteLine(responseObject);
                    modelList.Add((string)responseObject["data"]["data"]["data"]["uuid"]);

                    SendAndReceiveMessage(Route.FollowRoute(tunnelID,routeID,modelList.First()), stream);

                    //foreach (var model in modellist)
                    //{
                    //    console.writeline(model);
                    //}

                }
            }
            else
            {
                Console.WriteLine($"No user with the name '{userNameToFind}' found.");
            }
        }

        static byte[] SendAndReceiveMessage(string msg, NetworkStream stream)
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

            //send message
            stream.Write(outputMessage, 0, outputMessage.Length);

            //read received response 
            byte[] responseString = new byte[4];
            stream.Read(responseString, 0, responseString.Length);
            int responseSize = BitConverter.ToInt32(responseString, 0);

            //Console.WriteLine(responseSize);

            responseString = new byte[responseSize];
            int receivedBuffer = 0;
        
            while (receivedBuffer < responseSize)
            {
                receivedBuffer += stream.Read(responseString, receivedBuffer, responseSize - receivedBuffer);
            }

            //Console.WriteLine(Encoding.ASCII.GetString(responseString));
            return responseString;
        }
    }
}