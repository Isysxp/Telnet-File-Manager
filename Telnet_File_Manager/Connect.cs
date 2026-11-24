using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using VT105.MinimalisticTelnet;
using static System.Windows.Forms.LinkLabel;

namespace Telnet_File_Manager
{
    public partial class Connect : Form
    {
        private Form1 Frm1;

        private TelnetConnection mclient;

        public Connect(Form1 frm1,TelnetConnection lclient)
        {
            InitializeComponent();
            Frm1 = frm1;
            mclient = lclient;
        }


        private void button1_Click(object sender, EventArgs e)
        {

            String line="";
            timer1.Enabled = true;
            Cursor.Current = Cursors.WaitCursor;
            mclient = new TelnetConnection(IPAddr.Text, int.Parse(Port.Text));
            mclient.Login(User.Text, Password.Text, 100);
            do
            {
                line = line + mclient.Read();
            } while (!line.EndsWith(Frm1.Prompt));
            
            Frm1.Cncct = true;
            Frm1.Text = "File Manager - Connected";
            Cursor.Current = Cursors.Default;
            Frm1.Client = mclient;
            Frm1.directoryToolStripMenuItem_Click(null, null);
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Default;
            MessageBox.Show("Login failed.....");
            this.Close();
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
