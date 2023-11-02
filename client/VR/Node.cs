using System.ComponentModel;
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
                                    {"transform", new JObject
                                        {
                                            {"position", JArray.Parse("[ -128, 0, -128 ]")},
				                            {"scale", 1},
				                            {"rotation", JArray.Parse("[ 0, 0, 0 ]")}
                                        }
                                    },
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

    public static string AddModel(string tunnelID, string fileName, int x, int y, int z, int scale, int rotX, int rotY, int rotZ, string name)
    {
        JObject jsonModel = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/node/add" },
                            {"data", new JObject
                                {
                                { "name", name },
                                {"components", new JObject
                                {
                                    {"transform", new JObject
                                        {
                                            {"position", JArray.Parse("[ " + x + ", " + y +", " + z + "]")},
				                            {"scale", scale},
				                            {"rotation", JArray.Parse("[ " + rotX + ", " + rotY +", " + rotZ + "]")}
                                        }
                                    },
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

    public static string AddModelWithParent(string tunnelID, string parentID, string fileName, int x, int y, int z)
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
                                    {"parent", parentID},
                                {"components", new JObject
                                {
                                    {"transform", new JObject
                                        {
                                            {"position", JArray.Parse("[ " + x + ", " + y +", " + z + "]")},
				                            {"scale", 1},
				                            {"rotation", JArray.Parse("[ 0, 0, 0 ]")}
                                        }
                                    },
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

    public static string GetNode(string tunnelID, string nodeName){
        JObject sceneGet = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { 
                        {"dest", tunnelID},
                        {"data", new JObject{
	                        {"id", "scene/node/find"},
                            {"data", new JObject{
                                {"name", nodeName}
                            }}
                        }

                        }

                    }
                 }
        };

        return sceneGet.ToString();
    }

    public static string AddPanel(string tunnelID, string cameraID, int x, int y, int z, string name)
    {
        JObject jsonPanel = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/node/add" },
                            {"data", new JObject
                                {
                                    { "name", name},
                                    {"parent", cameraID},
                                {"components", new JObject
                                {

                                    {"transform", new JObject
                                        {
                                            {"position", JArray.Parse("[ " + x + ", " + y +", " + z + "]")},
				                            {"scale", 1},
				                            {"rotation", JArray.Parse("[ 0, 0, 0 ]")}
                                        }
                                    },

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

    public static string AddTerrainLayer(string tunnelID, string terrainID){
        JObject jsonTerrainLayer = new JObject{
            { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        {"id", "scene/node/addlayer"},
                            {"data", new JObject
                                {
                                    {"id", terrainID},
                                    {"diffuse", "data/NetworkEngine/textures/grass_diffuse.png"},
                                    {"normal", "data/NetworkEngine/textures/grass_normal.png"},
                                    {"minHeight", 0},
                                    {"maxHeight", 16},
                                    {"fadeDist", 1}
                                }
                            }
                        }
                    }
                }
            }
        };



        return jsonTerrainLayer.ToString();        
    }

    public static string UpdateNode(string tunnelID, string nodeID, string parentID){
        JObject camera = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/node/update" },
                            {"data", new JObject{
                                {"id", nodeID},
                                {"parent", parentID},
                                {"transform", new JObject
                                        {
                                            {"position", JArray.Parse("[ 0, 0, 0]")},
                                            {"scale", 1.0},
                                            {"rotation", JArray.Parse("[ 0, 90, 0]")},
				                            
                                        }
                                }
                            }
                            }
                    }
                }
            }
                    
        }
        };

        return camera.ToString();
    }

    public static string GetScene(string tunnelID){
        JObject scene = new JObject{
            { "id", "tunnel/send" },
            {"data", new JObject
                { 
                    {"dest", tunnelID},
                    {"data", new JObject{
                            { "id", "scene/get" }
                        }
                    }
                }
            }
        };

        return scene.ToString();
    }
}