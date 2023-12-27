using ChatTCP.Classes;
using ChatTCP.Classes.Extensions;
using ChatTCP.Classes.Logger;
using ChatTCP.Classes.TCP;
using PropertyChanged;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Threading;

namespace ChatTCP.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        private bool isServer;
        public bool IsServer { get => isServer; set { isServer = value; IsServerChanged(); } }
        public bool IsConnected { get; set; }
        public string ServerIp {  get; set; }
        public string ConsoleText {  get; set; }
        public string MessagesText {  get; set; }
        public string MessageToSend { get; set; }
        ClientObject Client { get; set; }
        ServerObject Server { get; set; }
        ILogger ConsoleLogger { get; set; }
        ILogger MessageLogger { get; set; }
        ILogger ErrorLogger { get; set; }
        public MainViewModel(ILogger ErrorLogger)
        {
            ConsoleLogger = new Logger();
            ConsoleLogger.Logged += ConsoleLogger_Logged;
            MessageLogger = new Logger();
            MessageLogger.Logged += MessageLogger_Logged;
            this.ErrorLogger = ErrorLogger;
            Client = new ClientObject(ConsoleLogger, MessageLogger);
        }
        private RelayCommand connect;
        public RelayCommand Connect => connect ?? (connect = new RelayCommand(async p =>
        {
            if(!IsConnected)
            {
                if (IsServer)
                {
                    Server = new ServerObject(ConsoleLogger);
                    Client.StartClient(ServerIp);
                }
                else
                {
                    Client.StartClient(ServerIp);
                }
                IsConnected = true;
                CheckConnectionCoroutine();
            }
            else
            {
                Server?.Disconnect();
                Client.Close();
                IsConnected = false;
            }
        }));
        private async Task CheckConnectionCoroutine()
        {
            while(true)
            {
                Debug.WriteLine(Client.tcpClient.GetState().ToString());
                await Task.Delay(1000);
            }
        }
        private RelayCommand sendMessage;

        public RelayCommand SendMessage => sendMessage ?? (sendMessage = new RelayCommand(async p =>
        {
            if (!string.IsNullOrEmpty(MessageToSend))
            {
                await Client.SendMessageAsync(MessageToSend);
                MessageToSend = string.Empty;
            }
        }));
        private void IsServerChanged()
        {
            if (IsServer)
            {
                ServerIp = GetLocalIPAddress();
            }
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        private void MessageLogger_Logged(object sender, MessageEventArgs e)
        {
            MessagesText += e.Message;
            InvokeScrollToBottom();
        }

        private void ConsoleLogger_Logged(object sender, MessageEventArgs e)
        {
            ConsoleText += e.Message;
            InvokeScrollToBottom();
        }
        private void InvokeScrollToBottom()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ScrollToBottomRequested?.Invoke(null, EventArgs.Empty);
            }));
        }
        public event EventHandler ScrollToBottomRequested;
    }
}
