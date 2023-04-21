using System;
using System.ComponentModel;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using static IPC;

namespace PG_PR_L6_Sockets
{
    internal class UniqueList
    {
        List<int> numbers = new List<int>();
        Thread read;
        Thread write;
        public enum MessageType
        {
            Number,
            Response,
            Skip
        }
        public UniqueList(Socket socket, Socket respond)
        {
            MessageType messageType = MessageType.Response;




            int LOOP = 100;

            write = new Thread(() =>
            {
                for (int i = 0; i < LOOP; i++)
                {
                    //lock (numbers)
                    {
                        int num = new Random().Next(100);
                        if (!numbers.Contains(num))
                        {
                            //send number
                            socket.Send(BitConverter.GetBytes(num));
                            //read responce
                            byte[] buffer = new byte[1024];
                            var receive = respond.Receive(buffer);
                            bool response = BitConverter.ToBoolean(buffer);

                            if (response && !numbers.Contains(num))
                            {
                                Console.WriteLine($"{num}");
                            }
                            else
                                Console.WriteLine($"cannot add(other)");

                        }
                        else
                        {
                            Console.WriteLine($"cannot add");
                        }
                    }
                }
            });

            read = new Thread(() =>
            {
                for (int i = 0; i < LOOP; i++)
                {
                    //read number
                    bool contain;
                    lock (numbers)
                    {
                        byte[] bytes = new byte[1024];
                        var responce = socket.Receive(bytes);

                        int number = BitConverter.ToInt32(bytes);
                        if (numbers.Contains(number))
                            contain = true;
                        else
                        {
                            contain = false;
                            numbers.Add(number);
                            Console.WriteLine($" {number}");
                        }
                    }
                    //send responce
                    respond.Send(BitConverter.GetBytes(contain));
                }
            });

            read.Start();
            write.Start();

            read.Join();
            write.Join();

        }

    }
}