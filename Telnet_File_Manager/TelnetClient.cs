using System;
using System.Collections.Generic;
using System.Text;

namespace Telnet_File_Manager
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Windows.Forms;

    public class TelnetClient
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private StreamReader reader;
        private StreamWriter writer;

        // Telnet protocol constants
        private const byte IAC = 255;   // Interpret As Command
        private const byte DO = 253;
        private const byte DONT = 254;
        private const byte WILL = 251;
        private const byte WONT = 252;

        public bool Connect(string host, int port)
        {
            try
            {
                tcpClient = new TcpClient(host, port);
                networkStream = tcpClient.GetStream();
                reader = new StreamReader(networkStream, Encoding.UTF8);
                writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                return false;
            }
        }

        public void SendCommand(string command)
        {
            if (writer != null)
            {
                writer.Write(command + "\r\n");
                writer.Flush();
            }
        }

        public void SendChar(String command)
        {
            if (writer != null)
            {
                byte[] bfr = Encoding.ASCII.GetBytes(command);
                //writer.Write(command);
                tcpClient.Client.Send(bfr);
            }
        }

        public void SendNULL()
        {
            byte[] EOF = { 0 };
            if (writer != null)
            {
                writer.Write((char)8);
            }
        }

        public void SendEOF()
        {
            byte[] EOF = { 4 };
            if (writer != null)
            {
                Disconnect();
            }
        }
        public string ReadResponse()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                while (networkStream.DataAvailable)
                {
                    int b = networkStream.ReadByte();
                    if (b == -1) break;

                    if (b == 0xef)
                    {
                        networkStream.ReadByte();
                        networkStream.ReadByte();
                    }
                    else 
                    if (b == IAC) // Telnet command
                    {
                        int cmd = networkStream.ReadByte();
                        int opt = networkStream.ReadByte();

                        // Respond to negotiation
                        if (cmd == DO)
                        {
                            // Politely refuse all DO requests, allow SGA (3)
                            if (opt == 3)
                                {
                                    networkStream.Write(new byte[] { IAC, WILL, (byte)opt }, 0, 3);
                                } else
                                {
                                    networkStream.Write(new byte[] { IAC, WONT, (byte)opt }, 0, 3);
                                }
                        }
                        else if (cmd == WILL)
                        {
                            // Politely refuse all WILL requests
                            networkStream.Write(new byte[] { IAC, DONT, (byte)opt }, 0, 3);
                        }
                        // Ignore other commands
                    }
                    else
                    {
                        sb.Append((char)b);
                    }
                }
            }
            catch (Exception ex)
            {
                sb.Append($"[Read error: {ex.Message}]");
            }
            return sb.ToString();
        }

        public void Disconnect()
        {
            try
            {
                reader?.Close();
                writer?.Close();
                networkStream?.Close();
                tcpClient?.Close();
            }
            catch { /* Ignore cleanup errors */ }
        }
/*
        static void Main()
        {
            TelnetClient client = new TelnetClient();

            Console.Write("Enter Telnet host: ");
            string host = Console.ReadLine();

            Console.Write("Enter Telnet port (default 23): ");
            if (!int.TryParse(Console.ReadLine(), out int port))
                port = 23;

            if (!client.Connect(host, port))
            {
                Console.WriteLine("Failed to connect.");
                return;
            }

            Console.WriteLine("Connected. Type commands (type 'exit' to quit).");

            while (true)
            {
                string response = client.ReadResponse();
                if (!string.IsNullOrEmpty(response))
                    Console.Write(response);

                Console.Write("> ");
                string cmd = Console.ReadLine();
                if (cmd.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                client.SendCommand(cmd);
                System.Threading.Thread.Sleep(200);
                Console.Write(client.ReadResponse());
            }

            client.Disconnect();
            Console.WriteLine("Disconnected.");
        }
*/
    }
}
