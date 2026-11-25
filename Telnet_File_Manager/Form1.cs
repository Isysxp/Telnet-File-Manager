using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using VT105.MinimalisticTelnet;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;

namespace Telnet_File_Manager
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32")]
        static extern int SetConsoleMode(IntPtr hConsoleHandle, long dwMode);
        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out long lpMode);
        // Import AllocConsole from kernel32.dll  
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        // Import FreeConsole from kernel32.dll (optional, to close the console)  
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();
        // Import GetConsoleWindow to check if console exists  
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        private delegate bool ConsoleEventDelegate(int eventType);

        // Import SetConsoleCtrlHandler from kernel32.dll
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate handler, bool add);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int ENABLE_PROCESSED_OUTPUT = 0x1;
        private const int ENABLE_WRAP_AT_EOL_OUTPUT = 0x2;
        private const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x4;
        private const int ENABLE_VIRTUAL_TERMINAL_INPUT = 0x200;
        private const int ENABLE_ECHO_INPUT = 0x0004;
        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        // Event type constants
        private const int CTRL_C_EVENT = 0;
        private const int CTRL_BREAK_EVENT = 1;
        private const int CTRL_CLOSE_EVENT = 2;
        private const int CTRL_LOGOFF_EVENT = 5;
        private const int CTRL_SHUTDOWN_EVENT = 6;

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        // Menu constants
        private const uint SC_CLOSE = 0xF060;
        private const uint MF_BYCOMMAND = 0x00000000;
        public bool Cncct = false;
        public String Prompt;
        public Form CForm;
        public TelnetConnection Client;
        private ContextMenuStrip contextMenu;
        public Form1()
        {
            InitializeComponent();
            // Create ContextMenuStrip
            contextMenu = new ContextMenuStrip();
            var menuView = new ToolStripMenuItem("View");
            var menuDelete = new ToolStripMenuItem("Delete");
            var menuNew = new ToolStripMenuItem("New");
            var menuCopy = new ToolStripMenuItem("Copy");
            // Attach event handlers
            menuView.Click += MenuView_Click;
            menuDelete.Click += MenuDelete_Click;
            menuNew.Click += MenuNew_Click;
            menuCopy.Click += MenuCopy_Click;
            contextMenu.Items.AddRange(new ToolStripItem[] { menuView, menuDelete, menuNew, menuCopy });
            listBox1.ContextMenuStrip = contextMenu;
            Prompt = "$ ";
        }

        private void MenuView_Click(object sender, EventArgs e)
        {

            if (listBox1.SelectedIndex != -1)
            {
                String fnames = listBox1.SelectedItem.ToString();
                String[] fields = fnames.Split(' ');
                String fname = fields[fields.Length - 1];
                fname = fname.Replace("\r", "");
                String line,cmd = "cat " + fname;
                StringBuilder sb = new StringBuilder();

                Client.WriteLine(cmd);
                Cursor.Current = Cursors.WaitCursor;
                System.Windows.Forms.Application.DoEvents();
                do
                {
                    line = Client.Read();
                    sb.Append(line);
                } while (!line.EndsWith(Prompt));
                sb.Replace(cmd, "");
                sb.RemoveLastLine();
                File.WriteAllText(fname, sb.ToString());

                Process notePad = new Process();
                notePad.StartInfo.FileName = "notepad.exe";
                notePad.StartInfo.Arguments = fname;
                notePad.Start();
                Cursor.Current = Cursors.Default;
                using (notePad)
                {
                    notePad.WaitForExit();
                    // Retrieve the app's exit code
                }
            }
        }

        private void MenuDelete_Click(object sender, EventArgs e)
        {
            String line;
            // Show confirmation dialog
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this item?",
                "Confirm Action",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (result == DialogResult.No)
            {
                return;
            }

                if (listBox1.SelectedIndex != -1)
            {
                String fnames = listBox1.SelectedItem.ToString();
                String[] fields = fnames.Split(' ');
                String fname = fields[fields.Length - 1];
                Client.WriteLine("rm ./" + fname);
                do
                {
                    line = Client.Read();

                } while (!line.EndsWith(Prompt));
                directoryToolStripMenuItem_Click(null, null);
            }
        }

        private void MenuCopy_Click(object sender, EventArgs e)
        {
            String line="";

            string input = Interaction.InputBox("Destination file name", "Copy file", "", this.Bounds.Right, this.Bounds.Top);

            if (input == "")
            {
                return;
            }

            if (listBox1.SelectedIndex != -1)
            {
                String fnames = listBox1.SelectedItem.ToString();
                String[] fields = fnames.Split(' ');
                String fname = fields[fields.Length - 1];
                fname = fname.Replace("\r", "");
      
                Client.WriteLine("cp " + fname + " " + input);
                do
                {
                    line = line + Client.Read();

                } while (!line.EndsWith(Prompt));
                directoryToolStripMenuItem_Click(null, null);
            }
        }

        private void MenuNew_Click(object sender, EventArgs e)
        {
            String line;

            string input = Interaction.InputBox("New file name", "New file", "", this.Bounds.Right, this.Bounds.Top);

            if (input == "")
            {
                return;
            }

            if (listBox1.SelectedIndex != -1)
            {
                Client.WriteLine("touch " + input);
                do
                {
                    line = Client.Read();

                } while (!line.EndsWith(Prompt));
                directoryToolStripMenuItem_Click(null, null);
            }
        }

        private void connectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Cncct)
            {
                MessageBox.Show("Already connected......");
                return;
            }
            Cncct = false;
            CForm = new Connect(this,Client);
            CForm.Show();
        }

        public void directoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            String line;
            String[] dtr;

            if (!Cncct)
            {
                MessageBox.Show("Please connect first....");
                return;

            }
            if (GetConsoleWindow() != IntPtr.Zero)
            {
                ShowWindow(GetConsoleWindow(), SW_HIDE);
            }

            listBox1.Items.Clear();
            Client.WriteLine(" ls -l");
            line = "";
            do
            {
                line = Client.Read();
                sb.Append(line);
            } while (!sb.ToString().EndsWith(Prompt));
            sb.Replace("cd ..", "");
            sb.Replace("\r", "");
            //sb.RemoveLastLine();
            sb.RemoveFirstLine();
            dtr = sb.ToString().Split('\n');
            listBox1.Items.Add("..");
            listBox1.Items.AddRange(dtr);
            listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Cncct)
            {
                MessageBox.Show("Please connect first....");
                return;

            }
            CheckConsole();
            Client.WriteLine("");
            timer1.Enabled = true;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            if (!Cncct)
            {
                MessageBox.Show("Please connect first....");
                return;

            }

            StringBuilder sb = new StringBuilder();
            int exitCode;
            String fnames = listBox1.SelectedItem.ToString();
            String[] fields = fnames.Split(' ');
            String fname = fields[fields.Length - 1];
            fname = fname.Replace("\r", "");
            String line="", cmd = "cat " + fname;
            byte[] EOF = { 4 };

            if (fname == "..")
            {
                Client.WriteLine("cd ..");
                do
                {
                    line = line + Client.Read();

                } while (!line.EndsWith(Prompt));
                directoryToolStripMenuItem_Click(null, null);
                return;
            }

            if (fnames.IndexOf("drwx") == 0)
            {
                Client.WriteLine("cd " + fname);
                do
                {
                    line = Client.Read();

                } while (!line.EndsWith(Prompt));
                directoryToolStripMenuItem_Click(null, null);
                return;
            }

            if (fnames.IndexOf("rwxr")>0)
            {
                cmd = "./" + fname;
                CheckConsole();
                timer1.Enabled = true;
                Client.WriteLine(cmd);
                return;
            }
            if (fname.IndexOf("-rw") == -1)
            {
                return;
            }

            Client.WriteLine(cmd);
            do
            {
                line = Client.Read();
                sb.Append(line);
            } while (!line.EndsWith(Prompt));
            sb.Replace(cmd, "");
            sb.Replace(Prompt, "");
            File.WriteAllText(fname, sb.ToString());

            Process notePad = new Process();
            notePad.StartInfo.FileName = "notepad.exe";
            notePad.StartInfo.Arguments = fname;
            notePad.Start();

            using (notePad)
            {
                notePad.WaitForExit();
                // Retrieve the app's exit code
                exitCode = notePad.ExitCode;
                Cursor.Current = Cursors.WaitCursor;
                line = "";
                Client.WriteLine("stty -echo");
                do
                {
                    line = line + Client.Read();
                } while (!line.EndsWith(Prompt));
                line = "cat > " + fname;
                Client.WriteLine(line);
                Thread.Sleep(100);
                string fileContent = File.ReadAllText(fname);
                StringReader reader = new StringReader(fileContent);
                reader.ReadLine();
                while ((line=reader.ReadLine()) != null)
                {
                   Thread.Sleep(100);
                   Client.WriteLine(line);
                }
                reader.Close();
                Thread.Sleep(100);
                line = Encoding.UTF8.GetString(EOF);
                Client.Write(line);
                line = "";
                Client.WriteLine("stty echo");
                do
                {
                    line = line + Client.Read();
                } while (!line.EndsWith(Prompt));
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Write completed....");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            String line;
            ConsoleKeyInfo InKey;

            if (GetConsoleWindow()==IntPtr.Zero)
            {
                timer1.Enabled = false;
                return;
            }
            if (Console.KeyAvailable)
            {
                InKey = Console.ReadKey(true);
                line = InKey.KeyChar.ToString();
                if (InKey.KeyChar == 13)
                {
                    line = line + "\n";
                }
                if (InKey.KeyChar != 4)
                {
                    Client.Write(line);
                } else
                {
                    MessageBox.Show("Use 'Exit' to disconnect");
                }
                return;
            }
            line = Client.Read();
            if (!string.IsNullOrEmpty(line))
            {
                Console.Write(line);
            }
        }

        // Handler method
        private static bool ConsoleEventHandler(int eventType)
        {
            switch (eventType)
            {
                case CTRL_C_EVENT:
                case CTRL_BREAK_EVENT:
                    Console.WriteLine("Ctrl+C or Ctrl+Break detected — ignoring.");
                    return true; // Prevent termination

                case CTRL_CLOSE_EVENT:
                    Console.WriteLine("Close button clicked — delaying shutdown...");
                    // Delay for demonstration
                    System.Threading.Thread.Sleep(3000);
                    return true; // Prevent immediate termination

                case CTRL_LOGOFF_EVENT:
                case CTRL_SHUTDOWN_EVENT:
                    Console.WriteLine("System logoff/shutdown detected — cleaning up...");
                    return false; // Allow termination after cleanup
            }
            return false;
        }
        private void CheckConsole()
        {
            if (GetConsoleWindow() == IntPtr.Zero)
            {
                AllocConsole();
                SetConsoleCtrlHandler(ConsoleEventHandler, true);
                IntPtr consoleWindow = GetConsoleWindow();
                IntPtr systemMenu = GetSystemMenu(consoleWindow, false);
                DeleteMenu(systemMenu, SC_CLOSE, MF_BYCOMMAND);
                Console.TreatControlCAsInput = true;
                System.IntPtr Chndl = GetStdHandle(STD_OUTPUT_HANDLE);
                System.IntPtr Mhndl = GetStdHandle(STD_INPUT_HANDLE);
                SetConsoleMode(Chndl, ENABLE_PROCESSED_OUTPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
                SetConsoleMode(Mhndl, ENABLE_VIRTUAL_TERMINAL_INPUT);
            }
            else
            {
                ShowWindow(GetConsoleWindow(), SW_SHOW);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(Client == null))
            {
                Client.Disconnect();
            }
            System.Windows.Forms.Application.Exit();
        }
    }
}

