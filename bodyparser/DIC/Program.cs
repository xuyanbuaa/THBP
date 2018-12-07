using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

using CRFEventExtraction.conf;
using CRFEventExtraction.util;
using CRFEventExtraction.tools.CRFpp;

using Annotator.conf;
using Annotator.conf.entity;
using Annotator.util;


namespace DIC
{
    static class Program
    {
        static void Main()
        {
            int op = 1;
            switch (op)
            {
                case 0:
                    {
                        anatomydict.setdict();
                        break;
                    }
                case 1:
                    {
                        anatomydict.setpartdict();
                        break;
                    }
            }
        }
    }
}
