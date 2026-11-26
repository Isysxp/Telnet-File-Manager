
using Microsoft.VisualBasic;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

// This module is based upon Jon Sagara's rebuild of Tom Janssen's Minimalistic Telnet. See: https://github.com/jonsagara/MinimalisticTelnet
// Site: https://github.com/jonsagara



namespace VT105.MinimalisticTelnet
{
    internal enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    internal enum Options
    {
        SGA = 3
    }

    public class TelnetConnection
    {
        private TcpClient tcpSocket;

        public void NoKeep(TcpClient tcl)
        {
            const uint on = 0;
            const uint time = 2000;
            const uint interval = 2000;

            byte[] inOptionValues = new byte[Marshal.SizeOf(on) * 3];

            BitConverter.GetBytes(on).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes(time).CopyTo(inOptionValues, Marshal.SizeOf(on));
            BitConverter.GetBytes(interval).CopyTo(inOptionValues, Marshal.SizeOf(on) * 2);

            tcl.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
        public TelnetConnection(string Hostname, int Port)
        {
            tcpSocket = new TcpClient();
            NoKeep(tcpSocket);

            IAsyncResult result = tcpSocket.BeginConnect(Hostname, Port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(4000); // 1000 ms timeout
            if (success)
            {
                tcpSocket.EndConnect(result);
            }
            else
            {
                tcpSocket.Close();
                MessageBox.Show("Telnet connection failed....");
                System.Windows.Forms.Application.Exit();
            }

        }

        public string Login(string Username, string Password, String Prompt)
        {
            string s="";
            do
            {
                s = s + Read();
            } while (!s.EndsWith("login: "));
            WriteLine(Username);
            s = "";
            do
            {
                s = s + Read();
                if (s.EndsWith(Prompt))
                {
                    WriteLine("");
                    return s;
                }
            } while (!s.EndsWith("Password:"));
            WriteLine(Password);
            s += Read();
            return s;
        }

        public void WriteLine(string cmd)
        {
            Write(cmd + Constants.vbCrLf);
        }

        public void Write(string cmd)
        {
            if (!tcpSocket.Connected)
                return;
            var buf = Encoding.ASCII.GetBytes(cmd.Replace(Constants.vbNullChar + "xFF", Constants.vbNullChar + "xFF" + Constants.vbNullChar + "xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        public void WriteChar(char Chr)
        {
            if (!tcpSocket.Connected)
                return;
            var buf = Encoding.UTF8.GetBytes(Chr.ToString());
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        public string Read()
        {
            if (!tcpSocket.Connected)
                return null;
            var sb = new StringBuilder();

            do
            {
                ParseTelnet(sb);
                Application.DoEvents();
            }
            while (tcpSocket.Available > 0);

            return sb.ToString();
        }

        public void Disconnect()
        {

            if (!tcpSocket.Connected)
                return;
            tcpSocket.GetStream().WriteByte((byte)4);
            tcpSocket.GetStream().Close();
        }
        public bool IsConnected
        {
            get
            {
                return tcpSocket.GetStream().CanWrite;
            }
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA)
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }
    }
}