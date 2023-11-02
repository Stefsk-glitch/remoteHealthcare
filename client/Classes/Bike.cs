using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace client.Classes
{
    public class Bike
    {
        static CommandSender  sender;
        public Bike(CommandSender commandSender)
        {
            sender = commandSender;
        }

        private NetworkStream stream;

        public NetworkStream GetStream()
        {
            return stream;
        }

        public async Task Connect()
        {
            string serverIpAddress = "127.0.0.1";
            int serverPort = 9000;

            TcpClient tcpClient = new TcpClient();

            await tcpClient.ConnectAsync(serverIpAddress, serverPort);

            stream = tcpClient.GetStream();
        }

        public async Task Listen()
        {
                while (true)
                {
                    byte[] responseBytes = new byte[4];
                    stream.Read(responseBytes, 0, responseBytes.Length);
                    uint responseSize = BitConverter.ToUInt32(responseBytes, 0);

                    //Console.WriteLine(responseSize);

                    responseBytes = new byte[responseSize];
                    int receivedBuffer = 0;

                    while (receivedBuffer < responseSize)
                    {
                        receivedBuffer += stream.Read(responseBytes, receivedBuffer, (int)(responseSize - receivedBuffer));
                    }

                    int index = Array.IndexOf(responseBytes, (byte)0);
                    if (index < 0) continue;

                    String service = Encoding.UTF8.GetString(responseBytes, 0, index);

                    byte[] dataBytes = new byte[responseSize - index - 1];
                    Array.Copy(responseBytes, index + 1, dataBytes, 0, responseSize - index - 1);

                    String data = ConvertHexToDecimalString(dataBytes);

                    /*Console.WriteLine(service + " " + data);*/
                }
        }

        public static string ConvertHexToDecimalString(byte[] hexData)
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
                GetSpeed(hexData);
                return decimalString.ToString();
            }

            return null;
        }

        // Assumes last byte is checksum
        public static bool VerifyChecksum(byte[] bytes)
        {
            byte sum = CalculateChecksum(bytes);
            return sum == bytes[bytes.Length - 1];
        }

        public static byte CalculateChecksum(byte[] bytes)
        {
            byte sum = 0;
            for (int i = 0; i < bytes.Length - 1; ++i)
            {
                sum ^= bytes[i];
            }
            return sum;
        }

        public static void GetSpeed(byte[] data)
        {
            Console.WriteLine("{0} K/h", BitConverter.ToUInt16(data, 8) * 0.001 * 3.6);
            sender.SendData((byte)(BitConverter.ToUInt16(data, 8) * 0.001 * 3.6), 2);
        }

        public static async void SetResistanceAsync(int res, BLE bleBike)
        {
            byte[] data = new byte[13];
            data[0] = 164;
            data[1] = 9;
            data[2] = 78;
            data[3] = 5;
            data[4] = 48;
            data[11] = (byte)res;

            byte checksum = CalculateChecksum(data);

            data[data.Length - 1] = checksum;

            await bleBike.WriteCharacteristic("6e40fec3-b5a3-f393-e0a9-e50e24dcca9e", data);
        }
    }
}
