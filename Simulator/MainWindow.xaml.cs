using Simulator;
using System;
using System.Diagnostics;
using System.Windows;

namespace HeartRateBikeSpeedApp
{
    public partial class MainWindow : Window
    {
        private double heartRate = 80; // Starting heart rate
        private double bikeSpeed = 10; // Starting bike speed in km/hour
        private Random random = new Random();
        private Stopwatch stopwatch;
        private bool scenario = false;
        private TCPServer server;

        public MainWindow()
        {
            InitializeComponent();
            // Start a timer to update values every second
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(UpdateValues);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            timer.Start();
            stopwatch = new Stopwatch();

            server = new TCPServer();
        }

        private void UpdateValues(object? sender, EventArgs e)
        {
            // Simulate a gradual change in heart rate and bike speed
            int heartRateChange = random.Next(-2, 3); // Change between -2 and 2 bpm
            int bikeSpeedChange = random.Next(-1, 2); // Change between -1 and 1 km/hour

            double temp = stopwatch.ElapsedMilliseconds;

            if (temp > 6300)
            {
                stopwatch.Stop();
                stopwatch.Reset();
                scenario = false;
            }

            if (!scenario)
            {
                // Ensure that heart rate and bike speed stay within realistic bounds
                heartRate = Math.Round(Math.Max(60, Math.Min(110, heartRate + heartRateChange)));
                bikeSpeed = Math.Round(Math.Max(5, Math.Min(20, bikeSpeed + bikeSpeedChange)));

            }
            else
            {

                // Ensure that heart rate and bike speed stay within realistic bounds
                heartRate = Math.Round(Math.Max(60, Math.Min(240, heartRate + heartRateChange + Math.Sin(temp * 0.0005f))));
                bikeSpeed = Math.Round(Math.Max(5, Math.Min(50, bikeSpeed + bikeSpeedChange + Math.Sin(temp * 0.0005f))));
            }
            // Update the labels with the generated values
            heartRateLabel.Content = "Heart Rate: " + heartRate + " bpm";
            bikeSpeedLabel.Content = "Bike Speed: " + bikeSpeed + " km/hour";

            UInt16 speed = (UInt16)(bikeSpeed / 3.6 * 1000);

            byte[] data = new byte[] { 164, 9, 78, 5, 16, 25, 0/*elapsed time*/, 0/*distance traveled*/, 1, 1/*speed*/, 0/*heart rate*/, 0/*capabilities/state*/, 0 };
            BitConverter.GetBytes(speed).CopyTo(data, 8);

            byte checksum = TCPServer.CalculateChecksum(data);
            data[data.Length - 1] = checksum;

            server.SendDataToClient(data, "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            stopwatch.Start();
            scenario = true;
        }
    }

}
