using System.IO;
using System.Net.Sockets;
using System.Net;
using ChatTCP.Classes.Logger;

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
        public void Close()
        {
            Writer.Close();
            Reader.Close();
            tcpClient.Close();
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
            await Writer.WriteLineAsync(MessageToSend);
            await Writer.FlushAsync();
            MessageLogger.Log(MessageToSend);
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
                catch
                {
                    break;
                }
            }
        }
    }
}
