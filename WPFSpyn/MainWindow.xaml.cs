using SharpTools.Log;
using SharpTools.System;
using SharpTools.TCPSocketClient;
using SharpTools.TCPSocketServer;
using SharpTools.Utility.UserInput;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WPFSpyn.ViewModel;

namespace WPFSpyn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog _log = SharpToolsLog.GetLogger();

        public MainWindow()
        {
            InitializeComponent();
            // LOG
            _log.Debug("init complete");
            //
            Closed += new EventHandler(OnExit);

            // Display name of computer service is executing on
            SetSpynServer(Properties.Settings.Default.strSpynServ.ToUpper());
            // LOG
            _log.Info("Checking for service on [" + Properties.Settings.Default.strSpynServ.ToUpper() + "]");
            //
            // Create the ViewModel to which 
            // the main window binds.
            var viewModel = new MainWindowViewModel(MainWindowViewModel.DATA_PATH);
            // LOG
            _log.Debug("View Model created from");
            _log.Debug("[" + MainWindowViewModel.DATA_PATH  + "]");
            //
            // When the ViewModel asks to be closed, 
            // close the window.
            EventHandler handler = null;
            handler = delegate
            {
                viewModel.RequestClose -= handler;
                Close();
            };
            viewModel.RequestClose += handler;


            // Allow all controls in the window to 
            // bind to the ViewModel by setting the 
            // DataContext, which propagates down 
            // the element tree.
            DataContext = viewModel;
            // LOG
            _log.Debug("Data Context set");
            //
        }


        private void btnPoll_Click(object sender, RoutedEventArgs e)
        {
            SetSpynServer(Properties.Settings.Default.strSpynServ.ToUpper());
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            // LOG
            _log.Info("Close program...");
            //
            Close();
        }


        /// <summary>
        /// Opens window to collect new name for server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSpynServ_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // LOG
            _log.Debug("Edit SpynServer");
            //
            UserInput suu = new UserInput("Enter Server Name:", txtSpynServ.Text);
             suu.ShowDialog();
            bool booResponse = suu.booResponse;
            if (booResponse ==  true)
                SetSpynServer(suu.Answer.ToUpper());
            // LOG
            _log.Info(string.Format ("Set SpynServer to [{0}]", suu.Answer.ToUpper()));
            //
            txtSpynServ.Select(0, 0);

        }


        private void OnExit(object sender, EventArgs e)
        {
            // LOG
            _log.Debug(string.Format("Executing OnExit event...{0}" , sender.ToString()));
            _log.Info("Saving settings...");
            //
            Properties.Settings.Default.Save();
            // LOG
            _log.Info("Settings saved successfully.");
            //
        }


        /// <summary>
        /// Queries server to see if it is online
        /// </summary>
        /// <returns>A string response if online</returns>
        private string PollServer()
        {
            // LOG
            _log.Info(string.Format("Polling [{0}] on port [{1}]", Properties.Settings.Default.strSpynServ, SharpToolsTCPSocketServer.STSS_PORT));
            //
            return SharpToolsTCPSocketClient.StartClient("POLL", Properties.Settings.Default.strSpynServ, SharpToolsTCPSocketServer.STSS_PORT);
        }


        /// <summary>
        /// Updates stored server name, and then checks if it exists.
        /// Changes color of text to reflect online status.
        /// </summary>
        /// <param name="p_strSpynServ">New name for server</param>
        private void SetSpynServer(string p_strSpynServ)
        {
            Properties.Settings.Default.strSpynServ = p_strSpynServ;
            string strServerOnline = PollServer();
            if (strServerOnline == "ONLINE")
                txtSpynServ.Foreground = Brushes.LimeGreen;
            else
                txtSpynServ.Foreground = Brushes.Red;
        }

    }
}
