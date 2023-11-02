using System.Text.Json;

namespace JsonParser
{
    public class JsonDataParser
    {
        public static List<DataItem> ExtractDataItems(string jsonString)
        {
            // Deserialize the JSON-string to a DataObject
            Data dataObject = JsonSerializer.Deserialize<Data>(jsonString);

            return dataObject.data;
        }

        public static List<DataItem> FindUserItems(string jsonString, string userName)
        {
            List<DataItem> userItems = new List<DataItem>();
            List<DataItem> dataItems = ExtractDataItems(jsonString);

            foreach (var item in dataItems)
            {
                if (item.clientinfo.user == userName)
                {
                    userItems.Add(item);
                }
            }

            return userItems;
        }

        public static string CreateJsonString(Tunnel tunnel)
        {
            // Serialize the Tunnel object to JSON
            string jsonString = JsonSerializer.Serialize(tunnel);

            return jsonString;
        }

        public static string CreateTunnel(string sessionGiven)
        {
            var tunnel = new Tunnel
            {
                id = "tunnel/create",
                data = new TunnelData
                {
                    session = sessionGiven,
                    key = ""
                }
            };

            return CreateJsonString(tunnel);
        }

        public static string GetTunnelID(string jsonString)
        {
            Console.WriteLine(jsonString);
            return jsonString;
        }
    }
}