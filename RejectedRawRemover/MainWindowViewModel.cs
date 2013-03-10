using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _dispatcherTimer;
        private RelayCommand _browseRootDirCommand;
        private RelayCommand _startSearchCommand;
        private RelayCommand _stopSearchCommand;
        private string _currentDir;
        private string _currentFile;
        private bool _isSearching;
        private ConcurrentBag<RawFileInfo> _rejectedFiles;
        private readonly HashSet<RawFileInfo> _rejectedFilesSet = new HashSet<RawFileInfo>();
        private string _rootDir = @"d:\Photo";
        private CancellationTokenSource _searchCancellationTokenSource;

        public MainWindowViewModel()
        {
            RejectedFiles = new ObservableCollection<RawFileInfo>();
            _dispatcher = Dispatcher.CurrentDispatcher;
            _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
                                   {
                                       Interval = TimeSpan.FromMilliseconds(300),
                                       IsEnabled = true
                                   };
            _dispatcherTimer.Tick += UpdateProgress;
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

        public string CurrentDir
        {
            get { return _currentDir; }
            set
            {
                _currentDir = value;
                RaisePropertyChanged("CurrentDir");
            }
        }

        public long ProcessedFileCount { get; private set; }

        public long TotalSize { get; private set; }

        private void UpdateProgress(object sender, EventArgs e)
        {
            if (!IsSearching)
                return;

            RaisePropertyChanged("ProcessedFileCount");
            RaisePropertyChanged("TotalSize");

            if (!string.IsNullOrEmpty(_currentFile))
                CurrentDir = Path.GetDirectoryName(_currentFile);

            foreach (var rejectedFile in _rejectedFiles)
            {
                if (!_rejectedFilesSet.Contains(rejectedFile))
                {
                    _rejectedFilesSet.Add(rejectedFile);
                    RejectedFiles.Add(rejectedFile);
                }
            }
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
                _rejectedFiles = new ConcurrentBag<RawFileInfo>();
                _rejectedFilesSet.Clear();
                TotalSize = 0;
                ProcessedFileCount = 0;
                foreach (var fileInfo in XmpUtil.GetRejectedXmpFileInfos(RootDir, OnFileProcessed))
                {
                    _rejectedFiles.Add(fileInfo);
                    TotalSize += fileInfo.Size/(1024*1024);  // In megabytes
                    _searchCancellationTokenSource.Token.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                _dispatcher.BeginInvoke((Action) (() => { IsSearching = false; }));
            }
        }

        private void OnFileProcessed(string file)
        {
            ProcessedFileCount++;
            _currentFile = file;
        }
    }
}