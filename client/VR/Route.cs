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
                                {"nodes", JArray.Parse("[{\"pos\":[-50,0,-50],\"dir\":[50,0,-50]},{\"pos\":[50,0,-50],\"dir\":[50,0,50]},{\"pos\":[50,0,50],\"dir\":[-50,0,50]},{\"pos\":[-50,0,50],\"dir\":[-50,0,-50]},]")}
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
                                { "speed", 0.0 },
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

    public static string FollowSpeed(string tunnelID, double speed, string nodeid)
    {
        JObject json = new JObject
        {
            { "id", "tunnel/send" },
            {"data", new JObject
                { {"dest", tunnelID},
                {"data", new JObject {
                        { "id", "route/follow/speed" },
                        {
                            "data", new JObject
                            {
                                { "node", nodeid },
                                { "speed", speed },
                            }
                        }
                        }
                    }
                }
            }
        };

        return json.ToString();
    }
}