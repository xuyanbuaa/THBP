using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TimeExtractor.units;
using TimeExtractor.section.I2b2corefutil;

namespace Annotator.conf.entity
{

        public abstract class Entity
        {

            private static List<string> leftstopPhrase = new List<string>();
            private static List<string> rightstopword = new List<string>();
            private static List<string> keepPhrase = new List<string>();
            private static List<string> leftstopword = new List<string>();
            private static List<string> laststopword = new List<string>();
            private static List<string> stopPhrase = new List<string>();
            private static List<string> prep = new List<string>();

            public static void Preprocess()
            {

                string dict_path = @"..\..\..\Dictionary\";
                string filename1 = dict_path + "keep_Prep.txt";
                string filename2 = dict_path + "leftStopPhrase.txt";
                string filename3 = dict_path + "rightStopPhrase.txt";
                string filename4 = dict_path + "leftstopword.txt";
                string filename5 = dict_path + "laststopword.txt";
                string filename6 = dict_path + "Mid.of";
                string filename7 = dict_path + "prep.txt";

                keepPhrase = FileIO.ReadContent(filename1);
                leftstopPhrase = FileIO.ReadContent(filename2);
                rightstopword = FileIO.ReadContent(filename3);
                leftstopword = FileIO.ReadContent(filename4);
                laststopword = FileIO.ReadContent(filename5);
                stopPhrase = FileIO.ReadContent(filename6);
                prep = FileIO.ReadContent(filename7);

            }

            private string text_ = "";
            private string normalized_text_ = "";
            private TextIdentifier startLoc_ = new TextIdentifier();
            private TextIdentifier endLoc_ = new TextIdentifier();
            private string type_ = "";
            private FileRecord fr;

            public bool is_section = false;
            public bool is_relative_tp = false;
            public string id = "";
            public string sec_time_rel = "";

            protected List<TimePoint> timePoints_ = new List<TimePoint>();
            protected Dictionary<Entity, TLinkType> relations_ = new Dictionary<Entity, TLinkType>();

            public List<Item> items;

            public string text
            {
                get { return text_; }
                set { text_ = value;
                ConceptNormalize();
                }
            }

            public string normalized_text
            {
                get { return normalized_text_; }
                set { normalized_text_ = value; }
            }

            public TextIdentifier startLoc
            {
                get { return startLoc_; }
                set { startLoc_ = value; }
            }

            public TextIdentifier endLoc
            {
                get { return endLoc_; }
                set { endLoc_ = value; }
            }

            public string type
            {
                get { return type_; }
                set { type_ = value; }
            }

            public void addTimePoint(TimePoint timePoint)
            {
                timePoints_.Add(timePoint);
            }

            public void setTimePoint(TimePoint timePoint)
            {
                timePoints_ = new List<TimePoint>();
                timePoints_.Add(timePoint);
            }

            public FileRecord Fr
            {
                get { return fr; }
                set { fr = value; }
            }

            public TimePoint getFirstTimePoint()
            {
                if (timePoints_.Count == 0)
                    return new TimePoint();
                else
                {
                    IEnumerable<TimePoint> ie =
                        from tp in timePoints_
                        orderby tp.getYear(), tp.getMonth(), tp.getDay(), tp.getHour(), tp.getMinute(), tp.getSecond() ascending
                        select tp;
                    return ie.First();
                }
            }

            public TimePoint getLastTimePoint()
            {
                if (timePoints_.Count == 0)
                    return new TimePoint();
                else
                {
                    IEnumerable<TimePoint> ie =
                        from tp in timePoints_
                        orderby tp.getYear(), tp.getMonth(), tp.getDay(), tp.getHour(), tp.getMinute(), tp.getSecond() ascending
                        select tp;
                    return ie.Last();
                }
            }

            public TimePeriod timePeriod
            {
                get
                {
                    TimePoint firstTP = getFirstTimePoint();
                    TimePoint lastTP = getLastTimePoint();
                    return new TimePeriod(firstTP, lastTP);
                }
                set
                {
                    TimePeriod tprd = (TimePeriod)value;
                    timePoints_ = new List<TimePoint>();
                    timePoints_.Add(tprd.getFirstTimePoint());
                    timePoints_.Add(tprd.getLastTimePoint());
                }
            }

            public void addRelation(Entity entity, TLinkType trc)
            {
                if (relations_.ContainsKey(entity))
                    relations_[entity] = trc;
                else
                    relations_.Add(entity, trc);
            }

            public void clearRelation()
            {
                relations_ = new Dictionary<Entity, TLinkType>();
            }

            public TLinkType getRelation(Entity entity)
            {
                TLinkType ret = TLinkType.OVERLAP;
                if (relations_.TryGetValue(entity, out ret))
                    return ret;
                else
                    return TLinkType.OVERLAP;
            }

            public void ConceptNormalize()
            {
                
                //foreach (Concept con in conList)
                {
                    string contain = this.text.ToLower();
                    bool bPhrase = false;
                    //shortness of breath
                    for (int k = 0; k < keepPhrase.Count; k++)
                    {
                        if (contain.IndexOf(keepPhrase[k]) > -1)
                        {
                            bPhrase = true;
                        }
                    }

                    if (!bPhrase)
                    {
                        //a course of acyclovir == acyclovir
                        for (int ii = 0; ii < leftstopPhrase.Count; ii++)
                        {
                            string str = leftstopPhrase[ii];
                            int pos = contain.ToLower().IndexOf(str + " ");
                            if (pos == 0)
                            {
                                contain = contain.Substring(pos + str.Length).Trim();
                                break;
                            }
                        }
                        //up to the knees
                        for (int ii = 0; ii < rightstopword.Count; ii++)
                        {
                            string str = rightstopword[ii];
                            int pos = contain.ToLower().IndexOf(" " + str);
                            if ((pos != -1) && (pos + str.Length + 1 == contain.Length))
                            {
                                contain = contain.Substring(0, pos).Trim();
                                break;
                            }
                        }
                    }
                    //a an
                    bool find = true;
                    while (find)
                    {
                        find = false;
                        for (int ii = 0; ii < leftstopword.Count; ii++)
                        {
                            string str = leftstopword[ii];
                            int pos = contain.ToLower().IndexOf(str + " ");
                            if (pos == 0)
                            {
                                contain = contain.Substring(pos + str.Length).Trim();
                                find = true;
                                break;
                            }
                        }
                    }
                    // powder
                    for (int ii = 0; ii < laststopword.Count; ii++)
                    {
                        string str = laststopword[ii];
                        int pos = contain.ToLower().IndexOf(" " + str);
                        if ((pos != -1) && (pos + str.Length + 1 == contain.Length))
                        {
                            contain = contain.Substring(0, pos).Trim();
                            break;
                        }
                    }
                    //XXX unit of  YYY == YYY
                    if (!bPhrase)
                    {
                        for (int ii = 0; ii < stopPhrase.Count; ii++)
                        {
                            string str = stopPhrase[ii];
                            int pos = contain.ToLower().IndexOf(str + " ");
                            if (pos > -1)
                            {
                                contain = contain.Substring(pos + str.Length).Trim();
                            }
                        }

                        // A XX of YY == A XX
                        bool bflagPrep = false;
                        for (int ii = 0; ii < prep.Count && !bflagPrep; ii++)
                        {
                            string[] sG = contain.Split(' ');
                            if (sG.Contains(prep[ii]))
                            {
                                for (int kk = 1; kk < sG.Length; kk++)
                                {
                                    if (prep[ii] == sG[kk])
                                    {
                                        string sNew = null;
                                        for (int jj = 0; jj < kk; jj++)
                                        {
                                            sNew = sNew + " " + sG[jj];
                                        }
                                        contain = sNew.Trim();
                                        bflagPrep = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    contain = RemoveNoise(contain);
                    this.normalized_text = contain;
                    /*this.probaseSynonym.Add(contain);
                    this.needleSeekSynonym.Add(contain);
                    con.lsWikiAnchor.Add(contain);
                    con.lsWikiBoldName.Add(contain);
                    count++;*/
                }
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

            public override string ToString()
            {
                return text;
            }

            public abstract string ToI2b2Format(bool StemOnly);
            public abstract string ToAnnotateFormat(bool StemOnly);

        }

}
