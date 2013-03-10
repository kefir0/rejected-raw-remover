using System.Windows.Controls;

namespace RejectedRawRemover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel = new MainWindowViewModel();
        }

        private void FileListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.SetSelectedItems(fileList.SelectedItems);
        }
    }
}