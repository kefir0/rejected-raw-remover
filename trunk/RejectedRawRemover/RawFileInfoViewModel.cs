using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using Microsoft.WindowsAPICodePack.Shell;
using XmpUtils;

namespace RejectedRawRemover
{
    public class RawFileInfoViewModel : ViewModelBase
    {
        private readonly RawFileInfo _rawFileInfo;
        private BitmapSource _thumbnail;
        private bool _isLoading;

        public RawFileInfoViewModel(RawFileInfo rawFileInfo)
        {
            _rawFileInfo = rawFileInfo;
        }

        public string Path
        {
            get { return _rawFileInfo.Path; }
        }

        public RawFileInfo RawFileInfo
        {
            get { return _rawFileInfo; }
        }

        public BitmapSource Thumbnail
        {
            get
            {
                if (!_isLoading && _thumbnail == null)
                {
                    _isLoading = true;
                    Task.Factory.StartNew(LoadThumbnail);
                }
                return _thumbnail;
            }
        }

        private void LoadThumbnail()
        {
            var shellFile = ShellFile.FromFilePath(_rawFileInfo.Path);
            _thumbnail = shellFile.Thumbnail.MediumBitmapSource;
            _thumbnail.Freeze();
            RaisePropertyChanged("Thumbnail");
        }

        public string DeleteFiles()
        {
            try
            {
                File.Delete(_rawFileInfo.XmpPath);
                File.Delete(_rawFileInfo.Path);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}