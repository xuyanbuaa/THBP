using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Annotator.conf;
using Annotator.conf.entity;

using TimeExtractor.units;
using TimeExtractor.util;
using TimeExtractor.conf;

namespace Annotator.util
{
    public class EntityUtil
    {

        public static string AppendZero(int x, int leng)
        {
            string s = Convert.ToString(x);
            while (s.Length < leng)
                s = "0" + s;
            return s;
        }

        public static string GetDurationExpression(Dictionary<DurationUnit, double> duration)
        {
            bool istime = false;
            string s = "";
            foreach (KeyValuePair<DurationUnit, double> kvp in duration)
            {
                if (kvp.Value == 0) continue;
                string t = Convert.ToString(Math.Abs(kvp.Value));
                int p = t.LastIndexOf(".");
                if (p != -1 && p + 2 < t.Length)
                    t = t.Substring(0, p + 3);
                s += t;
                switch (kvp.Key)
                {
                    case DurationUnit.DAY: s += "D"; break;
                    case DurationUnit.HOUR: s += "H"; istime = true; break;
                    case DurationUnit.MINUTE: s += "M"; istime = true; break;
                    case DurationUnit.MONTH: s += "M"; break;
                    case DurationUnit.SECOND: s += "S"; istime = true; break;
                    case DurationUnit.WEEK: s += "W"; break;
                    case DurationUnit.YEAR: s += "Y"; break;
                }
            }
            if (istime)
                return "PT" + s;
            else
                return "P" + s;
        }

        public static Dictionary<DurationUnit, double> GetDurationExpression(string val)
        {

            val = val.Substring(1);

            val = val.ToUpper();

            if (val == "")
                return new Dictionary<DurationUnit, double>();

            bool istime = false;
            if (val[0] == 'T')
            {
                istime = true;
                val = val.Substring(1);
            }

            Dictionary<DurationUnit, double> ret = new Dictionary<DurationUnit, double>();
            Regex regex = new Regex(@"(?<value>[^A-Z]+)(?<unit>[A-Z])");
            MatchCollection matches = regex.Matches(val);
            foreach (Match match in matches)
            {
                double value = Convert.ToDouble(match.Groups["value"].Value.Trim(new char[] {'.'}));
                char unit = match.Groups["unit"].Value[0];
                if (value == 0) continue;
                switch (unit)
                {
                    case 'Y': if (!ret.ContainsKey(DurationUnit.YEAR)) ret.Add(DurationUnit.YEAR, value); else ret[DurationUnit.YEAR] += value; break;
                    case 'D': if (!ret.ContainsKey(DurationUnit.DAY)) ret.Add(DurationUnit.DAY, value); else ret[DurationUnit.DAY] += value; break;
                    case 'H': if (!ret.ContainsKey(DurationUnit.HOUR)) ret.Add(DurationUnit.HOUR, value); else ret[DurationUnit.HOUR] += value; break;
                    case 'S': if (!ret.ContainsKey(DurationUnit.SECOND)) ret.Add(DurationUnit.SECOND, value); else ret[DurationUnit.SECOND] += value; break;
                    case 'M':
                        {
                            DurationUnit u = (istime) ? DurationUnit.MINUTE : DurationUnit.MONTH;
                            if (!ret.ContainsKey(u)) ret.Add(u, value); else ret[u] += value;
                            break;
                        }
                    case 'W': if (!ret.ContainsKey(DurationUnit.WEEK)) ret.Add(DurationUnit.WEEK, value); else ret[DurationUnit.WEEK] += value; break;
                }
            }

            return ret;

        }

        public static string TimeEntityGetVal(TimeEntity e)
        {
            //TimePoint tp;
            //string s = "";
            //switch (e.type)
            //{
            //    case TimeEntityType.DATE:
            //        tp = e.getFirstTimePoint();
            //        if (tp.getYear() == TimeConstants.DEFAULT_VALUE) return "";
            //        s = Convert.ToString(tp.getYear());
            //        if (tp.getMonth() != TimeConstants.DEFAULT_VALUE) s += "-" + AppendZero(tp.getMonth(), 2);
            //        if (tp.getDay() != TimeConstants.DEFAULT_VALUE) s += "-" + AppendZero(tp.getDay(), 2);
            //        return s;

            //    case TimeEntityType.TIME:
            //        tp = e.getFirstTimePoint();
            //        if (tp.apm == ApmType.PM && tp.getHour() < 12)
            //            tp.setHour(tp.getHour() + 12);
            //        s = "T" + AppendZero(tp.getHour(), 2) + ":" + 
            //            AppendZero(((tp.getMinute() == TimeConstants.DEFAULT_VALUE)? 0 : tp.getMinute()), 2);
            //        if (tp.getDay() != TimeConstants.DEFAULT_VALUE)
            //            s = tp.getYear() + "-" + 
            //                AppendZero(tp.getMonth(), 2) + "-" + 
            //                AppendZero((tp.getDay() == TimeConstants.DEFAULT_VALUE)? 1 : tp.getDay(), 2) + s;
            //        return s;

            //    case TimeEntityType.DURATION:
            //        return GetDurationExpression(e.duration);

            //    case TimeEntityType.FREQUENCY:
            //        s = "R";
            //        if (e.repeat_times > 0) s += e.repeat_times;
            //        if (e.duration == null || e.duration.Count == 0)
            //            return s;
            //        return s + GetDurationExpression(e.duration);

            //    case TimeEntityType.RELATIVE_TP:
            //        s = GetDurationExpression(e.duration);
            //        s += ":" + ((e.relation != TLinkType.BEFORE) ? "+" : "-") + ":" + e.keyev;
            //        return s;
                    
            //}

            return "";
        }

        public static void ValGetTimeEntity(string val, TimeEntity e)
        {

            string origval = val;

            val = val.ToUpper();
            val = val.Replace(" ", "");
            val = val.Replace("\t", "");

            if (Regex.IsMatch(val, @"\:(\+|\-)\:"))
            {
                e.type = TimeEntityType.RELATIVE_TP;
                int p1 = val.IndexOf(":");
                int p2 = val.LastIndexOf(":");
                string val_dur = val.Substring(0, p1);
                string val_keyev = val.Substring(p2 + 1);
                string val_rel = val.Substring(p1 + 1, 1);
                e.duration = GetDurationExpression(val_dur);
                e.relation = (val_rel == "-") ? TLinkType.BEFORE : TLinkType.AFTER;
                if (val_keyev == "ADMISSION")
                    e.keyev = KeyTimeType.ADMISSION;
                else if (val_keyev == "DISCHARGE")
                    e.keyev = KeyTimeType.DISCHARGE;
                else if (val_keyev == "OPERATION")
                    e.keyev = KeyTimeType.OPERATION;
                else if (val_keyev == "TRANSFER")
                    e.keyev = KeyTimeType.TRANSFER;
                else if (val_keyev == "LIFE" || val_keyev == "BIRTH")
                    e.keyev = KeyTimeType.BIRTH;
                else if (val_keyev == "OTHERS" || val_keyev == "THISTP")
                    e.keyev = KeyTimeType.OTHERS;
                return;
            }

            if (val == "")
            {
                //Date
                TimePoint tp = new TimePoint();
                e.setTimePoint(tp);
                return;
            }

            Regex dateRegex1 = new Regex(@"^(?<year>[0-9]+)\-(?<month>[0-9]+)\-(?<day>[0-9]+)$");
            if (dateRegex1.IsMatch(val))
            {
                e.type = TimeEntityType.DATE;
                Match match = dateRegex1.Match(val);
                TimePoint tp = new TimePoint(Convert.ToInt32(match.Groups["year"].Value), Convert.ToInt32(match.Groups["month"].Value), Convert.ToInt32(match.Groups["day"].Value));
                e.setTimePoint(tp);
                return;
            }

            Regex dateRegex2 = new Regex(@"^(?<year>[0-9]+)\-(?<month>[0-9]+)$");
            if (dateRegex2.IsMatch(val))
            {
                e.type = TimeEntityType.DATE;
                Match match = dateRegex2.Match(val);
                TimePoint tp = new TimePoint(Convert.ToInt32(match.Groups["year"].Value), Convert.ToInt32(match.Groups["month"].Value), TimeConstants.DEFAULT_VALUE);
                e.setTimePoint(tp);
                return;
            }

            Regex dateRegex3 = new Regex(@"^(?<year>[0-9]+)$");
            if (dateRegex3.IsMatch(val))
            {
                e.type = TimeEntityType.DATE;
                Match match = dateRegex3.Match(val);
                TimePoint tp = new TimePoint(Convert.ToInt32(match.Groups["year"].Value), TimeConstants.DEFAULT_VALUE, TimeConstants.DEFAULT_VALUE);
                e.setTimePoint(tp);
                return;
            }

            Regex timeRegex1 = new Regex(@"^(?<year>[0-9]+)\-(?<month>[0-9]+)\-(?<day>[0-9]+)T(?<hour>[0-9]+)\:(?<minute>[0-9]+)$");
            if (timeRegex1.IsMatch(val))
            {
                e.type = TimeEntityType.TIME;
                Match match = timeRegex1.Match(val);
                TimePoint tp = new TimePoint(Convert.ToInt32(match.Groups["year"].Value), Convert.ToInt32(match.Groups["month"].Value), Convert.ToInt32(match.Groups["day"].Value));
                tp.setHour(Convert.ToInt32(match.Groups["hour"].Value));
                tp.setMinute(Convert.ToInt32(match.Groups["minute"].Value));
                e.setTimePoint(tp);
                return;
            }

            Regex timeRegex2 = new Regex(@"^T(?<hour>[0-9]+)\:(?<minute>[0-9]+)$");
            if (timeRegex2.IsMatch(val))
            {
                e.type = TimeEntityType.TIME;
                Match match = timeRegex2.Match(val);
                TimePoint tp = new TimePoint();
                tp.setHour(Convert.ToInt32(match.Groups["hour"].Value));
                tp.setMinute(Convert.ToInt32(match.Groups["minute"].Value));
                e.setTimePoint(tp);
                return;
            }

            Regex timeRegex3 = new Regex(@"^T(?<hour>[0-9][0-9])(?<minute>[0-9][0-9])$");
            if (timeRegex3.IsMatch(val))
            {
                e.type = TimeEntityType.TIME;
                Match match = timeRegex3.Match(val);
                TimePoint tp = new TimePoint();
                tp.setHour(Convert.ToInt32(match.Groups["hour"].Value));
                tp.setMinute(Convert.ToInt32(match.Groups["minute"].Value));
                e.setTimePoint(tp);
                return;
            }

            Regex timeRegex4 = new Regex(@"^(?<year>[0-9]+)\-(?<month>[0-9]+)\-(?<day>[0-9]+)T(?<hour>[0-9][0-9])(?<minute>[0-9][0-9])$");
            if (timeRegex4.IsMatch(val))
            {
                e.type = TimeEntityType.TIME;
                Match match = timeRegex4.Match(val);
                TimePoint tp = new TimePoint(Convert.ToInt32(match.Groups["year"].Value), Convert.ToInt32(match.Groups["month"].Value), Convert.ToInt32(match.Groups["day"].Value));
                tp.setHour(Convert.ToInt32(match.Groups["hour"].Value));
                tp.setMinute(Convert.ToInt32(match.Groups["minute"].Value));
                e.setTimePoint(tp);
                return;
            }



            Regex tRegex = new Regex(@"T(?<hour>[0-9]+)\:(?<minute>[0-9]+)$");
            if (tRegex.IsMatch(val))
            {
                Match match = tRegex.Match(val);
                TimePoint tp = new TimePoint();
                tp.setHour(Convert.ToInt32(match.Groups["hour"].Value));
                tp.setMinute(Convert.ToInt32(match.Groups["minute"].Value));
                e.setTimePoint(tp);
            }

            
            val = tRegex.Replace(val, "");

            Regex absFreqRegex = new Regex(@"R(?<repeats>[0-9]*)$");

            if (absFreqRegex.IsMatch(val))
            {
                Match match = absFreqRegex.Match(val);
                e.type = TimeEntityType.FREQUENCY;
                e.repeat_times = 0;
                e.duration = new Dictionary<DurationUnit, double>();
                if (match.Groups["repeats"].Value.Length > 0)
                    e.repeat_times = Convert.ToInt32(match.Groups["repeats"].Value);
                return;
            }
            else if (val.StartsWith("R"))
            {

                if (val.IndexOf("P") == -1)
                    val = val.Replace("R", "RP");

                e.type = TimeEntityType.FREQUENCY;
                int p = val.IndexOf("P");
                string t = val.Substring(1, p - 1);
                if (t == "")
                    e.repeat_times = 0;
                else
                    e.repeat_times = Convert.ToDouble(t);
                val = val.Substring(p);
            }
            else
            {
                e.type = TimeEntityType.DURATION;
            }

            e.duration = GetDurationExpression(val);

        }

        public static string RemoveNoise(string s)
        {
            bool bFlag = true;
            while (bFlag)
            {
                if (s.EndsWith("-")
                    || s.EndsWith(".")
                    || s.EndsWith(":")
                    || s.EndsWith(";")
                    || s.EndsWith("+")
                    || s.EndsWith(" ")
                    || s.EndsWith("(")
                    || s.EndsWith("/")
                    || s.EndsWith("\\")
                    || s.EndsWith("*"))
                {
                    s = s.Substring(0, s.Length - 1);
                    continue;
                }
                bFlag = false;
            }
            return s;
        }

        private static Entity get_entity(string s, FileRecord fr)
        {
            Regex regex = new Regex(@"(?<cate>.+)=""(?<text>.+)"" (?<sline>[0-9]+)\:(?<scol>[0-9]+) (?<eline>[0-9]+)\:(?<ecol>[0-9]+)");
            Match match = regex.Match(s);
            int line = Convert.ToInt32(match.Groups["sline"].Value);
            int scol = Convert.ToInt32(match.Groups["scol"].Value);
            int ecol = Convert.ToInt32(match.Groups["ecol"].Value);

            foreach (Entity entity in fr.entities)
                if (entity.startLoc.line == line && entity.startLoc.col == scol && entity.endLoc.col == ecol)
                    return entity;

            return null;
        }

        private static Entity get_entity_overlap(string s, FileRecord fr)
        {
            Regex regex = new Regex(@"(?<cate>.+)=""(?<text>.+)"" (?<sline>[0-9]+)\:(?<scol>[0-9]+) (?<eline>[0-9]+)\:(?<ecol>[0-9]+)");
            Match match = regex.Match(s);
            int line = Convert.ToInt32(match.Groups["sline"].Value);
            int scol = Convert.ToInt32(match.Groups["scol"].Value);
            int ecol = Convert.ToInt32(match.Groups["ecol"].Value);

            foreach (Entity entity in fr.entities)
                if (entity.startLoc.line == line && entity.startLoc.col <= scol && ecol <= entity.endLoc.col)
                    return entity;

            return null;
        }

        public static EntityLink I2b2formToTLink(string i2b2str, string filename, FileRecord fr)
        {
            string[] terms = i2b2str.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            if (terms.Length != 3) 
                return null;
            Entity efrom = get_entity(terms[0], fr);
            Entity eto = get_entity(terms[1], fr);
            if (efrom == null)
                efrom = get_entity_overlap(terms[0], fr);
            if (eto == null)
                eto = get_entity_overlap(terms[1], fr);
            if (efrom == null || eto == null) 
                return null;

            EntityLink ret = new EntityLink();
            ret.from = efrom;
            ret.to = eto;

            Regex regex_type = new Regex(@"^type=""(?<type>.+)""$");
            Match match = regex_type.Match(terms[2]);
            string s = match.Groups["type"].Value;
            if (s == "BEFORE")
                ret.type = TLinkType.BEFORE;
            else if (s == "AFTER")
                ret.type = TLinkType.AFTER;
            else if (s == "OVERLAP")
                ret.type = TLinkType.OVERLAP;
            else
                ret.type = TLinkType.OTHER;

            return ret;
            
        }

        public static Entity I2b2formToEntity(string i2b2str, string filename)
        {


            Entity ret = null;

            if (i2b2str.IndexOf("three years prior") != -1)
                ret = ret;

            if (i2b2str.IndexOf("~~") == -1)
            {
                string[] terms = i2b2str.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                Regex conregex = new Regex(@"""(?<con>[A-Za-z_]+)""");
                Match matchcon = conregex.Match(terms[1]);
                string con = matchcon.Groups["con"].Value;

                if (con == "time")
                {
                    ret = new TimeEntity();
                    for (int i = 2; i < terms.Length; i++)
                    {
                        Regex normRegex = new Regex(@"normalization=\((?<year>[0-9]+),(?<month>[0-9]+),(?<day>[0-9]+),(?<hour>[0-9]+),(?<minute>[0-9]+),(?<second>[0-9]+)\)");
                        Match normMatch = normRegex.Match(terms[i]);
                        int year = Convert.ToInt32(normMatch.Groups["year"].Value);
                        int month = Convert.ToInt32(normMatch.Groups["month"].Value);
                        int day = Convert.ToInt32(normMatch.Groups["day"].Value);
                        int hour = Convert.ToInt32(normMatch.Groups["hour"].Value);
                        int minute = Convert.ToInt32(normMatch.Groups["minute"].Value);
                        int second = Convert.ToInt32(normMatch.Groups["second"].Value);
                        TimePoint tp = new TimePoint(year, month, day, hour, minute, second);
                        ret.addTimePoint(tp);
                    }
                    ret.type = con;
                }

                else if (terms[0].StartsWith("sectime", StringComparison.CurrentCultureIgnoreCase))
                {
                    Regex regex = new Regex(@"dvalue=""(?<year>[0-9]+)-(?<month>[0-9]+)-(?<day>[0-9]+)""");
                    Match match = regex.Match(terms[2]);

                    int year = Convert.ToInt32(match.Groups["year"].Value);
                    int month = Convert.ToInt32(match.Groups["month"].Value);
                    int day = Convert.ToInt32(match.Groups["day"].Value);

                    TimeEntity te = new TimeEntity();

                    te.mode = TimeEntityMode.NA;
                    te.type = TimeEntityType.DATE;
                    te.setTimePoint(new TimePoint(year, month, day));

                    ret = te;
                    ret.type = "time";
                }

                else if (con == "DATE" || con == "TIME" || con == "DURATION" || con == "FREQUENCY" || con == "RELATIVE")
                {

                    TimeEntity te = new TimeEntity();

                    //value
                    if (terms.Length > 2)
                    {
                        string val = terms[2].Substring(5, terms[2].Length - 6);
                        ValGetTimeEntity(val, te);
                    }

                    //mode
                    if (terms.Length > 2)
                    {
                        Regex modeRegex = new Regex(@"""(?<mod>[A-Za-z]+)""$");
                        Match match = modeRegex.Match(terms[3]);
                        string mod = match.Groups["mod"].Value.ToUpper();
                        if (mod == "NA") te.mode = TimeEntityMode.NA;
                        else if (mod == "APPROX") te.mode = TimeEntityMode.APPROX;
                        else if (mod == "END") te.mode = TimeEntityMode.END;
                        else if (mod == "LESS") te.mode = TimeEntityMode.LESS;
                        else if (mod == "MORE") te.mode = TimeEntityMode.MORE;
                        else if (mod == "MIDDLE") te.mode = TimeEntityMode.MIDDLE;
                        else if (mod == "START") te.mode = TimeEntityMode.START;
                    }
                    //type
                    if (con == "DATE")
                        te.type = TimeEntityType.DATE;
                    else if (con == "TIME")
                        te.type = TimeEntityType.TIME;
                    else if (con == "DURATION")
                        te.type = TimeEntityType.DURATION;
                    else if (con == "FREQUENCY")
                        te.type = TimeEntityType.FREQUENCY;
                    else
                        te.type = TimeEntityType.RELATIVE_TP;

                    if (terms[terms.Length - 1].IndexOf("relative") != -1)
                        te.is_relative_tp = true;

                    ret = te;
                    ret.type = con;


                }

                else if (con.ToUpper() == "IMPLICIT" || con.ToUpper() == "EXPLICIT" || con.ToUpper() == "TREATMENT" ||
                    con.ToUpper() == "CLINICAL_DEPT" || con.ToUpper() == "EVIDENTIAL" || con.ToUpper() == "OCCURRENCE")
                {

                    PMTEntity pe = new PMTEntity();
                    pe.type = con.ToUpper();

                    //modality
                    if (terms.Length > 2)
                    {
                        Regex modRegex = new Regex(@"""(?<mod>[A-Za-z]+)""$");
                        Match match = modRegex.Match(terms[2]);
                        string mode = match.Groups["mod"].Value.ToUpper();
                        if (mode == "CONDITIONAL")
                            pe.modality = Modality.CONDITIONAL;
                        else if (mode == "FACTUAL" || mode == "ACTUAL")
                            pe.modality = Modality.FACTUAL;
                        else if (mode == "POSSIBLE")
                            pe.modality = Modality.POSSIBLE;
                        else
                            pe.modality = Modality.PROPOSED;
                    }

                    //polarity
                    if (terms.Length > 2)
                    {
                        Regex polRegex = new Regex(@"""(?<pol>[A-Za-z]+)""$");
                        Match match = polRegex.Match(terms[3]);
                        string pol = match.Groups["pol"].Value.ToUpper();
                        if (pol == "POS")
                            pe.polarity = Polarity.POS;
                        else
                            pe.polarity = Polarity.NEG;
                    }

                    //sec_time_rel
                    if (terms.Length > 4)
                    {
                        Regex secRegex = new Regex(@"""(?<sec>.+)""$");
                        Match match = secRegex.Match(terms[4]);
                        string sec = match.Groups["sec"].Value.ToUpper();
                        pe.sec_time_rel = sec;
                    }

                    ret = pe;
                    ret.type = con.ToUpper();

                }

                else
                {
                    ret = new PMTEntity();
                    ret.type = con;
                }

                Regex textregex = new Regex(@"^[A-Za-z0-9]+\=""(?<text>.+)"" [0-9]+\:[0-9]+ [0-9]+\:[0-9]+$");
                Match matchtext = textregex.Match(terms[0]);
                ret.text = matchtext.Groups["text"].Value;

                Regex posregex = new Regex(@"(?<sline>[0-9]+)[:](?<scol>[0-9]+) (?<eline>[0-9]+)[:](?<ecol>[0-9]+)");

                Match matchpos = posregex.Match(terms[0]);

                int sline = Convert.ToInt32(matchpos.Groups["sline"].Value);
                int scol = Convert.ToInt32(matchpos.Groups["scol"].Value);

               /* if (i2b2str.StartsWith("TIMEX3") || i2b2str.StartsWith("EVENT"))
                    sline++;*/

                ret.startLoc = new TextIdentifier(filename, sline, scol);
                int eline = Convert.ToInt32(matchpos.Groups["eline"].Value);
                int ecol = Convert.ToInt32(matchpos.Groups["ecol"].Value);

              /*  if (i2b2str.StartsWith("TIMEX3") || i2b2str.StartsWith("EVENT"))
                    eline++;*/

                ret.endLoc = new TextIdentifier(filename, eline, ecol);

            }

            if (ret == null)
            {
                ret = null;
            }

            return ret;

        }

        public static void ExportConcept(string filename, IEnumerable<Entity> entities, bool StemOnly)
        {
            List<string> a = new List<string>();
            foreach (Entity entity in entities)
                a.Add(entity.ToI2b2Format(StemOnly));
            File.WriteAllLines(filename, a.ToArray());
        }

        public static Entity[] ImportConcept(string filename, string orig_suffix, string obj_suffix)
        {
            List<Entity> ret = new List<Entity>();
            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                Entity entity;
                if (orig_suffix == "")
                    entity = I2b2formToEntity(line, filename);
                else
                    entity = I2b2formToEntity(line, filename.Replace(orig_suffix, obj_suffix));
                ret.Add(entity);
            }
            return ret.ToArray();
        }

        private static TimePoint AnnotateToTimePoint(string str)
        {
            if (str == "" || str == "YMDHMS")
                return null;
            Regex pointRegex = new Regex(@"(?<year>[0-9]*)Y(?<month>[0-9]*)M(?<day>[0-9]*)D(?<hour>[0-9]*)H(?<minute>[0-9]*)M(?<second>[0-9]*)S");
            Match match = pointRegex.Match(str);
            if (match == null)
                return null;
            TimePoint ret = new TimePoint();
            string[] names = { "year", "month", "day", "hour", "minute", "second" };
            for (int i = 0; i < names.Length; i++)
            {
                string val = match.Groups[names[i]].Value;
                if (val != "")
                {
                    switch (i)
                    {
                        case 0: ret.setYear(Convert.ToInt32(val)); break;
                        case 1: ret.setMonth(Convert.ToInt32(val)); break;
                        case 2: ret.setDay(Convert.ToInt32(val)); break;
                        case 3: ret.setHour(Convert.ToInt32(val)); break;
                        case 4: ret.setMinute(Convert.ToInt32(val)); break;
                        case 5: ret.setSecond(Convert.ToInt32(val)); break;
                    }
                }
            }
            return ret;
        }

        private static TimePeriod AnnotateToTimePeriod(string str)
        {
            int p1 = str.IndexOf("{");
            if (p1 == -1) return new TimePeriod();
            int p2 = str.IndexOf("}", p1 + 1);
            string expr1 = str.Substring(p1 + 1, p2 - p1 - 1);
            TimePoint point1 = AnnotateToTimePoint(expr1);
            if (point1 == null) point1 = new TimePoint();

            p1 = str.IndexOf("{", p2 + 1);
            if (p1 == -1)
                return new TimePeriod(point1, point1);
            p2 = str.IndexOf("}", p1 + 1);
            string expr2 = (p2 == -1)? "" : str.Substring(p1 + 1, p2 - p1 - 1);
            TimePoint point2 = AnnotateToTimePoint(expr2);
            if (point2 == null)
                return new TimePeriod(point1, point1);
            else
                return new TimePeriod(point1, point2);
        }

        public static Entity AnnotateToEntity(string str)
        {

            Regex timeRegex = new Regex(@"``(?<text>[^`]+(?:`[^`]+)*)``(?<normalize>[^\s]*)");
            Match match = timeRegex.Match(str);
            if (match != null)
            {
                //A time entity
                Entity entity = new TimeEntity();
                entity.text = match.Groups["text"].Value;
                entity.text = TimeExtractor.util.StringUtil.strip(entity.text);
                entity.type = "time";
                entity.timePeriod = AnnotateToTimePeriod(match.Groups["normalize"].Value);
                return entity;
            }

            return null;

        }

    }
}
