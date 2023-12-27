using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using ChatTCP.Classes.Logger;

namespace ChatTCP.Classes.TCP
{
    public class ServerObject
    {
        private readonly ILogger logger;

        TcpListener tcpListener = new TcpListener(IPAddress.Any, DataHolder.Port); // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения
        public ServerObject(ILogger logger)
        {
            this.logger = logger;
            ListenAsync();
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

                    //Client clientObject = new Client(tcpClient, this);
                    //clients.Add(clientObject);
                    Task.Run(() => ProcessAsync(new StreamReader(tcpClient.GetStream()), Guid.NewGuid().ToString()));
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
        public async Task ProcessAsync(StreamReader reader, string id)
        {
            try
            {
                // получаем имя пользователя
                string? userName = await reader.ReadLineAsync();
                string? message = $"{userName} вошел в чат";
                // посылаем сообщение о входе в чат всем подключенным пользователям
                await BroadcastMessageAsync(message, id);
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = await reader.ReadLineAsync();
                        if (message == null) continue;
                        message = $"{userName}: {message}";
                        await BroadcastMessageAsync(message, id);
                    }
                    catch
                    {
                        message = $"{userName} покинул чат";
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
                // в случае выхода из цикла закрываем ресурсы
                RemoveConnection(id);
            }
        }
    }
}