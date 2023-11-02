namespace client;

public class Program
{
    public delegate void speedCallback(double speed);
    public delegate void heartrateCallback(int heartrate);
    public delegate void chatCallback(string message);
    public delegate void resistanceCallback(int clientId, byte resistance);
    public delegate void runningCallback(bool value);

    public static void Main()
    {
        HardwareServer hs = new HardwareServer();
        RemoteServer server = new RemoteServer("Localhost");
        VRManager vr = new VRManager();

        hs.AddSpeedCallback(server.SpeedCallback);
        hs.AddSpeedCallback(vr.SpeedCallback);
        
        hs.AddSpeedCallback(server.SpeedCallback);
        hs.AddSpeedCallback(vr.SpeedCallback);
        server.AddChatCallback(vr.ChatCallback);
        server.AddResistanceCallback(hs.ResistanceCallback);
        server.AddRunningCallback(hs.SetRunning);
        server.AddSpeedCallback(vr.SpeedCallback);

        Thread vrThread = new Thread(vr.Run);
        Thread hsThread = new Thread(hs.Run);
        Thread serverThread = new Thread(server.Run);

        vrThread.Start();
        hsThread.Start();
        serverThread.Start();
    }
}
