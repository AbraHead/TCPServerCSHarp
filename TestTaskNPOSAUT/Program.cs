using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Security.Cryptography;

internal class Program
{

    private static async Task Main(string[] args)
    {
        await TCPwithTCPListener();
    }

    public static async Task TCPwithTCPListener()
    {
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        /*IPAddress localAddr = IPAddress.Parse("127.0.0.1");*/
        TcpListener server = new TcpListener(localAddr, 8888);
        server.Start(1000);
        while (true)
        {
            var client = await server.AcceptTcpClientAsync();
            Console.WriteLine($"Адрес подключенного клиента: {client.Client.RemoteEndPoint}");
            //Получаем сетевой поток от клиента
            NetworkStream stream = client.GetStream();
            //Размер буфера
            int bufferSize = 512;
            //Определяем буфер
            byte[] responseData = new byte[bufferSize];
            //Строковая переменная для данных
            StringBuilder response = new StringBuilder();
            int bytes;
            do
            {
                bytes = await stream.ReadAsync(responseData);
                response.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
            } while (bytes > bufferSize);
            Console.WriteLine(response);

            try {
                var jsonData = JsonSerializer.Deserialize<List<Dictionary<string, Dictionary<string, string>>>>(response.ToString());
                foreach (var elem in jsonData)
                {
                    if (elem.ContainsKey("data"))
                    {
                        elem["data"]["processed"] = "true";
                    }
                }
                var json = JsonSerializer.Serialize(jsonData);
                var data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data);
            } catch (JsonException jex) {
                await stream.WriteAsync(Encoding.UTF8.GetBytes(jex.Message));
            } 
            
            client.Close();
            stream.Close();
        }
    }

    public static async Task TCPSockets() 
    {
        /*Console.WriteLine("Hello, World!");*/
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(ipPoint);   // связываем с локальной точкой ipPoint
        socket.Listen(1000);
        // получаем входящее подключение
        Socket client = await socket.AcceptAsync();
        // получаем адрес клиента
        Console.WriteLine($"Адрес подключенного клиента: {client.RemoteEndPoint}");
        NetworkStream stream = new NetworkStream(client);
        int buffersize = 512;
        byte[] responseData = new byte[buffersize];
        /*var bytes = await stream.ReadAsync(responseData)*/
        var response = new StringBuilder();
        int bytes = 1;
        /*Encoding.UTF8.GetString(responseData, 0, bytes);*/
        do
        {
            bytes = await stream.ReadAsync(responseData);
            Console.WriteLine(bytes);
            response.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
            /*Console.WriteLine(bytes);*/
        } while (bytes > buffersize);
        Console.WriteLine(response);


        var jsonData = JsonSerializer.Deserialize<List<Dictionary<string, Dictionary<string, string>>>>(response.ToString());
        foreach (var elem in jsonData)
        {
            if (elem.ContainsKey("data"))
            {
                elem["data"]["processed"] = "true";
            }
        }
        var json = JsonSerializer.Serialize(jsonData);
        var data = Encoding.UTF8.GetBytes(json);
        await stream.WriteAsync(data);

        stream.Close();
        client.Close();

        
    }


}