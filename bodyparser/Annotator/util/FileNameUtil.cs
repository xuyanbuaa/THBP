using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Annotator.util
{
    public class FileNameUtil
    {

        public static string FileName(string path)
        {
            int p = path.LastIndexOf("\\");
            if (p == -1)
                return path;
            else
                return path.Substring(p + 1);
        }

        public static string FileNameNoSuffix(string path)
        {
            string t = FileName(path);
            //int p = t.IndexOf(".");
            int p = t.LastIndexOf(".");
            if (p == -1)
                return t;
            else
                return t.Substring(0, p);
        }

        public static string RemoveSuffix(string path)
        {
            /*int p1 = path.LastIndexOf("\\");
            int p2 = path.IndexOf(".", p1 + 1);*/

            int p2 = path.LastIndexOf(".");

            if (p2 == -1)
                return path;
            else
                return path.Substring(0, p2);
        }

    }
}
