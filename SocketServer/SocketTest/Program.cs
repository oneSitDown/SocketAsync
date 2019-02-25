using System;
using System.Text;
using System.Net.Sockets;
using System.Net;


class Program
{
    /// <summary>
    /// 缓存接受的数据的byte数组
    /// </summary>
    private static byte[] buffer = new byte[1024];

    private static int connectCount=0;
    static void Main(string[] args)
    {
        //服务器需要绑定的IP和端口号
        IPEndPoint ed = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7788);
        //创建一个新的Tcp协议的Socket对象
        Socket Server = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        //服务器绑定该端口号和ip地址
        Server.Bind(ed);
        //设置该服务器至多只能监听十个客户端
        Server.Listen(10);
        //异步接收客户端
        Server.BeginAccept(new AsyncCallback(ClienAppcet), Server);
        Console.ReadKey();
    }

    private static void ClienAppcet(IAsyncResult ar)
    {
        //每当连接进来的客户端数量增加时链接数量自增1
        connectCount++;
        //服务端对象获取
        Socket ServerSocket = ar.AsyncState as Socket; 
        if(null != ServerSocket)
        {
            //得到接受进来的socket客户端
            Socket client = ServerSocket.EndAccept(ar);

            Console.WriteLine("第" + connectCount + "连接进来了");

            //开始异步接收客户端数据
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
        }
        if (null != ServerSocket)
        {
            //通过递归来不停的接收客户端的连接
            ServerSocket.BeginAccept(new AsyncCallback(ClienAppcet), ServerSocket);
        }
       
     }

    private static void ReceiveMessage(IAsyncResult ar)
    {
        Socket client = ar.AsyncState as Socket; //客户端对象
        if (client != null)
        {
            IPEndPoint clientipe = (IPEndPoint)client.RemoteEndPoint;
            try
            {
                int length = client.EndReceive(ar);

                string message = Encoding.UTF8.GetString(buffer, 0, length);
                WriteLine(clientipe + " ：" + message, ConsoleColor.White);
                //每当服务器收到消息就会给客户端返回一个Server received data
                client.Send(Encoding.UTF8.GetBytes("Server received data"));
                //通过递归不停的接收该客户端的消息
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
            }
            catch (Exception)
            {
                //设置计数器
                connectCount--;
                //断开连接
                WriteLine(clientipe + " is disconnected，total connects " + (connectCount), ConsoleColor.Red);
            }
        }

    }
    public static void WriteLine(string str, ConsoleColor color)
    {
        Console.ForegroundColor = color;

        Console.WriteLine("[{0:MM-dd HH:mm:ss}] {1}", DateTime.Now, str);
    }

}

