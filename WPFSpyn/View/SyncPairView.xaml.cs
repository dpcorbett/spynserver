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
        public SyncPairView()
        {
            InitializeComponent();
            DataContextChanged += TreeViewPath_DataContextChanged;

        }

        private void TreeViewPath_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SyncPairViewModel vm = (SyncPairViewModel)e.NewValue;
            if (vm != null)
                vm.UpdateDirectoryPath += ViewModel_UpdateDirectoryPath;
        }

        private void ViewModel_UpdateDirectoryPath(object sender, EventArgs e)
        {

            trvSrcDir.reloadTreeView(this, new PropertyChangedEventArgs(srcRootTxt.Text));
        }

    }
}
