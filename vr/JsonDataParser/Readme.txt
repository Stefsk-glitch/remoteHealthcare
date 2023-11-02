using JsonParser;

namespace HelloWorld
{
    class Hello
    {
        static void Main(string[] args)
        {
            string jsonString = "{\"items\":[{\"id\":\"123\",\"clientinfo\":{\"user\":\"John\"}}, {\"id\":\"456\",\"clientinfo\":{\"user\":\"Jane\"}}, {\"id\":\"789\",\"clientinfo\":{\"user\":\"John\"}}]}";
            string userNameToFind = "Jane";

            List<DataItem> userItems = JsonDataParser.FindUserItems(jsonString, userNameToFind);

            if (userItems.Count > 0)
            {
                foreach (var item in userItems)
                {
                    Console.WriteLine($"User: {item.clientinfo.user}, ID: {item.id}");
                    Console.WriteLine(JsonDataParser.CreateTunnel(item.id));
                }
            }
            else
            {
                Console.WriteLine($"No user with the name '{userNameToFind}' found.");
            }
        }
    }
}