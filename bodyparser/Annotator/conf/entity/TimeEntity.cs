using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TimeExtractor.units;
using TimeExtractor.util;

using Annotator.util;

namespace Annotator.conf.entity
{


    public enum TimeEntityType
    {
        DATE, TIME, DURATION, FREQUENCY, RELATIVE_TP
    }

    public enum TimeEntityMode
    {
        NA , APPROX , MORE , LESS , START , END , MIDDLE
    }

    public enum DurationUnit
    {
        YEAR, MONTH, WEEK, DAY, HOUR, MINUTE, SECOND
    }

    public enum TLinkType
    {
        BEFORE, AFTER, OVERLAP, OTHER
    }

    public class TimeEntity : Entity
    {

        public TimeEntityType type = TimeEntityType.DATE;

        public TimeEntityMode mode = TimeEntityMode.NA;

        public KeyTimeType keyev = KeyTimeType.OTHERS;

        public TLinkType relation = TLinkType.OVERLAP;

        public Dictionary<DurationUnit, double> duration = new Dictionary<DurationUnit, double>();
        public double repeat_times = 0;

        public override string ToI2b2Format(bool StemOnly)
        {

            if (this.text == "09/08/96")
                StemOnly = StemOnly;

            /*string stem = "c=\"" + text + "\" " + startLoc.line + ":" + startLoc.col + " " + endLoc.line + ":" + endLoc.col + "||t=\"" + type + "\"";
            if (StemOnly)
                return stem;
            TimePoint firstTP = getFirstTimePoint();
            TimePoint lastTP = getLastTimePoint();
            stem += "||normalization=" + TimePeriodUtil.TimePointToString(firstTP, true, false);
            if (!lastTP.equals(firstTP))
                stem += "||normalization=" + TimePeriodUtil.TimePointToString(lastTP, true, false);
            return stem;*/

            string stem = "TIMEX3=\"" + text + "\" " + (startLoc.line) + ":" + startLoc.col + " " + (endLoc.line) + ":" + endLoc.col;
            if (StemOnly)
                return stem;
            stem += "||type=\"";
            switch (type)
            {
                case TimeEntityType.DATE: stem += "DATE"; break;
                case TimeEntityType.TIME: stem += "TIME"; break;
                case TimeEntityType.DURATION: stem += "DURATION"; break;
                case TimeEntityType.FREQUENCY: stem += "FREQUENCY"; break;
                case TimeEntityType.RELATIVE_TP: stem += "RELATIVE"; break;
            }
            stem += "\"||val=\"";

            string val = EntityUtil.TimeEntityGetVal(this);
            stem += val;

            stem += "\"||mod=\"";
            switch (mode)
            {
                case TimeEntityMode.NA: stem += "NA"; break;
                case TimeEntityMode.APPROX: stem += "APPROX"; break;
                case TimeEntityMode.END: stem += "END"; break;
                case TimeEntityMode.LESS: stem += "LESS"; break;
                case TimeEntityMode.MIDDLE: stem += "MIDDLE"; break;
                case TimeEntityMode.MORE: stem += "MORE"; break;
                case TimeEntityMode.START: stem += "START"; break;
            }
            stem += "\"";

            /*if (is_relative_tp)
                stem += "||relative";*/

            return stem;

        }

        public override string ToAnnotateFormat(bool StemOnly)
        {
            string stem = @"``" + text + @"``";
            if (StemOnly)
                return stem;
            TimePoint firstTP = getFirstTimePoint();
            TimePoint lastTP = getLastTimePoint();
            stem += TimePeriodUtil.TimePointToString(firstTP, false, true);
            if (!lastTP.equals(firstTP))
                stem += TimePeriodUtil.TimePointToString(lastTP, false, true);
            return stem;
        }

    }

}
