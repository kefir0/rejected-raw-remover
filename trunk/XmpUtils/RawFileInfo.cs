using System.IO;

namespace XmpUtils
{
    public class RawFileInfo
    {
        public const string RawExtension = "cr2";

        public RawFileInfo(string xmpPath)
        {
            XmpPath = xmpPath;
            Path = System.IO.Path.ChangeExtension(xmpPath, RawExtension);
            Size = new FileInfo(Path).Length;
        }

        public string Path { get; private set; }
        public string XmpPath { get; private set; }
        public long Size { get; private set; }
    }
}