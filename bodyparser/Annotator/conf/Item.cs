using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Annotator;
using Annotator.conf.entity;

using TimeExtractor.units;

namespace Annotator.conf
{

    public enum KeyTimeType
    {

        ADMISSION,
        DISCHARGE,
        OPERATION,
        BIRTH,
        TRANSFER,
        EVALUATION,
        PRESENTATION,
        DELIVERY,

        THISTP,     //referrence point: given in the "tp" field or the "weekday" field

        OTHERS      //not known, must be deduced from contexts

    };

    public enum TimePeriodType
    {

        NONE,

        MORNING,
        NOON,
        AFTERNOON,
        EVENING,
        NIGHT,
        MIDNIGHT,
        DAY

    }

    public enum ItemType
    {

        TEXT,
        TIMEPOINT,
        RELATIVE_TIMEPOINT, //make use of the "offset" attribute
        ORDINAL,
        DIGITS,
        LENGTH,
        RELATIVE_LENGTH,
        TIMEUNITS,
        KEYEVENT,  //trig key events, e.g. POD, HD, ...

        TRIG_OTHERS,
        TRIG_TP,     //the trigger trigs a TP or a key event, e.g. until, prior to, before, after, ...
        TRIG_LENG,       //the trigger trigs a length, e.g. over, in, for, ...
        TRIG_TP_LENG,   //trigger both tps and length
        TRIG_ADVERB,

        //NLP items
        CONJUNCTION,    //and, - (hyphen), ...
        PUNCTUATION,    //only , . :

        //Other types
        MONTH,
        WEEKDAY,

        //Frequency
        FREQUENCY,
        REPEATS

    };

    public class Item
    {

        public Entity entity;
        public List<Item> child;
        public int startLoc, endLoc, depth;
        public ItemType type = ItemType.TEXT;

        //For all types
        public string text;

        public TimeEntityMode mode;

        //For type "TimePoint"
        public TimePoint tp = new TimePoint();   //100 stands for "not known"
        public TimePeriodType tptype = TimePeriodType.NONE;    //morning, evening, ...
        public int weekday = 0; //1-7: record the weekday

        //For type "relative" and "relative timepoint"
        //also for type "digits" and "ordinal"
        //for "month": indicate which month it is
        public double digits = 0;
        public Dictionary<DurationUnit, double> offset = new Dictionary<DurationUnit, double>();

        //for unit:
        public DurationUnit unit = DurationUnit.DAY;

        //For type KeyEvent or "relative timepoint"
        public KeyTimeType keytime = KeyTimeType.OTHERS;

        //for relative timepoint/length
        public TLinkType relation = TLinkType.OVERLAP;

        //for frequencies
        public double repeats = 0; //repeat times

        private string printoffset()
        {
            string ret = "OFFSET: ";
            foreach (KeyValuePair<DurationUnit, double> kvp in offset)
            {
                ret += "(" +  kvp.Key + "," + kvp.Value + ")";
            }
            return ret;
        }

        public override string ToString()
        {
            switch (type)
            {
                case ItemType.CONJUNCTION: return "<CONJ>";
                case ItemType.DIGITS: return "<DIGITS>";
                case ItemType.KEYEVENT: return "<KEYEVENT>";
                case ItemType.LENGTH: return "<LENGTH>";
                case ItemType.ORDINAL: return "<ORDINAL>";
                case ItemType.RELATIVE_TIMEPOINT: return "<RELATIVETP>";
                case ItemType.TEXT: return text;
                case ItemType.TIMEPOINT: return "<TIMEPOINT>";
                case ItemType.TRIG_ADVERB: return "<TRIG_ADVERB>";
                case ItemType.TRIG_LENG: return "<TRIG_LENG>";
                case ItemType.TRIG_OTHERS: return "<TRIG_OTHERS>";
                case ItemType.TRIG_TP: return "<TRIG_TP>";
                case ItemType.TRIG_TP_LENG: return "<TRIG_TP_LENG>";
                case ItemType.MONTH: return "<MONTH>";
                case ItemType.WEEKDAY: return "<WEEKDAY>";
                case ItemType.TIMEUNITS: return "<UNITS>";
                case ItemType.PUNCTUATION: return "<PUNC>";
                case ItemType.RELATIVE_LENGTH: return "<RELATIVE_LENGTH>";
                case ItemType.FREQUENCY: return "<FREQUENCY>";
                case ItemType.REPEATS: return "<REPEATS>";
            }
            return text;
        }

        public string ToFullString()
        {
            switch (type)
            {
                case ItemType.CONJUNCTION: return "<CONJ>: " + text;
                case ItemType.DIGITS: return "<DIGITS>: " + digits;
                case ItemType.KEYEVENT: return "<KEYEVENT>: " + keytime.ToString() + " " + mode.ToString();
                case ItemType.LENGTH: return "<LENGTH>: " + digits + " " + printoffset() + " " + mode.ToString() + " " + mode;
                case ItemType.ORDINAL: return "<ORDINAL>: " + digits;
                case ItemType.RELATIVE_TIMEPOINT: return "<RELATIVETP>: " + keytime.ToString() + " " + digits + " " + printoffset() + " " + mode.ToString() + " " + tptype.ToString() + " " + tp.toString() + " " + mode + " " + relation;
                case ItemType.TEXT: return "<TEXT:>" + text;
                case ItemType.TIMEPOINT: return "<TIMEPOINT>: " + tp.toString() + " " + mode.ToString() + " " + "WK" + weekday;
                case ItemType.TRIG_ADVERB: return "<TRIG_ADVERB>: " + text + " " + mode.ToString();
                case ItemType.TRIG_LENG: return "<TRIG_LENG>: " + text + " " + mode.ToString();
                case ItemType.TRIG_OTHERS: return "<TRIG_OTHERS>: " + text;
                case ItemType.TRIG_TP: return "<TRIG_TP>: " + text + " " + mode.ToString();
                case ItemType.TRIG_TP_LENG: return "<TRIG_TP_LENG>: " + text + " " + mode.ToString();
                case ItemType.MONTH: return "<MONTH>";
                case ItemType.WEEKDAY: return "<WEEKDAY>";
                case ItemType.TIMEUNITS: return "<UNITS>: " + text + " " + digits + " " + printoffset() + " " + unit;
                case ItemType.PUNCTUATION: return "<PUNC>";
                case ItemType.RELATIVE_LENGTH: return "<RELATIVE_LENGTH>: " + keytime.ToString() + " " + digits + " " + printoffset() + " " + mode.ToString() + " " + tptype.ToString() + " " + tp.toString() + " " + mode + relation;
                case ItemType.FREQUENCY: return "<FREQUENCY>: " + repeats + " " + printoffset();
                case ItemType.REPEATS: return "<REPEATS>: " + repeats + " " + printoffset();
            }
            return "<TEXT>: " + text;
        }

        public static string FromItemsToString(IEnumerable<Item> items)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Item item in items)
                sb.Append(" " + item.ToString());
            return sb.ToString();
        }

        public static string FromItesToFullString(IEnumerable<Item> items)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Item item in items)
                sb.Append(item.ToFullString() + "\r\n");
            return sb.ToString();
        }

    }
}
