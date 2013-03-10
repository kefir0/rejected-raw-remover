using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using XmpUtils;

namespace RejectedRawRemover
{
    public class MainWindowViewModel : ViewModelBase
    {
        private CancellationTokenSource _searchCancellationTokenSource;
        private RelayCommand _browseRootDirCommand;
        private bool _isSearching;
        private string _rootDir = @"d:\Photo";
        private RelayCommand _startSearchCommand;
        private RelayCommand _stopSearchCommand;
        private Dispatcher _dispatcher;

        public MainWindowViewModel()
        {
            RejectedFiles = new ObservableCollection<RawFileInfo>();
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableCollection<RawFileInfo> RejectedFiles { get; private set; }

        public RelayCommand StartSearchCommand
        {
            get { return _startSearchCommand ?? (_startSearchCommand = new RelayCommand(StartSearch, CanStartSearch)); }
        }

        public RelayCommand StopSearchCommand
        {
            get { return _stopSearchCommand ?? (_stopSearchCommand = new RelayCommand(StopSearch, () => IsSearching)); }
        }

        public string RootDir
        {
            get { return _rootDir; }
            set
            {
                _rootDir = value;
                RaisePropertyChanged("RootDir");
                RaisePropertyChanged("IsRootDirValid");
            }
        }

        public bool IsRootDirValid
        {
            get { return Directory.Exists(RootDir); }
        }

        public bool IsSearching
        {
            get { return _isSearching; }
            private set
            {
                _isSearching = value;
                RaisePropertyChanged("IsSearching");
                CommandManager.InvalidateRequerySuggested();
                //StartSearchCommand.RaiseCanExecuteChanged();
                //StopSearchCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand BrowseRootDirCommand
        {
            get { return _browseRootDirCommand ?? (_browseRootDirCommand = new RelayCommand(BrowseRootDir, () => !IsSearching)); }
        }

        private bool CanStartSearch()
        {
            Debug.WriteLine("CanStartSearch=" + (!IsSearching && IsRootDirValid));
            return !IsSearching && IsRootDirValid;
        }

        private void BrowseRootDir()
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                RootDir = dlg.SelectedPath;
            }
        }

        private void StopSearch()
        {
            _searchCancellationTokenSource.Cancel();
        }

        private void StartSearch()
        {
            RejectedFiles.Clear();
            _searchCancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(SearchImpl, _searchCancellationTokenSource.Token);
        }

        private void SearchImpl()
        {
            try
            {
                IsSearching = true;
                while (true)
                {
                    _searchCancellationTokenSource.Token.ThrowIfCancellationRequested();
                    Thread.Sleep(100);
                }
            }
            finally
            {
                _dispatcher.BeginInvoke((Action) (() =>
                                                      {
                                                          IsSearching = false;
                                                      }));
            }
        }
    }
}