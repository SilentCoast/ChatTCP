using System.Net.Sockets;
using System.Net;
using ChatTCP.Classes;
using System.Diagnostics;

namespace ChatTCP
{
    public class ServerObject
    {
        private readonly ILogger logger;
        
        TcpListener tcpListener = new TcpListener(IPAddress.Any, DataHolder.port); // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения
        public ServerObject(ILogger logger)
        {
            this.logger = logger;
        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null) clients.Remove(client);
            client?.Close();
        }
        // прослушивание входящих подключений
        protected internal async Task ListenAsync()
        {
            try
            {
                tcpListener.Start();
                
                logger.Log("Server started");
                while (true)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    clients.Add(clientObject);
                    Task.Run(clientObject.ProcessAsync);
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

        // трансляция сообщения подключенным клиентам
        protected internal async Task BroadcastMessageAsync(string message, string id)
        {
            foreach (var client in clients)
            {
                if (client.Id != id) // если id клиента не равно id отправителя
                {
                    await client.Writer.WriteLineAsync(message); //передача данных
                    await client.Writer.FlushAsync();
                }
            }
        }
        // отключение всех клиентов
        protected internal void Disconnect()
        {
            foreach (var client in clients)
            {
                client.Close(); //отключение клиента
            }
            tcpListener.Stop(); //остановка сервера
        }
    }
}