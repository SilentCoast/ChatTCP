﻿using ChatTCPlib.Logger;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ChatTCPlib.TCP
{
    public class ClientObject
    {
        public readonly string Id = Guid.NewGuid().ToString();
        public StreamReader? Reader { get; set; } = null;
        public StreamWriter? Writer { get; set; } = null;
        public TcpClient tcpClient { get; set; }
        ILogger ConsoleLogger { get; set; }
        ILogger MessageLogger { get; set; }
        bool ConnectionOk { get; set; } = true;
        bool ConnectionIsLost { get; set; }
        string Username { get; set; }

        public ClientObject(ILogger consoleLogger, ILogger messageLogger)
        {
            ConsoleLogger = consoleLogger;
            MessageLogger = messageLogger;
            
        }
        public ClientObject(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            NetworkStream stream = tcpClient.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream);
        }

        public bool StartClient(string serverIp)
        {
            tcpClient = new TcpClient();
            Username = NetworkInfo.GetLocalIPAddress();
            try
            {
                IPAddress ip = IPAddress.Parse(serverIp);
                tcpClient.Connect(new IPEndPoint(ip, NetworkInfo.Port));
                NetworkStream stream = tcpClient.GetStream();
                Reader = new StreamReader(stream);
                Writer = new StreamWriter(stream);
                if (Writer is null || Reader is null)
                {
                    throw new Exception("Error while getting Stream");
                }
                Task.Run(ReceiveMessageCoroutine);
                Task.Run(CheckConnectionCoroutine);
                SendMessageAsync(Username);
                ConsoleLogger.Log("Client started");
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log("Failed to connect");
                return false;
            }
            return true;
        }
        public async Task SendMessageAsync(string MessageToSend)
        {
            PacketDTO packet = new PacketDTO()
            {
                command = TCPCommand.message,
                message = MessageToSend
            };
            string jsonString = JsonConvert.SerializeObject(packet);
            await Writer.WriteLineAsync(jsonString);
            await Writer.FlushAsync();
        }
        public async Task SendMessageAsync(PacketDTO packet)
        {
            string jsonString = JsonConvert.SerializeObject(packet);
            await Writer.WriteLineAsync(jsonString);
            await Writer.FlushAsync();
        }
        private async Task CheckConnectionCoroutine()
        {
            await Task.Delay(3000);
            while (true)
            {
                if (ConnectionOk)
                {
                    ConnectionOk = false;
                    await SendMessageAsync(new PacketDTO { command = TCPCommand.connectionCheck });

                    await Task.Delay(3000);
                    if (ConnectionOk == false)
                    {
                        //if we didn't recieve any messages in 3 sec connection has been lost
                        ConnectionLost();
                    }
                }
            }
        }
        private async Task ReceiveMessageCoroutine()
        {
            while (true)
            {
                try
                {
                    string? response = await Reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(response))
                    {
                        ConnectionOk = true;
                        if (ConnectionIsLost)
                        {
                            ConnectionResumed();
                        }
                        PacketDTO packet = JsonConvert.DeserializeObject<PacketDTO>(response);
                        if (packet != null)
                        {
                            if (packet.command == TCPCommand.message)
                            {
                                MessageLogger.Log(packet.message);
                            }
                            else if (packet.command == TCPCommand.disconnect)
                            {
                                Close();
                                Disconnected?.Invoke(this, EventArgs.Empty);
                                ConsoleLogger.Log("Server disconnected");
                            }
                        }
                    }
                }
                catch
                {
                    Debug.WriteLine("Failed to recieve message from the server");
                    break;
                }
            }
        }

        private void ConnectionResumed()
        {
            ConnectionIsLost = false;
            ConsoleLogger.Log("Connection resumed");
        }

        private void ConnectionLost()
        {
            ConnectionIsLost = true;
            ConsoleLogger.Log("Connection lost");
            ConnectionLostEvent?.Invoke(this, EventArgs.Empty);
            Reconnect();
        }
        private void Reconnect()
        {
            ConsoleLogger.Log("Attemp to reconnect...");
            TryReconnect?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Disconects this TCPClient
        /// </summary>
        public void Close()
        {
            SendMessageAsync(new PacketDTO()
            {
                command = TCPCommand.disconnect
            });
            Writer.Close();
            Reader.Close();
            tcpClient.Close();
        }
        public event EventHandler Disconnected;
        public event EventHandler TryReconnect;
        public event EventHandler ConnectionLostEvent;
    }
}
