using System.ComponentModel;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
namespace vr;

// Base engine class to add other components (Terrain/Models)
class Node
{
    private int nodeID;

    public static string AddTerrain(string tunnelID)
    {
        JObject jsonTerrain = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/node/add" },
                        {
                            "data", new JObject
                            {
                                { "name", "terrain" },
                                {"components", new JObject{
                                    {"terrain", new JObject{
                                    }}
                                }}
                            }
                        }
                    }}
                }
            }
            };

        return jsonTerrain.ToString();
    }

    public static string AddModel(string tunnelID, string fileName)
    {
        JObject jsonModel = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/node/add" },
                            {"data", new JObject
                                {
                                    { "name", "model" },
                                {"components", new JObject
                                {
                                    {"model", new JObject
                                    {
                                    { "file", fileName },
                                    { "cullbackfaces", true },
                                    { "animated", false },
                                    { "animation", "" }
                                    } }
                                } }

                                }
                            }

                    }}
                }
            }
            };

        return jsonModel.ToString();
    }

    public static string AddPanel(string tunnelID, string fileName)
    {
        JObject jsonPanel = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/node/add" },
                            {"data", new JObject
                                {
                                    { "name", "panel" },
                                {"components", new JObject
                                {
                                    {"panel", new JObject
                                    {
                                    { "size", JArray.Parse("[1,1]") },
                                    { "resolution", JArray.Parse("[512,512]") },
                                    { "background", JArray.Parse("[1,1,1,1]") },
                                    { "castShadow", true }
                                    } }
                                } }

                                }
                            }

                    }}
                }
            }
        };   
        return jsonPanel.ToString();
    }
}