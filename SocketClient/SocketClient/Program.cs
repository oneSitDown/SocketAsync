using System;
using System.Net.Sockets;
using System.Text;


public class Program
{
    private static readonly byte[] Buffer = new byte[1024];

    private static void Main()
    {
        try
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //链接服务器的端口号和ip地址
            socket.Connect("127.0.0.1", 7788);

            WriteLine("Client: Connect to server success!", ConsoleColor.White);

            //开始异步接收服务器得到的消息
            socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);

            //只要客户端一直开着就能不停的往服务器发送消息
            while (true)
            {
                var message = Console.ReadLine();
                if (message != null)
                {
                    var outputBuffer = Encoding.UTF8.GetBytes(message);
                    //异步发送消息
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
            }
        }
        catch (Exception ex)
        {
            WriteLine("Client: Error " + ex.Message, ConsoleColor.Red);
        }
        finally
        {
            Console.Read();
        }
    }

    // 接收信息
    public static void ReceiveMessage(IAsyncResult ar)
    {
        try
        {
            var socket = ar.AsyncState as Socket;

            //方法参考：
            if (socket != null)
            {
                int length = socket.EndReceive(ar);
                string message = Encoding.ASCII.GetString(Buffer, 0, length);
                WriteLine(message, ConsoleColor.White);
            }

            //接收下一个消息
            if (socket != null)
            {
                socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
            }
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message, ConsoleColor.Red);
        }
    }

    public static void WriteLine(string str, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine("[{0:MM-dd HH:mm:ss}] {1}", DateTime.Now, str);
    }
}
