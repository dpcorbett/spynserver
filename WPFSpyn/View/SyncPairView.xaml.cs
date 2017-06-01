using SharpTools.Log;
using System;
using System.ComponentModel;
using System.Windows;
using WPFSpyn.ViewModel;

namespace WPFSpyn.View
{
    /// <summary>
    /// Interaction logic for SyncPairView.xaml
    /// </summary>
    public partial class SyncPairView : System.Windows.Controls.UserControl
    {
        // Create logger.
        private static readonly log4net.ILog _log = SharpToolsLog.GetLogger();

        /// <summary>
        /// Constructor that initialises gui and attaches method to data context changed event.
        /// </summary>
        public SyncPairView()
        {
            InitializeComponent();
            // LOG
            _log.Debug("init sync pair view complete.");
            //
            DataContextChanged += TreeViewPath_DataContextChanged;
            Loaded += new RoutedEventHandler(ViewModel_UpdateDirectoryPath);
        }

        /// <summary>
        /// Attach data context's new value to update directory path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewPath_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SyncPairViewModel spvm = (SyncPairViewModel)e.NewValue;
            if (spvm != null)
                spvm.UpdateDirectoryPath += ViewModel_UpdateDirectoryPath;
        }

        /// <summary>
        /// Update directory tree view with data context's new value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewModel_UpdateDirectoryPath(object sender, EventArgs e)
        {
            SyncPairViewModel spvm = (SyncPairViewModel)DataContext;
            if (spvm != null)
            {
                // Refresh source directory tree view.
                trvSrcDir.populateTreeView(this, new PropertyChangedEventArgs(spvm.SrcRoot));
                // Refresh destination directory tree view.
                trvDstDir.populateTreeView(this, new PropertyChangedEventArgs(spvm.DstRoot));
            }
        }

        // <summary>
        /// Update progress bar status view with data context's new value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewModel_UpdateProgressBar(object sender, EventArgs e)
        {
            SyncPairViewModel spvm = (SyncPairViewModel)DataContext;
            if (spvm != null)
            {
                // Refresh source directory tree view.
                pbrSyncProgress.Value = Convert.ToDouble(e.ToString());
            }

        }

    }
}
