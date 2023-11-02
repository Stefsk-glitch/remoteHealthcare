using System.Runtime.Serialization;

namespace Server
{
    [Serializable]
    class ClientData
    {
        public delegate void SendDataCallback(DataType type, int clientId, DateTime time, uint data);

        [NonSerialized] private List<SendDataCallback> callbacks = new List<SendDataCallback>();

        public int ID { get; set; }
        public List<HeartBeatData> HeartbeatList { get; set; } = new List<HeartBeatData>();
        public List<SpeedData> SpeedList { get; set; } = new List<SpeedData>();

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            callbacks = new List<SendDataCallback>();
        }

        public void AddCallback(SendDataCallback callback)
        {
            callbacks.Add(callback);
        }

        public void RemoveCallback(SendDataCallback callback)
        {
            callbacks.Remove(callback);
        }

        public void AddHeartBeat(HeartBeatData heartBeat)
        {
            foreach (SendDataCallback callback in callbacks)
            {
                callback.Invoke(DataType.HeartBeat, ID, heartBeat.Time, heartBeat.HeartBeat);
            }
            HeartbeatList.Add(heartBeat);
        }
        public void AddSpeed(SpeedData speed)
        {
            foreach (SendDataCallback callback in callbacks)
            {
                callback.Invoke(DataType.Speed, ID, speed.Time, speed.Speed);
            }
            SpeedList.Add(speed);
        }
    }

    [Serializable]
    class HeartBeatData
    {
        public uint HeartBeat { get; set; }
        public DateTime Time { get; set; }

        public HeartBeatData(uint heartBeat, DateTime time)
        {
            this.HeartBeat = heartBeat;
            this.Time = time;
        }

        public override string ToString()
        {
            return this.HeartBeat + " - " + this.Time;
        }
    }

    [Serializable]
    class SpeedData
    {
        public uint Speed { get; set; }
        public DateTime Time { get; set; }

        public SpeedData(uint speed, DateTime time)
        {
            this.Speed = speed;
            this.Time = time;
        }

        public override string ToString()
        {
            return this.Speed + " - " + this.Time;
        }
    }

    public enum DataType
    {
        HeartBeat,
        Speed,
    }
}
