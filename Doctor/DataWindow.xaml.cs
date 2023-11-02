using LiveCharts.Wpf.Charts.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace Doctor
{
    public partial class DataWindow : Window
    {
        private ConnectionManager connectionManager;
        private ObservableCollection<ClientData> ClientIds { get; set; }
    
        private int clientId = -999999;

        public DataWindow(ConnectionManager connectionManager)
        {
            InitializeComponent();
            this.connectionManager = connectionManager;
            connectionManager.addClientIdsCallback(fillClientIds);
            connectionManager.addSpeedDataListCallback(SpeedListCallback);
            connectionManager.addHeartBeatDataListCallback(HeartBeatListCallback);
            connectionManager.addSpeedDataCallback(SpeedCallback);
            connectionManager.addHeartBeatDataCallback(HeartBeatCallback);
            connectionManager.SendMessage(4, new byte[0]);

            ClientIds = new ObservableCollection<ClientData>();
            lstbxClients.ItemsSource = ClientIds;

            
            

            SendButton.Click += btn1_Click;
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            if (clientId == -999999)
            {
                return;
            }
            connectionManager.sendChatMessage(clientId, TextBox.Text);



            ChatMessage chatMessage = new(TextBox.Text);

            ClientData client = GetClientData(clientId);
            client.ChatMessages.Add(chatMessage);
            TextBox.Clear();
        }

        private ClientData GetClientData(int clientId)
        {

            foreach (var client in ClientIds)
            {
                if (client.ClientId == clientId)
                {
                    return client;
                }
            }
            return null;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClientData data = ((ClientData)lstbxClients.SelectedItem);
            Trace.WriteLine(data.ClientId);

            foreach (ClientData client in ClientIds)
            {
                if (client.ClientId == data.ClientId) data = client;
            }

            connectionManager.getClientData(data.ClientId, ((DateTimeOffset)data.LastTime).ToUnixTimeSeconds());

            clientId = data.ClientId;
            ChatListView.ItemsSource = GetClientData(clientId).ChatMessages;
            ChartWindow chartWindow = new ChartWindow(data);
            chartWindow.Show();
        }

        private void fillClientIds(int[] clientIds)
        {
            this.ClientIds.Clear();
            foreach (int clientId in clientIds)
                this.ClientIds.Add(new ClientData(clientId));
        }

        private void SpeedListCallback(int clientId, List<SpeedData> speedData)
        {
            foreach (var client in ClientIds)
            {
                if (client.ClientId == clientId)
                {
                    foreach (var speed in speedData)
                    {
                        if (client.LastTime < speed.time) client.LastTime = speed.time;
                        client.SpeedData.Add(speed);
                    }
                }
            }
        }

        private void SpeedCallback(int clientId, SpeedData speedData)
        {
            foreach (var client in ClientIds)
            {
                if (client.ClientId == clientId)
                {
                    if (client.LastTime < speedData.time) client.LastTime = speedData.time;
                    client.SpeedData.Add(speedData);
                }
            }
        }

        private void HeartBeatListCallback(int clientId, List<HeartBeatData> heartBeatData)
        {
            foreach (var client in ClientIds)
            {
                if (client.ClientId == clientId)
                {
                    foreach (var heartBeat in heartBeatData)
                    {
                        if (client.LastTime < heartBeat.time) client.LastTime = heartBeat.time;
                        client.HeartBeatData.Add(heartBeat);
                    }
                }
            }
        }
        private void HeartBeatCallback(int clientId, HeartBeatData heartBeatData)
        {
            foreach (var client in ClientIds)
            {
                if (client.ClientId == clientId)
                {
                    if (client.LastTime < heartBeatData.time) client.LastTime = heartBeatData.time;
                    client.HeartBeatData.Add(heartBeatData);
                }
            }
        }
    }
}
