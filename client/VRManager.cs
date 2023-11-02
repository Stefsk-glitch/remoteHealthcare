using client.Classes;
using vr;

namespace client
{
    internal class VRManager
    {
        private Terrain terrain;
        private VirtualReality VR;
        private string camera;
        private string dataPanel;
        private string chatPanel;

        internal VRManager()
        {
            terrain = new Terrain(256,256);
            terrain.CreateTerrainData();
            VR = new VirtualReality("20193159");
            VR.CreateTerrain(terrain);
            camera = VR.GetNodeID("Camera");
            VR.AddRoute();
            VR.AddModel("data/NetworkEngine/models/bike/bike.fbx", 0,0,0,1,0,0,0, "bike");
            VR.AddModel("data/NetworkEngine/models/trees/fantasy/tree3.obj", -40,2,-50,1,0,0,14, "tree");
            VR.AddModel("data/NetworkEngine/models/houses/set1/house1.obj", 20,3,20,6,0,0,0, "house");
            VR.AddModel("data/NetworkEngine/models/houses/set1/house2.obj", -50, 4,-70,6,5,0,0, "house2");
            VR.FollowRoute(VR.GetModel().First());
            VR.UpdateCamera(camera, VR.GetModel().First());
            dataPanel = VR.AddPanel("Bike speed:", camera, "data", -2, 2, 0);
            chatPanel = VR.AddPanel("", camera, "chat", -2, 1, 0);
        }

        public void SpeedCallback(double speed)
        {
            VR.FollowRouteSpeed(VR.GetModel().First(), speed);
            VR.RefreshPanel($"Bike speed: {speed:0.0}", dataPanel);
        }
        
        public void ChatCallback(string message)
        {
            VR.RefreshPanel(message, chatPanel);

        }
        internal void Run()
        {
        }
    }
}