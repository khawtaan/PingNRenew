using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMonitor
{
    class Program
    {
        //Global declare value
        static string nameOrIP = "10.37.0.1"; //TRUE WIFI DHCP IP
                                              //string nameOrIP = "192.168.66.50";

        //Count column for line
        static int columnNum = 0;
        static int cursorPosition = 0;
        static int[] currentPosition = new int[] { 0, 0 };
        static long replyTime = 0;

        //Count Failed time
        static int countF = 0;

        static void Main(string[] args)
        {
            //TO-DO
            /*
            - Ping command, interval with 1 Bite, show on console monitor
            - Use exception capture invalid IP address
            - SSH execute command to renew and follow up result 3 sec
            - SSH execute command to reconnect and follow up result 10 sec
            */

            //Ping command, interval with 1 Bite
            int timeOut = 1000; //Time out 1 sec
            Byte[] test = { 0 }; //Minimum Byte

            //Ping interval infinity loop
            Ping pingExec = new Ping();

            headerPrint();

            System.Console.Title = "Console Monitor, Ping command~ VERSION BETA";

            //Set console column size
            Console.SetWindowSize(68, Console.LargestWindowHeight - 30);
            Console.BufferWidth = 68;
            Console.BufferHeight = Console.LargestWindowHeight - 30;

            //Declare PingReply
            PingReply reply;

            while (true)
            {
                try
                {
                    reply = pingExec.Send(nameOrIP, timeOut, test);
                    //System.Console.WriteLine("Ping : " + nameOrIP + " , Result : " + reply.Status + " , Time : " + reply.RoundtripTime + " ms");
                    //System.Threading.Thread.Sleep(1000);
                    //System.Console.WriteLine(IPStatus.Success);
                    replyTime = reply.RoundtripTime;

                    if (columnNum == 0) { System.Console.Write("-[ "); }

                    //O is Green and success with time-out lower than 500ms
                    //- is Yellow and success with time-out between 500ms and 1000ms
                    //X is Red and success with time-out higher than 1000ms or failure ping
                    if (reply.Status == IPStatus.Success)
                    {
                        printColorWTimeout(reply.RoundtripTime);
                    }
                    else
                    {
                        printColorWTimeout(1001);
                    }

                    //Update status
                    //currentPosition = { Console.CursorLeft , Console.CursorTop };
                    currentPosition[0] = Console.CursorLeft;
                    currentPosition[1] = Console.CursorTop;
                    statusUpdate(cursorPosition, currentPosition, "ms    ", replyTime);

                    //Debug status
                    //System.Console.Write(reply.Status);

                }
                catch (PingException)
                {
                    //System.Console.WriteLine("False");
                    printColorWTimeout(1001);

                    //Debug status
                    //System.Console.Write(ex);
                }

                //Console with color, 60 columns in 1 line.
                if (columnNum >= 60)
                {
                    System.Console.WriteLine(" ]-");
                    columnNum = 0;

                    //End of console
                    if (Console.CursorTop == Console.BufferHeight - 1)
                    {
                        //currentPosition = { Console.CursorLeft , Console.CursorTop };
                        currentPosition[0] = Console.CursorLeft;
                        currentPosition[1] = Console.CursorTop;

                        Console.SetCursorPosition(0, 0);
                        headerPrint();
                        Console.SetCursorPosition(currentPosition[0], currentPosition[1]);
                    }
                }
                else
                {
                    columnNum++;
                }
                
                System.Threading.Thread.Sleep(1500); //Pause 1.5 sec

            }
        }

        private static void printColorWTimeout (long timeOut)
        {
            if (timeOut <= 500)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Write("O");
                countFailed(false);
            }
            else if (timeOut >=501 && timeOut <= 1000)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write("-");
                countFailed(false);
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Write("X");
                //System.Console.Beep();
                countFailed(true);
            }
            Console.ResetColor();
        }

        //Status update (ms)
        private static void statusUpdate (int position, int[] currentPs, string word, long ms)
        {
            Console.SetCursorPosition(position, 0);
            System.Console.Write(ms + word);
            Console.SetCursorPosition(currentPs[0], currentPs[1]);
        }

        private static void headerPrint()
        {
            //Header title of console
            System.Console.Write("Ping IP : ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write(nameOrIP);
            Console.ResetColor();

            //Status update
            System.Console.Write(" , Status : ");
            cursorPosition = Console.CursorLeft;
            System.Console.WriteLine("                                     ");
        }

        private static void countFailed(Boolean count)
        {
            if (countF >= 10)
            {
                System.Console.Beep();

                //Run ssh command to renew DHCP client
                SshClient ssh = new SshClient( "192.168.1.1", "ubnt", "9thekings" );
                //#TODO Exception be for there.
                ssh.Connect();
                if (ssh.IsConnected)
                {
                    string resultPid = ssh.RunCommand("pidof udhcpc").Result;
                    if (resultPid != null)
                    {
                        resultPid = resultPid.Substring(0, resultPid.Length - 1);
                        ssh.RunCommand(string.Format("kill -USR1 {0}", resultPid));
                    }
                    ssh.Disconnect();
                }
                System.Threading.Thread.Sleep(2000);
                countF = 0;
            }
            else
            {
                if(count)
                {
                    countF++;
                }
                else
                {
                    countF = 0;
                }
            }

            
        }
    }
}
