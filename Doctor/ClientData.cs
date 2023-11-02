using System;
using System.Collections.ObjectModel;

namespace Doctor
{
    public class ClientData
    {
        public ObservableCollection<SpeedData> SpeedData { get; set; } 
        public ObservableCollection<HeartBeatData> HeartBeatData { get; set; }
        public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new ObservableCollection<ChatMessage>();

        public DateTime LastTime { get; set; }

        public ClientData(int clientId)
        {
            SpeedData = new ObservableCollection<SpeedData>();
            HeartBeatData = new ObservableCollection<HeartBeatData>();  
            LastTime = DateTime.UnixEpoch;
            this.clientId = clientId;
        }

        private int clientId;
        public int ClientId { get { return clientId; } }


    }

    public class SpeedData
    {
        public uint speed { get; set; }
        public DateTime time { get; set; }

        public SpeedData(uint speed, DateTime time)
        {
            this.speed = speed;
            this.time = time;
        }

        public override string ToString()
        {
            return this.speed + " - " + this.time;
        }
    }
    public class HeartBeatData
    {
        public uint heartBeat { get; set; }
        public DateTime time { get; set; }

        public HeartBeatData(uint heartBeat, DateTime time)
        {
            this.heartBeat = heartBeat;
            this.time = time;
        }

        public override string ToString()
        {
            return this.heartBeat + " - " + this.time;
        }
    }
}
