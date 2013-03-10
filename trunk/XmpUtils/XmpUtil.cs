using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XmpUtils
{
    public class XmpUtil
    {
        public const string XmpExtension = "xmp";
        public const string RejectedToken = "xmp:Rating=\"-1\"";

        public static IEnumerable<string> GetRejectedXmpFilePaths(string rootDir, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.GetFiles(rootDir, "*." + XmpExtension, searchOption).Where(f => File.ReadAllText(f).Contains(RejectedToken));
        }

        public static IEnumerable<RawFileInfo> GetRejectedXmpFileInfos(string rootDir, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return GetRejectedXmpFilePaths(rootDir, searchOption).Select(f => new RawFileInfo(f));
        }
    }
}