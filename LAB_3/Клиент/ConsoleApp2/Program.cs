using System.Net.Sockets;
using System.Text; // Подключаем пространство имен для работы с сетевыми сокетами
DateTime date1 = DateTime.Now;
Dictionary<char, string> dictWindows1251 = new Dictionary<char, string>
{
       {'А', "0xC0"}, {'Б', "0xC1"}, {'В', "0xC2"}, {'Г', "0xC3"}, {'Д', "0xC4"}, {'Е', "0xC5"}, {'Ж', "0xC6"}, {'З', "0xC7"},
    {'И', "0xC8"}, {'Й', "0xC9"}, {'К', "0xCA"}, {'Л', "0xCB"}, {'М', "0xCC"}, {'Н', "0xCD"}, {'О', "0xCE"}, {'П', "0xCF"},
    {'Р', "0xD0"}, {'С', "0xD1"}, {'Т', "0xD2"}, {'У', "0xD3"}, {'Ф', "0xD4"}, {'Х', "0xD5"}, {'Ц', "0xD6"}, {'Ч', "0xD7"},
    {'Ш', "0xD8"}, {'Щ', "0xD9"}, {'Ъ', "0xDA"}, {'Ы', "0xDB"}, {'Ь', "0xDC"}, {'Э', "0xDD"}, {'Ю', "0xDE"}, {'Я', "0xDF"},
    {'а', "0xE0"}, {'б', "0xE1"}, {'в', "0xE2"}, {'г', "0xE3"}, {'д', "0xE4"}, {'е', "0xE5"}, {'ж', "0xE6"}, {'з', "0xE7"},
    {'и', "0xE8"}, {'й', "0xE9"}, {'к', "0xEA"}, {'л', "0xEB"}, {'м', "0xEC"}, {'н', "0xED"}, {'о', "0xEE"}, {'п', "0xEF"},
    {'р', "0xF0"}, {'с', "0xF1"}, {'т', "0xF2"}, {'у', "0xF3"}, {'ф', "0xF4"}, {'х', "0xF5"}, {'ц', "0xF6"}, {'ч', "0xF7"},
    {'ш', "0xF8"}, {'щ', "0xF9"}, {'ъ', "0xFA"}, {'ы', "0xFB"}, {'ь', "0xFC"}, {'э', "0xFD"}, {'ю', "0xFE"}, {'я', "0xFF"},
    {'Ё', "0xA8"}, {'ё', "0xB8"},
    {'A', "0x41"}, {'B', "0x42"}, {'C', "0x43"}, {'D', "0x44"}, {'E', "0x45"}, {'F', "0x46"}, {'G', "0x47"}, {'H', "0x48"},
    {'I', "0x49"}, {'J', "0x4A"}, {'K', "0x4B"}, {'L', "0x4C"}, {'M', "0x4D"}, {'N', "0x4E"}, {'O', "0x4F"}, {'P', "0x50"},
    {'Q', "0x51"}, {'R', "0x52"}, {'S', "0x53"}, {'T', "0x54"}, {'U', "0x55"}, {'V', "0x56"}, {'W', "0x57"}, {'X', "0x58"},
    {'Y', "0x59"}, {'Z', "0x5A"}, {'a', "0x61"}, {'b', "0x62"}, {'c', "0x63"}, {'d', "0x64"}, {'e', "0x65"}, {'f', "0x66"},
    {'g', "0x67"}, {'h', "0x68"}, {'i', "0x69"}, {'j', "0x6A"}, {'k', "0x6B"}, {'l', "0x6C"}, {'m', "0x6D"}, {'n', "0x6E"},
    {'o', "0x6F"}, {'p', "0x70"}, {'q', "0x71"}, {'r', "0x72"}, {'s', "0x73"}, {'t', "0x74"}, {'u', "0x75"}, {'v', "0x76"},
    {'w', "0x77"}, {'x', "0x78"}, {'y', "0x79"}, {'z', "0x7A"}, {'0', "0x30"}, {'1', "0x31"}, {'2', "0x32"}, {'3', "0x33"},
    {'4', "0x34"}, {'5', "0x35"}, {'6', "0x36"}, {'7', "0x37"}, {'8', "0x38"}, {'9', "0x39"},
    {' ', "0x20"}, {'.', "0x2E"}, {',', "0x2C"}, {'!', "0x21"}, {'?', "0x3F"}
};

 string sh(string message)
{
    string res = "";
    int n = message.Length;
    for (int i = 0; i < n; i++)
    {
        if (dictWindows1251.ContainsKey(message[i]))
        {
            res += dictWindows1251[message[i]];
        }
    }
    return res;
}

string host = "127.0.0.1"; // Задаем хост для подключения (локальный компьютер)
int port = 8888; // Задаем порт для подключения
using TcpClient client = new TcpClient(); // Создаем новый экземпляр TcpClient
Console.Write("Введите свое имя: "); // Запрашиваем у пользователя имя
string? userName = Console.ReadLine(); // Считываем имя пользователя
Console.WriteLine($"Добро пожаловать, {userName}"); // Приветствуем пользователя
StreamReader? Reader = null; // Инициализируем StreamReader для чтения данных
StreamWriter? Writer = null; // Инициализируем StreamWriter для записи данных

try
{
    client.Connect(host, port); // Подключаем клиента к указанному хосту и порту
    Reader = new StreamReader(client.GetStream()); // Инициализируем StreamReader для потока
    Writer = new StreamWriter(client.GetStream()); // Инициализируем StreamWriter для потока
    if (Writer is null || Reader is null) return; // Проверяем, были ли успешно созданы StreamReader и StreamWriter
    // Запускаем новый поток для получения данных
    Task.Run(() => ReceiveMessageAsync(Reader)); // Запускаем асинхронный метод для получения сообщений
    // Запускаем ввод сообщений
    await SendMessageAsync(Writer); // Вызываем метод для отправки сообщений
}
catch (Exception ex) // Обрабатываем исключения
{
    Console.WriteLine(ex.Message); // Выводим сообщение об ошибке
}
Writer?.Close(); // Закрываем StreamWriter, если он был инициализирован
Reader?.Close(); // Закрываем StreamReader, если он был инициализирован

// Отправка сообщений
async Task SendMessageAsync(StreamWriter writer) // Асинхронный метод для отправки сообщений
{
    // Сначала отправляем имя
    await writer.WriteLineAsync(userName); // Отправляем имя пользователя
    await writer.FlushAsync(); // Очищаем буфер для отправки данных
    Console.WriteLine("Для отправки сообщений введите сообщение и нажмите Enter"); // Подсказываем пользователю, что делать

    while (true) // Бесконечный цикл для ввода сообщений
    {
        string? message = Console.ReadLine(); // Считываем сообщение от пользователя
        await writer.WriteLineAsync(message); // Отправляем сообщение на сервер
        await writer.FlushAsync(); // Очищаем буфер для отправки данных
    }
}

// Получение сообщений
async Task ReceiveMessageAsync(StreamReader reader) // Асинхронный метод для получения сообщений
{
    while (true) // Бесконечный цикл для получения сообщений
    {
        try
        {
            // Считываем ответ в виде строки
            string? message = await reader.ReadLineAsync(); // Асинхронно считываем строку из потока
            // Если пустой ответ, ничего не выводим на консоль
            if (string.IsNullOrEmpty(message)) continue; // Пропускаем итерацию, если сообщение пустое
            Print(message); // Выводим сообщение на консоль
        }
        catch // Обрабатываем любые исключения
        {
            break; // Выходим из цикла в случае ошибки
        }
    }
}

// Чтобы полученное сообщение не накладывалось на ввод нового сообщения
void Print(string message) // Метод для печати сообщений на консоль
{
    StringBuilder binaryString = new StringBuilder();
    foreach (char c in message)
    {
        binaryString.Append(Convert.ToString(c, 2).PadLeft(8, '0') + " ");
    }
    if (OperatingSystem.IsWindows()) // Если ОС Windows
    {
        var position = Console.GetCursorPosition(); // Получаем текущую позицию курсора
        int left = position.Left; // Смещение в символах относительно левого края
        int top = position.Top; // Смещение в строках относительно верха
        // Копируем ранее введенные символы в строке на следующую строку
        Console.MoveBufferArea(0, top, left, 1, 0, top + 1); // Перемещаем область буфера консолей 
        // Устанавливаем курсор в начало текущей строки
        Console.SetCursorPosition(0, top); // Устанавливаем курсор в нужное место
        // В текущей строке выводим полученное сообщение
        Console.WriteLine('[' + date1.ToString() + ']' + '{' + binaryString.ToString().Trim() + '}' + ' ' + '-' + ' ' + '(' + message + ')' + ' ' + sh(message)); // Печатаем сообщение
                                                                                                                                      // Переносим курсор на следующую строку
                                                                                                                                      // И пользователь продолжает ввод уже на следующей строке

        Console.SetCursorPosition(left, top + 1); // Устанавливаем курсор на следующую строку
    }
    else Console.WriteLine('[' + date1.ToString() + ']' + '{' + binaryString.ToString().Trim() + '}' + ' ' + '-' + ' ' + '(' + message + ')' + ' ' + sh(message)); // В противном случае выводим сообщение для других ОС
}

