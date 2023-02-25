using System.Net;
using System.Net.Sockets;
using System.Text;



IPAddress recieverAddress = IPAddress.Parse("127.0.0.1");
int sendPort=0;
int listenPort=0;
int id=0;
string? username;
ConsoleKeyInfo key = new ConsoleKeyInfo();
Dictionary<string, int> nameId = new Dictionary<string, int>();
string path = "";
string loadPath = "E:\\Labs\\ITIROD\\Client\\bin\\Debug\\net6.0";
string text = "";

void InitId()
{

    Random rnd = new Random();
    id = rnd.Next(0, 100);

    Console.Write("Name: ");
    username = Console.ReadLine();

    if(nameId.Count == 0)
    {
        nameId.Add(username.ToString(), id);
    }
}

void initPath()
{
    path = $"{listenPort},{username}.txt";
    FileInfo fileInfo = new FileInfo(loadPath + "\\" + path);
    if(fileInfo.Exists)
    {
        LoadHistory();
    }
    else
    {
        fileInfo.Create();
    }
}

async Task SaveHistory()
{
    using (StreamWriter writer = new StreamWriter(path, false))
    {
        await writer.WriteLineAsync(text);
    }
}

async Task LoadHistory()
{
   
    using (StreamReader reader = new StreamReader(loadPath + "\\" +path))
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            text += line + "\n";
            Console.WriteLine(line);
        }
    }
}

void DictionaryCheck()
{
    foreach (var names in nameId)
    {
        if (names.Key == username)
        {
            LoadHistory();
        }
        else
        {
            nameId.Add(username.ToString(), id);
        }
    }
}

while (key.Key != ConsoleKey.Escape)
{
    InitId();
    
    Console.Write("Listener Port: ");
    try
    {
        int.TryParse(Console.ReadLine(), out listenPort);
    }
    catch (InvalidCastException)
    {
        Console.WriteLine("Port should contain only numbers");
    }
    Console.Write("RecieverPort: ");
    try
    {
        int.TryParse(Console.ReadLine(), out sendPort);
    }
    catch (ArgumentException)
    {
        Console.WriteLine("Port should contain only numbers\n");
    }

    initPath();

    Task.Run(ReceiveMessageAsync);
    await SendMessageAsync();

    Console.WriteLine("If you want to exit press ESC");
    key = Console.ReadKey();
    SaveHistory();
}


async Task SendMessageAsync()
{
    using Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    ConsoleKeyInfo key = new ConsoleKeyInfo();

    while (key.Key != ConsoleKey.Escape)
    {
       
        string? message = Console.ReadLine();
        if (string.IsNullOrEmpty(message)) break;

        message = $"{username}: {message}";

        text += message + "\n";

        byte[] data = Encoding.UTF8.GetBytes(message);
        await sender.SendToAsync(data,0, new IPEndPoint(recieverAddress, sendPort));

        key = Console.ReadKey(true);
    }
     
}


async Task ReceiveMessageAsync()
{

    byte[] data = new byte[65535];
    using Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    receiver.Bind(new IPEndPoint(recieverAddress, listenPort));
    while (true)
    {

        var result = await receiver.ReceiveFromAsync(data, 0, new IPEndPoint(IPAddress.Any, 0));
        var message = Encoding.UTF8.GetString(data, 0, result.ReceivedBytes);

        text += message + "\n";

        Print(message);
    }
}

void Print(string message)
{
    
    var position = Console.GetCursorPosition();
    int left = position.Left;
    int top = position.Top;
    Console.SetCursorPosition(0, top);
    Console.WriteLine(message);
    Console.SetCursorPosition(left, top + 1);

}
