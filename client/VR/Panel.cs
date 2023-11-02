using Newtonsoft.Json.Linq;
namespace vr;


class Panel{

    public static string DrawText(string tunnelID, string nodeID, string text){
        JObject jsonPanel = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/panel/drawtext" },
                            {"data", new JObject{
                                {"id", nodeID},
		                        {"text", text},
		                        {"position", JArray.Parse("[ 100.0, 100.0 ]")},
		                        {"size", 32.0},
		                        {"color", JArray.Parse("[ 0,0,0,1 ]")},
		                        {"font", "segoeui"}
                            }
                            }
                        }
                    }
                }
            }
        };

        


        return jsonPanel.ToString();
    }

    
    public static string SwapPanelBuffer(string tunnelID, string nodeID){
        JObject jsonPanel = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/panel/swap" },
                            {"data", new JObject{
                                {"id", nodeID},
                            }
                            }
                        }
                    }
                }
            }
        };

        return jsonPanel.ToString();
    }

    public static string ClearPanelBuffer(string tunnelID, string nodeID){
        JObject jsonPanel = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/panel/clear" },
                            {"data", new JObject{
                                {"id", nodeID},
                            }
                            }
                        }
                    }
                }
            }
        };

        return jsonPanel.ToString();
    }

}