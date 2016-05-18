using SharpTools.TCPSocketClient;
using SharpTools.TCPSocketServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WFASpyn
{
    public partial class frmMain : Form
    {
        //private EventLog elgSpynSvr = new EventLog("SPYNSLog");

        public frmMain()
        {
            InitializeComponent();

          /*  if (!EventLog.SourceExists("SPYNServer"))
            {
                EventLog.CreateEventSource("SPYNServer", "SPYNSLog");
            }
            elgSpynSvr.Source = "SPYNServer";
           * */
        }

      
        private void btnPoll_Click(object sender, EventArgs e)
        {
            //string serverIP = "localhost";
            //string message = "Hello";
            //Connect(serverIP, message);
            //EventLog.WriteEntry(elgSpynSvr.Log, "B1 Biches", EventLogEntryType.Information);
            SharpToolsTCPSocketClient.StartClient("POLL", "10.0.0.1", SharpToolsTCPSocketServer.STSS_PORT);
        }


   

        private void button3_Click(object sender, EventArgs e)
        {
            /*if (EventLog.SourceExists("SPYNServer"))
            {
                EventLog.DeleteEventSource("SPYNServer");
            }
             * */
            SharpToolsTCPSocketServer.StartServer(TCP_SOCK_APP.SPYNSERV, 13);
        }

        private void btnTestRequest_Click(object sender, EventArgs e)
        {
            SharpToolsTCPSocketClient.StartClient(txtTestRequest.Text, "DPPCS", SharpToolsTCPSocketServer.STSS_PORT);
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            // TODO Check paths are valid
            MessageBox.Show("Syncing " + txtMaster.Text + " with " + txtSatellite.Text);
        }

        private void txtMaster_Click(object sender, EventArgs e)
        {
            // HACK MessageBox.Show("Master Root");
            fbdRoot.ShowDialog();
            // TODO Error checking
            txtMaster.Text = fbdRoot.SelectedPath;
        }

        private void txtDest_Click(object sender, EventArgs e)
        {
            //HACK MessageBox.Show("Satellite Root");
            fbdRoot.ShowDialog();
            // TODO Error checking
            txtSatellite.Text = fbdRoot.SelectedPath;
        }

    }
}
