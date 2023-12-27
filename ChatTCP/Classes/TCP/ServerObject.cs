using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using ChatTCP.Classes.Logger;
using Newtonsoft.Json;

namespace ChatTCP.Classes.TCP
{
    public class ServerObject
    {
        private readonly ILogger logger;

        TcpListener tcpListener = new TcpListener(IPAddress.Any, DataHolder.Port); 
        List<ClientObject> clients = new List<ClientObject>(); 
        public ServerObject(ILogger logger)
        {
            this.logger = logger;
            ListenAsync();
        }
        protected internal void RemoveConnection(string id)
        {
            ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                clients.Remove(client);
            }
            client?.Close();
        }
        protected internal async Task ListenAsync()
        {
            try
            {
                tcpListener.Start();

                logger.Log("Server started");
                while (true)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                    ClientObject client = new ClientObject(tcpClient);
                    clients.Add(client);
                    Task.Run(() => ProcessAsync(client.Reader, client.Id));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                Disconnect();
            }
        }

        protected internal async Task BroadcastMessageAsync(string message, string id)
        {
            foreach (var client in clients)
            {
                //if (client.Id != id) 
                {
                    await client.Writer.WriteLineAsync(message); 
                    await client.Writer.FlushAsync();
                }
            }
        }
        protected internal void Disconnect()
        {
            foreach (var client in clients)
            {
                client.Close(); 
            }
            tcpListener.Stop(); 
        }
        public async Task ProcessAsync(StreamReader reader, string id)
        {
            try
            {
                string? jsonString = await reader.ReadLineAsync();
                PacketDTO packet = JsonConvert.DeserializeObject<PacketDTO>(jsonString);
                string? userName = packet.message;
                string? message = $"{userName} joined";
                await BroadcastMessageAsync(message, id);
                while (true)
                {
                    try
                    {
                        jsonString = await reader.ReadLineAsync();
                        if (jsonString == null) continue;

                        packet = JsonConvert.DeserializeObject<PacketDTO>(jsonString);
                        
                        if (packet.command == TCPCommand.message)
                        {
                            message = $"{userName}: {packet.message}";
                            await BroadcastMessageAsync(message, id);
                        }
                        else if(packet.command == TCPCommand.disconnect)
                        {
                            message = $"{userName} disconnected";
                            await BroadcastMessageAsync(message, id);
                            break;
                        }
                    }
                    catch
                    {

                        message = $"Connection with {userName} has been lost";
                        Debug.WriteLine(message);
                        await BroadcastMessageAsync(message, id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new NotImplementedException();
            }
            finally
            {
                RemoveConnection(id);
            }
        }
    }
}