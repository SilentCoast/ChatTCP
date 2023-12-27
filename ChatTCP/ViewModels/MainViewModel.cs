using ChatTCP.Classes;
using ChatTCP.Classes.Logger;
using ChatTCP.Classes.TCP;
using PropertyChanged;
using System.IO;
using System.Net;
using System.Net.Sockets;

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
        public MainViewModel()
        {
            ConsoleLogger = new Logger();
            ConsoleLogger.Logged += ConsoleLogger_Logged;
            MessageLogger = new Logger();
            MessageLogger.Logged += MessageLogger_Logged;
            Client = new ClientObject(ConsoleLogger, MessageLogger);
        }
        private RelayCommand connect;
        public RelayCommand Connect => connect ?? (connect = new RelayCommand(p =>
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
            }
            else
            {
                Server?.Disconnect();
                Client.Close();
                IsConnected = false;
            }
        }));
        private RelayCommand sendMessage;

        public RelayCommand SendMessage => sendMessage ?? (sendMessage = new RelayCommand(async p =>
        {
            await Client.SendMessageAsync(MessageToSend);
            MessageToSend = string.Empty;
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
        }

        private void ConsoleLogger_Logged(object sender, MessageEventArgs e)
        {
            ConsoleText += e.Message;
        }
    }
}
