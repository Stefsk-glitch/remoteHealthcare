using System.Globalization;
using Newtonsoft.Json.Linq;


namespace vr;

class Route {

    public static string AddRoute(string tunnelID) {
        JObject jsonRoute = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "route/add" },
                        {
                            "data", new JObject
                            {
                                {"nodes", JArray.Parse("[{\"pos\":[1,0,1],\"dir\":[5,0,-5]},{\"pos\":[50,0,1],\"dir\":[5,0,5]},{\"pos\":[50,0,50],\"dir\":[-5,0,5]},{\"pos\":[1,0,50],\"dir\":[-5,0,-5]},]")}
                            }
                        }
                        }
                    }
                    }
                }
            };

        return jsonRoute.ToString();

    }

    public static string FollowRoute(string tunnelID, string routeid, string nodeid)
    {
        
        JObject jsonRoute = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "route/follow" },
                        {
                            "data", new JObject
                            {
                                { "route", routeid },
                                { "node", nodeid },
                                { "speed", 20.0 },
                                { "offset", 0.0 },
                                { "rotate", "XZ" },
                                { "smoothing", 1.0 },
                                { "followHeight", true },
                                { "rotateOffset", new JArray { 0, 0, 0 } },
                                { "positionOffset", new JArray { 0, 0, 0} }
                            }
                        }
                        }
                    }
                    }
                }
            };

        return jsonRoute.ToString();
    }
}