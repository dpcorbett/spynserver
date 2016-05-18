using SharpTools.TCPSocketServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Sockets;
using System.Net;
using System.Security.AccessControl;
using System.Threading;


namespace WSVSpyn
{
    public partial class SvcSpynServer : ServiceBase
    {

        private ClassLibrarySharpTools.SharpTools stlSharpTools = new ClassLibrarySharpTools.SharpTools();
        //private ClassLibrarySharpTools.LogFile stlLogFile;
        private static System.Timers.Timer timTimer;
        private EventLog elgSpynSvr = new EventLog("SPYNSLog");
        //IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
        //static string output = "";

        public SvcSpynServer()
        {
            InitializeComponent();
            // Create event log
            this.CreateEventLog();


           /*             try
                        {
                            this.stlLogFile = new LogFile(new FileStream("./Output.txt", FileMode.Create, FileAccess.Write));
                            // Get a FileSecurity object that represents the
                            // current security settings.
                            FileSecurity fSecurity = File.GetAccessControl("Output.txt");
                            // Add the FileSystemAccessRule to the security settings.
                            fSecurity.AddAccessRule(new FileSystemAccessRule(@"DPCCS\LocalSystem", FileSystemRights.FullControl, AccessControlType.Allow));
                            // Set the new access settings.
                            File.SetAccessControl("Output.txt", fSecurity);

                            this.stlLogFile.stwLogOutput = new StreamWriter(stlLogFile.fstLogFile);
                            Console.SetOut(stlLogFile.stwLogOutput);
                            this.stlLogFile.stwLogOutput.WriteLine(this.stlLogFile.logWriteLine("Log Created"));
                            this.stlLogFile.stwLogOutput.Flush();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Cannot open Output.txt for writing");
                            Console.WriteLine(e.Message);
                            return;
                        }
            */

            this.ReadConfig();
            this.StartTimer();
        }


        private void CreateEventLog()
        {
            elgSpynSvr = new System.Diagnostics.EventLog();

            if (!EventLog.SourceExists("SPYNServer"))
            {
                EventLog.CreateEventSource("SPYNServer", "SPYNSLog");
            }
            elgSpynSvr.Source = "SPYNServer";
        }

        private void StartTimer()
        {
            timTimer = new System.Timers.Timer(10000);
            timTimer.Elapsed += new ElapsedEventHandler(OnTimer);
            timTimer.Enabled = true;
            timTimer.Start();
        }


        protected override void OnStart(string[] args)
        {
            // HACK
            EventLog.WriteEntry(elgSpynSvr.Log, "Command Service Start successfully called", EventLogEntryType.Information);
            //this.stlLogFile.stwLogOutput.WriteLine(this.stlLogFile.logWriteLine("FucK"));
        }


        protected override void OnStop()
        {
            // HACK
            EventLog.WriteEntry(elgSpynSvr.Log, "Command Service Stop successfully called", EventLogEntryType.Information);
        }


        private void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
             SharpToolsTCPSocketServer.StartServer(TCP_SOCK_APP.SPYNSERV, 13);
        }


        private void ReadConfig()
        {
            // HACK
            EventLog.WriteEntry(elgSpynSvr.Log, "Reading Config", EventLogEntryType.Information); 
        }

    }
    
}
