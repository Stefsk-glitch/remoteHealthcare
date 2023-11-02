using Newtonsoft.Json.Linq;

namespace vr;


class Road{


    public static string AddRoad(string tunnelID, string routeID){
        JObject jsonRoad = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/road/add" },
                        {
                            "data", new JObject
                            {
                                {"route", routeID},
		                        {"diffuse", "data/NetworkEngine/textures/tarmac_diffuse.png"},
		                        {"normal", "data/NetworkEngine/textures/tarmac_normal.png"},
		                        {"specular", "data/NetworkEngine/textures/tarmac_specular.png"},
		                        {"heightoffset", 0.01}
                            }
                        }
                        }
                    }
                    }
                }
            };
        
        
        return jsonRoad.ToString();


    }



}