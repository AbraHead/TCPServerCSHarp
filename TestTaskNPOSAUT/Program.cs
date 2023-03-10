using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            JObject? jsonDict = null;
            JArray? jsonArray = null;
            IEnumerable<JToken?> listJsonKeys;
            try
            {
                
                if (response[0] == '[')
                {
                    jsonArray = JArray.Parse(response.ToString());
                    listJsonKeys = jsonArray.SelectTokens("..data");
                } else
                {
                    jsonDict = JObject.Parse(response.ToString());
                    listJsonKeys = jsonDict.SelectTokens("..data");
                }

                foreach (var datapath in listJsonKeys)
                {
                    
                    if (datapath is not null)
                    {
                        datapath["processed"] = true;
                    };
                }

                var data = Encoding.UTF8.GetBytes(jsonDict is null ? jsonArray.ToString() : jsonDict.ToString());

                await stream.WriteAsync(data);

            }
            catch (JsonReaderException jsrex)
            {
                Console.WriteLine(jsrex);
                await stream.WriteAsync(Encoding.UTF8.GetBytes(response.ToString()));
            }
            catch (System.Text.Json.JsonException jex)
            {
                Console.WriteLine(jex.Message);
                await stream.WriteAsync(Encoding.UTF8.GetBytes(response.ToString()));
            } catch (InvalidOperationException ioe) {
                Console.WriteLine(ioe.Message);
                await stream.WriteAsync(Encoding.UTF8.GetBytes(response.ToString()));
            }
            
            client.Close();
            stream.Close();
        }
    }
    
/*    public static async Task TCPSockets() 
    {
        *//*Console.WriteLine("Hello, World!");*//*
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
        *//*var bytes = await stream.ReadAsync(responseData)*//*
        var response = new StringBuilder();
        int bytes = 1;
        *//*Encoding.UTF8.GetString(responseData, 0, bytes);*//*
        do
        {
            bytes = await stream.ReadAsync(responseData);
            Console.WriteLine(bytes);
            response.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
            *//*Console.WriteLine(bytes);*//*
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

        
    }*/

}