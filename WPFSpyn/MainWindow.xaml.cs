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

        public MainWindow()
        {
            InitializeComponent();

            this.Closed += new EventHandler(OnExit);
            
            // Display name of computer executing on
            this.txtLocalHost.Text = SharpToolsSystem.strComputerName.ToUpper();

            // Display name of computer service is executing on
            this.SetSpynServer(Properties.Settings.Default.strSpynServ.ToUpper());

            // Create the ViewModel to which 
            // the main window binds.
            var viewModel = new MainWindowViewModel(MainWindowViewModel.DATA_PATH);

            // When the ViewModel asks to be closed, 
            // close the window.
            EventHandler handler = null;
            handler = delegate
            {
                viewModel.RequestClose -= handler;
                this.Close();
            };
            viewModel.RequestClose += handler;


            // Allow all controls in the window to 
            // bind to the ViewModel by setting the 
            // DataContext, which propagates down 
            // the element tree.
            this.DataContext = viewModel;

            //this.Show();

        }


        private void btnPoll_Click(object sender, RoutedEventArgs e)
        {
            this.SetSpynServer(Properties.Settings.Default.strSpynServ.ToUpper());
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private string getSyncHost()
        {
            return Properties.Settings.Default.strSpynServ.ToUpper();

        }


        /// <summary>
        /// Opens window to collect new name for server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSpynServ_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            UserInput suu = new UserInput("Enter Server Name:", this.txtSpynServ.Text);
             suu.ShowDialog();
            bool booResponse = suu.booResponse;
            if (booResponse ==  true)
                this.SetSpynServer(suu.Answer.ToUpper());
            this.txtSpynServ.Select(0, 0);

        }


        private void OnExit(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }


        /// <summary>
        /// Queries server to see if it is online
        /// </summary>
        /// <returns>A string response if online</returns>
        private string PollServer()
        {
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
                this.txtSpynServ.Foreground = Brushes.LimeGreen;
            else
                this.txtSpynServ.Foreground = Brushes.Red;
        }

    }
}
