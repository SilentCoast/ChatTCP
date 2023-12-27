using System.IO;
using System.Net.Sockets;
using ChatTCP.Classes.TCP;

namespace ChatTCP
{
    public class ClientObjectOLD
    {
        protected internal readonly string Id = Guid.NewGuid().ToString();
        protected internal StreamWriter Writer { get; }
        protected internal StreamReader Reader { get; }

        TcpClient client;
        ServerObject server; // объект сервера

        public ClientObjectOLD(TcpClient tcpClient, ServerObject serverObject)
        {
            client = tcpClient;
            server = serverObject;
            // получаем NetworkStream для взаимодействия с сервером
            NetworkStream stream = client.GetStream();
            // создаем StreamReader для чтения данных
            Reader = new StreamReader(stream);
            // создаем StreamWriter для отправки данных
            Writer = new StreamWriter(stream);
        }

        //public async Task ProcessAsync()
        //{
        //    try
        //    {
        //        // получаем имя пользователя
        //        string? userName = await Reader.ReadLineAsync();
        //        string? message = $"{userName} вошел в чат";
        //        // посылаем сообщение о входе в чат всем подключенным пользователям
        //        await server.BroadcastMessageAsync(message, Id);
        //        // в бесконечном цикле получаем сообщения от клиента
        //        while (true)
        //        {
        //            try
        //            {
        //                message = await Reader.ReadLineAsync();
        //                if (message == null) continue;
        //                message = $"{userName}: {message}";
        //                await server.BroadcastMessageAsync(message, Id);
        //            }
        //            catch
        //            {
        //                message = $"{userName} покинул чат";
        //                await server.BroadcastMessageAsync(message, Id);
        //                break;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new NotImplementedException();
        //    }
        //    finally
        //    {
        //        // в случае выхода из цикла закрываем ресурсы
        //        server.RemoveConnection(Id);
        //    }
        //}
        // закрытие подключения
        public void Close()
        {
            Writer.Close();
            Reader.Close();
            client.Close();
        }
    }
}