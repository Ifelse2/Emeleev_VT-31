using System.Net;
using System.Net.Sockets;

ServerObject server = new ServerObject(); // создаем экземпляр сервера
await server.ListenAsync(); // запускаем асинхронное прослушивание подключений

class ServerObject
{
    TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888); // создаем сервер для прослушивания на порту 8888
    List<ClientObject> clients = new List<ClientObject>(); // создаем список для хранения подключенных клиентов

    protected internal void RemoveConnection(string id) // метод для удаления подключения по ID
    {
        ClientObject? client = clients.FirstOrDefault(c => c.Id == id); // ищем клиента по ID
        if (client != null) clients.Remove(client); // если клиент найден, удаляем его из списка
        client?.Close(); // закрываем соединение с клиентом
    }

    protected internal async Task ListenAsync() // метод для асинхронного прослушивания входящих подключений
    {
        try
        {
            tcpListener.Start(); // запускаем tcpListener
            Console.WriteLine("Сервер запущен. Ожидание подключений..."); // выводим сообщение о запуске сервера

            while (true) // бесконечный цикл для постоянного прослушивания
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync(); // ожидаем подключения клиента

                ClientObject clientObject = new ClientObject(tcpClient, this); // создаем объект клиента
                clients.Add(clientObject); // добавляем клиента в список
                Task.Run(clientObject.ProcessAsync); // запускаем обработку сообщений от клиента в отдельном потоке
            }
        }
        catch (Exception ex) // обработка исключений
        {
            Console.WriteLine(ex.Message); // выводим сообщение об ошибке
        }
        finally
        {
            Disconnect(); // отключаем всех клиентов и останавливаем сервер
        }
    }

    protected internal async Task BroadcastMessageAsync(string message, string id) // метод для трансляции сообщений клиентам
    {
        foreach (var client in clients) // проходимся по всем клиентам
        {
            if (client.Id != id) // если ID клиента не равен ID отправителя
            {
                await client.Writer.WriteLineAsync(message); // отправляем сообщение клиенту
                await client.Writer.FlushAsync(); // очищаем буфер для записи
            }
        }
    }

    protected internal void Disconnect() // метод для отключения всех клиентов
    {
        foreach (var client in clients) // проходимся по списку клиентов
        {
            client.Close(); // закрываем соединение с каждым клиентом
        }
        tcpListener.Stop(); // останавливаем tcpListener
    }
}

class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString(); // уникальный идентификатор клиента
    protected internal StreamWriter Writer { get; } // объект для записи данных
    protected internal StreamReader Reader { get; } // объект для чтения данных

    TcpClient client; // объект клиента
    ServerObject server; // ссылка на сервер

    public ClientObject(TcpClient tcpClient, ServerObject serverObject) // конструктор класса ClientObject
    {
        client = tcpClient; // присваиваем объект клиента
        server = serverObject; // присваиваем объект сервера
        var stream = client.GetStream(); // получаем поток для взаимодействия с сервером
        Reader = new StreamReader(stream); // инициализация StreamReader для чтения данных
        Writer = new StreamWriter(stream); // инициализация StreamWriter для отправки данных
    }

    public async Task ProcessAsync() // метод для обработки сообщений от клиента
    {
        try
        {
            string? userName = await Reader.ReadLineAsync(); // получаем имя пользователя
            string? message = $"{userName} вошел в чат"; // формируем сообщение о входе
            await server.BroadcastMessageAsync(message, Id); // рассылаем сообщение всем клиентам
            Console.WriteLine(message); // выводим сообщение на консоль

            while (true) // бесконечный цикл для получения сообщений от клиента
            {
                try
                {

                    message = await Reader.ReadLineAsync(); // ожидаем сообщение от клиента
                    if (message == null) continue; // продолжаем ожидание, если сообщение null
                    message = $"{userName}: {message}"; // формируем сообщение с именем пользователя
                    Console.WriteLine(message); // выводим сообщение на консоль
                    await server.BroadcastMessageAsync(message, Id); // рассылаем сообщение всем клиентам
                }
                catch // обрабатываем исключения при получении сообщений
                {
                    message = $"{userName} покинул чат"; // формируем сообщение о выходе
                    Console.WriteLine(message); // выводим сообщение на консоль
                    await server.BroadcastMessageAsync(message, Id); // рассылаем сообщение всем клиентам
                    break; // выходим из цикла
                }
            }
        }
        catch (Exception e) // обработка исключений
        {
            Console.WriteLine(e.Message); // выводим сообщение об ошибке
        }
        finally
        {
            server.RemoveConnection(Id); // закрываем соединение при выходе
        }
    }

    protected internal void Close() // метод для закрытия подключения
    {
        Writer.Close(); // закрываем StreamWriter
        Reader.Close(); // закрываем StreamReader
        client.Close(); // закрываем TcpClient
    }
}
