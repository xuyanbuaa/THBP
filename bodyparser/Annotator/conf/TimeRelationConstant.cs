using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Annotator.conf
{
    public class TimeRelationConstant : IEquatable<TimeRelationConstant>
    {

        private string type_;

        public string type
        {
            get { return type_; }
            set { type_ = value; }
        }

        public TimeRelationConstant()
        {
            type_ = "unknown";
        }

        public TimeRelationConstant(string str)
        {
            type_ = str;
        }

        public bool Equals(TimeRelationConstant trc)
        {
            if (this.type_ == trc.type_)
                return true;
            else
                return false;
        }

        public static TimeRelationConstant UNKNOWN = new TimeRelationConstant("unknown");
        public static TimeRelationConstant BEFORE = new TimeRelationConstant("before");
        public static TimeRelationConstant AFTER = new TimeRelationConstant("after");
        public static TimeRelationConstant SIMULTANEOUSLY = new TimeRelationConstant("simultaneously");

    }
}
