using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Doctor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private readonly SynchronizationContext _syncContext;
        private readonly ConnectionManager connection;

        public MainWindow()
        {
            InitializeComponent();

            _syncContext = SynchronizationContext.Current;
            
            string serverAddress = "127.0.0.1";
            int serverPort = 8000;
            
            TcpClient client = new TcpClient(serverAddress, serverPort);
            
            SslStream sslStream = new SslStream(client.GetStream(), false, CertificateValidation);
            sslStream.AuthenticateAsClient("localhost", null, SslProtocols.None, true);
            
            connection = new ConnectionManager(sslStream, _syncContext, this);

            List<uint> initialKmList = new List<uint> { 1, 2, 3, 4, 5 };
            List<uint> initialHeartList = new List<uint> { 70, 72, 75, 68, 80 };

            tbUser.Focus();
        }

        private static bool CertificateValidation(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
        {
            if (cert.GetCertHashString().Equals("A9C26B7E4FCA6974C3B7BA3C5ADEA1C7F35C259B")) return true;

            return false;
        }

        private void BtnOnLoginClick(object sender, EventArgs e)
        {
            Login(tbUser.Text, tbPass.Password);
        }

        private void Login(string userString, string passString)
        {
            byte[] user = Encoding.UTF8.GetBytes(userString);
            byte[] pass = Encoding.UTF8.GetBytes(passString);

            byte[] message = new byte[user.Length + pass.Length + 2];
            message[0] = 1; // doctor
            Array.Copy(user, 0, message, 1, user.Length);
            message[user.Length + 1] = 0;
            Array.Copy(pass, 0, message, user.Length + 2, pass.Length);
            connection.SendMessage(3, message);
        }

        public void LoadDataWindow(bool login)
        {
            string messageBoxText = "Login Failed";
            string messageBoxTextgood = "Login Succes";
            string caption = "Login";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxImage icongood = MessageBoxImage.None;
            MessageBoxResult result;

            if (login)
            {
                result = MessageBox.Show(messageBoxTextgood, caption, button, icongood, MessageBoxResult.Yes);
                DataWindow window = new DataWindow(connection);
                window.Show();
                this.Close();
            }
            else
            {
                result = MessageBox.Show(messageBoxText, caption, button, icongood, MessageBoxResult.Yes);
                MainWindow window = new MainWindow();
                window.Show();
                this.Close();
            }
        }

        private void LoginBoxKeyHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login(tbUser.Text, tbPass.Password);
            }
        }
    }
}
