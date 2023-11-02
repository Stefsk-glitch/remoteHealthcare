using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;
using System.Text;
using System.Threading;

namespace Doctor
{
    public class ConnectionManager
    {
        private SslStream sslStream;
        private SynchronizationContext _syncContext;
        MainWindow mainWindow;
        private List<int> clients = new List<int>();

        public delegate void clientIdsCallback(int[] clientIds);
        private clientIdsCallback? clientIds;

        public delegate void speedDataListCallback(int clientId, List<SpeedData> speedData);
        public delegate void heartBeatDataListCallback(int clientId, List<HeartBeatData> heartBeatData);
        public delegate void speedDataCallback(int clientId, SpeedData speedData);
        public delegate void heartBeatDataCallback(int clientId, HeartBeatData heartBeatData);

        private speedDataListCallback? speedDataList;
        private heartBeatDataListCallback? heartBeatDataList;

        private speedDataCallback? speedData;
        private heartBeatDataCallback? heartBeatData;

        public ConnectionManager(SslStream sslStream, SynchronizationContext _syncContext, MainWindow mainWindow)
        {
            this.sslStream = sslStream;
            this._syncContext = _syncContext;
            this.mainWindow = mainWindow;
            Thread t = new Thread(() => Listen(sslStream));
            t.IsBackground = true;
            t.Start();

        }

        public void sendChatMessage(int clientId,string message)
        {
            byte[] m = new byte[Encoding.UTF8.GetBytes(message).Length + 4];
            BitConverter.GetBytes(clientId).CopyTo(m, 0);
            Encoding.UTF8.GetBytes(message).CopyTo(m, 4);

            SendMessage(12, m);
        }

        public void SendMessage(byte type, byte[] message)
        {
            uint len = (uint)(message.Length + 1);

            sslStream.Write(BitConverter.GetBytes(len));
            sslStream.WriteByte(type);
            sslStream.Write(message);
            sslStream.Flush();

        }

        public void addClientIdsCallback(clientIdsCallback callback)
        {
            clientIds = callback;
        }

        public void addSpeedDataListCallback(speedDataListCallback callback)
        {
            speedDataList = callback;
        }

        public void addHeartBeatDataListCallback(heartBeatDataListCallback callback)
        {
            heartBeatDataList = callback;
        }
        public void addSpeedDataCallback(speedDataCallback callback)
        {
            speedData = callback;
        }

        public void addHeartBeatDataCallback(heartBeatDataCallback callback)
        {
            heartBeatData = callback;
        }

        private void Listen(SslStream sslStream)
        {
            while (true)
            {
                byte[] responseBytes = new byte[4];
                sslStream.Read(responseBytes, 0, responseBytes.Length);
                uint responseSize = BitConverter.ToUInt32(responseBytes, 0);

                responseBytes = new byte[responseSize];
                int receivedBuffer = 0;

                while (receivedBuffer < responseSize)
                {
                    receivedBuffer += sslStream.Read(responseBytes, receivedBuffer, (int)(responseSize - receivedBuffer));
                }

                switch (responseBytes[0])
                {
                    case 5:
                        uint len = BitConverter.ToUInt32(responseBytes, 1);

                        int[] clientIds = new int[len];
                        for (int i = 0; i < len; i++)
                        {
                            clientIds[i] = BitConverter.ToInt32(responseBytes, 5 + i * 4);
                        }
                        
                        if (this.clientIds != null) _syncContext.Post(o => this.clientIds.Invoke(clientIds), null);
                        break;
                    case 6:
                        _syncContext.Post(o => mainWindow.LoadDataWindow(responseBytes[1] != 0), null);
                        break;
                    case 8:
                        int clientId = BitConverter.ToInt32(responseBytes, 1);
                        uint heartBeat = BitConverter.ToUInt32(responseBytes, 5);
                        long timeLong = BitConverter.ToInt64(responseBytes, 9);
                        if (this.heartBeatData != null) _syncContext.Post(o => this.heartBeatData.Invoke(clientId, new HeartBeatData(heartBeat, DateTimeOffset.FromUnixTimeSeconds(timeLong).UtcDateTime)), null);
                        break;
                    case 9:
                        clientId = BitConverter.ToInt32(responseBytes, 1);
                        uint speed = BitConverter.ToUInt32(responseBytes, 5);
                        timeLong = BitConverter.ToInt64(responseBytes, 9);
                        if (this.speedData != null) _syncContext.Post(o => this.speedData.Invoke(clientId, new SpeedData(speed, DateTimeOffset.FromUnixTimeSeconds(timeLong).UtcDateTime)), null);
                        break;
                    case 10: //Heartbeat
                        clientId = BitConverter.ToInt32(responseBytes, 1);
                        int count = (responseBytes.Length - 5) / 12;
                        List<HeartBeatData> heartBeatData = new List<HeartBeatData>();
                        for (int i = 0; i < count; ++i)
                        {
                            speed = BitConverter.ToUInt32(responseBytes, 5 + i * 12);
                            DateTime time = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt64(responseBytes, 9 + i * 12)).UtcDateTime;
                            heartBeatData.Add(new HeartBeatData(speed, time));
                        }
                        if (this.heartBeatDataList != null) _syncContext.Post(o => this.heartBeatDataList.Invoke(clientId, heartBeatData), null);
                        break;
                    case 11: //speedMessage
                        clientId = BitConverter.ToInt32(responseBytes, 1);
                        count = (responseBytes.Length - 5) / 12;
                        List<SpeedData> speedData = new List<SpeedData>();
                        for (int i = 0; i < count; ++i)
                        {
                            heartBeat = BitConverter.ToUInt32(responseBytes, 5 + i * 12);
                            DateTime time = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt64(responseBytes, 9 + i * 12)).UtcDateTime;
                            speedData.Add(new SpeedData(heartBeat, time));
                        }
                        if (this.speedDataList != null) _syncContext.Post(o => this.speedDataList.Invoke(clientId, speedData), null);
                        break;
                }
            }
        }

        public void getClientData(int clientId, long time)
        {
            byte[] message = new byte[12];
            BitConverter.GetBytes(clientId).CopyTo(message, 0);
            BitConverter.GetBytes(time).CopyTo(message, 4);
            SendMessage(7, message);
        }

        public void RequestClientIds()
        {
            SendMessage(4, new byte[0]);
        }
    }
}
