using System.IO;
using System.Net.Sockets;
using System.Net;
using ChatTCP.Classes.Logger;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Windows;

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

        public void StartClient(string serverIp)
        {
            tcpClient = new TcpClient();

            //TODO: try catch
            IPAddress ip = IPAddress.Parse(serverIp);
            tcpClient.Connect(new IPEndPoint(ip, DataHolder.Port));
            NetworkStream stream = tcpClient.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream);
            if (Writer is null || Reader is null)
            {
                throw new Exception("Error while getting Stream");
            }
            //END
            Task.Run(ReceiveMessageCoroutine);

            ConsoleLogger.Log("Client started");
        }
        public async Task SendMessageAsync(string MessageToSend)
        {
            PacketDTO packet = new PacketDTO()
            {
                command = Glossary.message,
                message = MessageToSend
            };
            string jsonString = JsonConvert.SerializeObject(packet);
            await Writer.WriteLineAsync(jsonString);
            await Writer.FlushAsync();
            if(packet.command == null)
            {
                MessageLogger.Log(MessageToSend);
            }
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
                    string? message = await Reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(message))
                    {
                        continue;
                    }
                    else
                    {
                        MessageLogger.Log(message);
                    }
                }
                catch(Exception ex) 
                {
                    Debug.WriteLine(ex);
                    break;
                }
            }
        }
        public void Close()
        {
            SendMessageAsync(new PacketDTO()
            {
                command = Glossary.disconnect
            });
            Writer.Close();
            Reader.Close();
            tcpClient.Close();
        }
    }
}
