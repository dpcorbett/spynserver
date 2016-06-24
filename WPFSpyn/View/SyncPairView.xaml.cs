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
        /// <summary>
        /// Constructor that initialises gui and attaches method to data context changed event.
        /// </summary>
        public SyncPairView()
        {
            InitializeComponent();
            DataContextChanged += TreeViewPath_DataContextChanged;
            this.Loaded += new RoutedEventHandler(ViewModel_UpdateDirectoryPath);
        }

        /// <summary>
        /// Attach data context's new value to update directory path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewPath_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SyncPairViewModel vm = (SyncPairViewModel)e.NewValue;
            if (vm != null)
                vm.UpdateDirectoryPath += ViewModel_UpdateDirectoryPath;
        }

        /// <summary>
        /// Update directory tree view with data context's new valu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewModel_UpdateDirectoryPath(object sender, EventArgs e)
        {
            SyncPairViewModel obj = (SyncPairViewModel)DataContext;
            // Refresh source directory tree view.
            trvSrcDir.populateTreeView(this, new PropertyChangedEventArgs(obj.SrcRoot));
            // Refresh destination directory tree view.
            trvDstDir.populateTreeView(this, new PropertyChangedEventArgs(obj.DstRoot));
        }

    }
}
