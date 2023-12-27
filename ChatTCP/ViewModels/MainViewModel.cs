using ChatTCP.Classes;
using PropertyChanged;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;

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

        StreamReader? _reader = null;
        StreamWriter? _writer = null;
        TcpClient _client;
        ServerObject server;
        ILogger ConsoleLogger { get; set; }
        ILogger MessageLogger { get; set; }
        public MainViewModel()
        {
            ConsoleLogger = new Logger();
            ConsoleLogger.Logged += ConsoleLogger_Logged;
            MessageLogger = new Logger();
            MessageLogger.Logged += MessageLogger_Logged;
        }
        

        private RelayCommand connect;
        public RelayCommand Connect => connect ?? (connect = new RelayCommand(p =>
        {
            if(!IsConnected)
            {
                if (IsServer)
                {
                    StartServer();
                    StartClient();
                }
                else
                {
                    StartClient();
                }
                IsConnected = true;
            }
            else
            {
                server.Disconnect();
                IsConnected = false;
            }
            
        }));
        private RelayCommand sendMessage;

        public RelayCommand SendMessage => sendMessage ?? (sendMessage = new RelayCommand(p =>
        {
            SendMessageAsync(_writer);
        }));
        
        private async void StartServer()
        {
            server = new ServerObject(ConsoleLogger);
            await server.ListenAsync();
        }
        private async void StartClient()
        {
            int port = 8888;
            _client = new();
            
            try
            {
                _client.Connect(new IPEndPoint(Convert.ToInt64(ServerIp), port));
                //_client.Connect(ServerIp, port); //подключение клиента
                _reader = new StreamReader(_client.GetStream());
                _writer = new StreamWriter(_client.GetStream());
                if (_writer is null || _reader is null) return;

                Task.Run(() => ReceiveMessageCoroutine(_reader));
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            ConsoleLogger.Log("Client started");
        }
        private async Task SendMessageAsync(StreamWriter writer)
        {
            MessageLogger.Log(MessageToSend);
            await writer.WriteLineAsync(MessageToSend);
            await writer.FlushAsync();
            MessageToSend = string.Empty;
        }
        private async Task ReceiveMessageCoroutine(StreamReader reader)
        {
            while (true)
            {
                try
                {
                    string? message = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(message)) 
                    { 
                        continue; 
                    }
                    else
                    {
                        MessagesText += "\n" + message;
                    }
                }
                catch
                {
                    break;
                }
            }
        }
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

        ~MainViewModel()
        {
            _writer?.Close();
            _reader?.Close();
            _client.Dispose();
        }
    }
}
