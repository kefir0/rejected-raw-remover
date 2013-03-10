using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XmpUtils
{
    public class XmpUtil
    {
        public const string XmpExtension = "xmp";
        public const string RejectedToken = "xmp:Rating=\"-1\"";

        public static IEnumerable<string> GetRejectedXmpFilePaths(string rootDir, Action<string> currentFileAction, SearchOption searchOption = SearchOption.AllDirectories)
        {
            foreach (var f in Directory.GetFiles(rootDir, "*." + XmpExtension, searchOption))
            {
                if (currentFileAction != null)
                    currentFileAction(f);

                if (File.ReadAllText(f).Contains(RejectedToken)) 
                    yield return f;
            }
        }

        public static IEnumerable<RawFileInfo> GetRejectedXmpFileInfos(string rootDir, Action<string> currentFileAction = null, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return GetRejectedXmpFilePaths(rootDir, currentFileAction, searchOption).Select(f => new RawFileInfo(f));
        }
    }
}