using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;
using System.IO;

namespace RemoteHealthcare
{
    class Program
    {
        private static TcpListener listener;
        private static TcpClient client;
        private static NetworkStream stream;
        private static BLE bleBike;

        private static readonly string ipAddress = "127.0.0.1"; // "145.49.43.10"
        private static readonly int port = 9000;
        private static readonly string bikeName = "Tacx Flux 01249";

        static async Task Main(string[] args)
        {
            int errorCode = 0;
            bleBike = new BLE();
            BLE bleHeart = new BLE();
            Thread.Sleep(1000); // We need some time to list available devices

            // List available devices
            List<string> bleBikeList = bleBike.ListDevices();
            Console.WriteLine("Devices found: ");
            foreach (var name in bleBikeList)
            {
                Console.WriteLine($"Device: {name}");
            }

            // Connecting
            while ((errorCode = await bleBike.OpenDevice(bikeName)) == 1)
            {
                Console.WriteLine("Couldn't open device {0}", bikeName);
            }

            var services = bleBike.GetServices;
            foreach (var service in services)
            {
                Console.WriteLine($"Service: {service.Name}");
            }

            // Set service
            errorCode = await bleBike.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
            // __TODO__ error check

            await SetResistance(0);

            // Subscribe
            bleBike.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            errorCode = await bleBike.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");


            listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            listener.Start();
            Console.WriteLine("Server started on {0}:{1}", ipAddress, port);

            client = listener.AcceptTcpClient();
            stream = client.GetStream();

            (new Thread(Listen)).Start();


            // Heart rate
            /*            errorCode =  await bleHeart.OpenDevice("Decathlon Dual HR");
                        if(errorCode == 1){
                            //Console.WriteLine("Couldn't open device Decathlon Dual HR");
                        }

                        await bleHeart.SetService("HeartRate");

                        bleHeart.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
                        await bleHeart.SubscribeToCharacteristic("HeartRateMeasurement");*/



            Console.Read();
        }

        private static async void Listen()
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (true)
                    {
                        uint length = reader.ReadUInt32();

                        byte[] receivedBytes = new byte[length];
                        int receivedBuffer = 0;

                        while (receivedBuffer < length)
                        {
                            receivedBuffer += reader.Read(receivedBytes, (int)receivedBuffer, (int)(length - receivedBuffer));
                        }

                        byte messageType = receivedBytes[0];

                        //Console.WriteLine($"Received data: {data}");
                        Console.WriteLine($"Received messageType: {messageType}");

                        switch (messageType)
                        {
                            case 14:
                                byte resistance = receivedBytes[5];
                                await SetResistance(resistance);
                                break;
                        }
                    }
                }
            }
            catch (IOException)
            {
            }
            finally
            {
                Console.WriteLine("Disconnected");
            }
        }

        private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            if (!VerifyChecksum(e.Data)) return;
            string data = ConvertHexToDecimalString(e.Data);
            if (data != null)
            {
                Console.WriteLine("Received from {0}: {1}", e.ServiceName, data);
                SendDataToClient(e.Data, e.ServiceName);
            }
        }

        private static string ConvertHexToDecimalString(byte[] hexData)
        {
            StringBuilder decimalString = new StringBuilder();

            byte page = (byte)(hexData[4] & 0x01111111);

            for (int i = 0; i < hexData.Length; i++)
            {
                decimalString.AppendFormat("{0:D3}", hexData[i]);

                if (i < hexData.Length - 1)
                {
                    decimalString.Append(" ");
                }
            }

            if (page.Equals(016))
            {
                return decimalString.ToString();
            }

            return null;
        }


        // Assumes last byte is checksum
        private static bool VerifyChecksum(byte[] bytes)
        {
            byte sum = CalculateChecksum(bytes);
            return sum == bytes[bytes.Length - 1];
        }

        private static byte CalculateChecksum(byte[] bytes)
        {
            byte sum = 0;
            for (int i = 0; i < bytes.Length - 1; ++i)
            {
                sum ^= bytes[i];
            }
            return sum;
        }

        private static void SendDataToClient(byte[] data, string serviceName)
        {
            if (stream == null) return;

            byte[] s = Encoding.UTF8.GetBytes(serviceName);

            uint len = (uint)(s.Length + data.Length + 1);
            stream.Write(BitConverter.GetBytes(len), 0, 4);
            stream.Write(s, 0, s.Length);
            stream.Write(new byte[] { 0 }, 0, 1);
            // Write the data to the client using the network stream
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        // resistance in 0.5%
        private static async Task SetResistance(byte resistance)
        {
            if (resistance < 0 || resistance > 200) return;

            Console.WriteLine("Setting resistance to {0}", resistance);

            byte[] data = new byte[] { 164, 09, 0x4E, 0x05, 0x30, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, resistance, 0xff };
            byte checksum = CalculateChecksum(data);

            data[data.Length - 1] = checksum;

            await bleBike.WriteCharacteristic("6e40fec3-b5a3-f393-e0a9-e50e24dcca9e", data);
        }
    }
}
