using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Annotator.conf
{
    public class TimeClassificationConstant : IEquatable<TimeClassificationConstant>
    {

        private string type_;

        public string type
        {
            get { return type_; }
            set { type_ = value; }
        }

        public TimeClassificationConstant()
        {
            type_ = "";
        }

        public TimeClassificationConstant(string str)
        {
            type_ = str;
        }

        public bool Equals(TimeClassificationConstant tcc)
        {
            if (tcc.type_ == type_)
                return true;
            else
                return false;
        }

        public static TimeClassificationConstant NON = new TimeClassificationConstant("");
        public static TimeClassificationConstant U = new TimeClassificationConstant("u"); //unknown
        public static TimeClassificationConstant WA = new TimeClassificationConstant("wa");
        public static TimeClassificationConstant BA = new TimeClassificationConstant("ba");
        public static TimeClassificationConstant A = new TimeClassificationConstant("a");
        public static TimeClassificationConstant AA = new TimeClassificationConstant("aa");
        public static TimeClassificationConstant AD = new TimeClassificationConstant("ad");

    }
}
