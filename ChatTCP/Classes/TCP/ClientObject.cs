using System.IO;
using System.Net.Sockets;
using System.Net;
using ChatTCP.Classes.Logger;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ChatTCP.Classes.TCP
{
    internal class ClientObject
    {
        protected internal readonly string Id = Guid.NewGuid().ToString();
        public StreamReader? Reader { get; set; } = null;
        public StreamWriter? Writer { get; set; } = null;
        public TcpClient tcpClient { get; set; }
        ILogger ConsoleLogger { get; set; }
        ILogger MessageLogger { get; set; }

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

            try
            {
                IPAddress ip = IPAddress.Parse(serverIp);
                tcpClient.Connect(new IPEndPoint(ip, DataHolder.Port));
                NetworkStream stream = tcpClient.GetStream();
                Reader = new StreamReader(stream);
                Writer = new StreamWriter(stream);
                if (Writer is null || Reader is null)
                {
                    throw new Exception("Error while getting Stream");
                }
                Task.Run(ReceiveMessageCoroutine);

                ConsoleLogger.Log("Client started");
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log(ex.Message);
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
        private async Task ReceiveMessageCoroutine()
        {
            while (true)
            {
                try
                {
                    string? response = await Reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(response))
                    {
                        continue;
                    }
                    else
                    {
                        PacketDTO packet = JsonConvert.DeserializeObject<PacketDTO>(response);
                        if (packet != null)
                        {
                            if(packet.command == TCPCommand.message)
                            {
                                MessageLogger.Log(packet.message);
                            }
                            else if(packet.command == TCPCommand.disconnect)
                            {
                                Close();
                                Disconnected?.Invoke(this, EventArgs.Empty);
                                ConsoleLogger.Log("Server disconnected");
                            }
                        }
                    }
                }
                catch(Exception ex) 
                {
                    Debug.WriteLine(ex);
                    break;
                }
            }
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
    }
}
