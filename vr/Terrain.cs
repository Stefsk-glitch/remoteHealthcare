using System.Data.SqlTypes;
using Newtonsoft.Json.Linq;
namespace vr;

class Terrain{
    private int terrainWidth;
    private int terrainLength;
    private double[] terrainData;

    public Terrain(int width, int length){
        this.terrainWidth = width;
        this.terrainLength = length;
        terrainData = new double[width*length];
    }

    public void CreateTerrainData(){
        // Create and configure FastNoise object
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetSeed(69);                        

        // Gather noise data and put it in terrain data
        int index = 0;

        for (int y = 0; y < terrainWidth; y++)
        {
            for (int x = 0; x < terrainLength; x++)
            {
                //Max height is 256. 
                //Noise returns values between -1 and 1 so we add 1 and multiply it by 127 (Max will be 254);
                terrainData[index++] = (noise.GetNoise(x, y) + 1.0f) * 5;
            }
        }
    }

    public string CreateTerrainJson(string tunnelID){
        JArray jTerrainSize = new JArray();
        jTerrainSize.Add(terrainWidth);
        jTerrainSize.Add(terrainLength);

        JArray jTerrainData = new JArray();
        foreach(var b in terrainData){
            jTerrainData.Add(b);
        }
        

        JObject jsonTerrain = new JObject{
                { "id", "tunnel/send" },
                {"data", new JObject
                    { {"dest", tunnelID},
                    {"data", new JObject{
                        { "id", "scene/terrain/add" },
                        {
                            "data", new JObject
                            {
                                { "size", jTerrainSize }, 
                                { "heights", jTerrainData }
                            }
                        }
                    }}
                }
            }
            };

    
        return jsonTerrain.ToString();
    }

    public void UpdateTerrainJson(){
        //TODO
    }

    public void DeleteTerrainJson(){
        //TODO
    }

    public int GetTerrainHeight(int x, int y){
        return (int)terrainData[y + x*256];    
    }

}