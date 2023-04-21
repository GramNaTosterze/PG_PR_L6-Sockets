using PG_PR_L6_Sockets;
using System;
using System.Data;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

class IPC
{
    const string pipeName = "PG_PR_L6-Sockets";
    const string resName = "Sock-RES";
    static IPHostEntry host = Dns.GetHostEntry("localhost");
    static IPAddress ipAddress = host.AddressList[0];
    static IPEndPoint localEP = new IPEndPoint(ipAddress, 11000);
    static IPEndPoint responseEP = new IPEndPoint(ipAddress, 11100);
    static void Main(string[] args)
    {
        if (args.Length == 0)
            Server();
        else
            Client(args);

    }
    static async void Server()
    {
        try {

            // Create a Socket that will use Tcp protocol
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket responseListener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // A Socket must be associated with an endpoint using the Bind method
            listener.Bind(localEP);
            responseListener.Bind(responseEP);

            listener.Listen(10);
            responseListener.Listen(10);

            Console.WriteLine("Waiting for a connection...");
            

            string clientPath = @"PG_PR_L6-Sockets.exe";

            var startInfo = new ProcessStartInfo(clientPath, pipeName + " " + resName);
            startInfo.UseShellExecute = true;
            Process childProcess = Process.Start(startInfo);

            Socket handler = listener.Accept();
            Socket responseHandler = responseListener.Accept(); 

            new UniqueList(handler, responseHandler);
            
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\n Press any key to continue...");
        Console.ReadKey();
    }
    public static void Client(string[] args)
    {
        byte[] bytes = new byte[1024];

        try
        {

            // Create a TCP/IP  socket.
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            Socket responce = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);


            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                // Connect to Remote EndPoint
                sender.Connect(localEP);
                responce.Connect(responseEP);

                new UniqueList(sender, responce);

                // Release the socket.
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.Read();
    }
}