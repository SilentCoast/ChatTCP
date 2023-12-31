﻿using ChatTCP.Classes;
using ChatTCPlib;
using ChatTCPlib.Logger;
using ChatTCPlib.TCP;
using PropertyChanged;
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
            CreateNewClient();
        }
        private void CreateNewClient()
        {
            Client = new ClientObject(ConsoleLogger, MessageLogger);
            Client.Disconnected += Client_Disconnected;
            Client.TryReconnect += Client_TryRecconect;
            Client.ConnectionLostEvent += Client_ConnectionLostEvent;
        }
        private void Client_ConnectionLostEvent(object? sender, EventArgs e)
        {
            Connect.Execute(this);
            CreateNewClient();
        }

        private void Client_TryRecconect(object? sender, EventArgs e)
        {
            Connect.Execute(this);
        }

        private void Client_Disconnected(object? sender, EventArgs e)
        {
            IsConnected = false;
        }

        private RelayCommand connect;
        public RelayCommand Connect => connect ?? (connect = new RelayCommand(async p =>
        {
            //TODO: fix freeze of UI when waiting to time out when connecting to non existent server
            if (IsConnected)
            {
                Server?.Disconnect();
                Client.Close();
                IsConnected = false;
            }
            else
            {
                if (IsServer)
                {
                    Server = new ServerObject(ConsoleLogger);
                }
                CreateNewClient();
                if (Client.StartClient(ServerIp))
                {
                    IsConnected = true;
                }
            }
        }));
        
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
                ServerIp = NetworkInfo.GetLocalIPAddress();
            }
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
