using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Annotator.conf;
using Annotator.conf.entity;
using Annotator.util;

using TimeExtractor.units;

namespace Annotator.conf
{
    public class FileRecord
    {

        //constants (mostly suffixes)

        public static string raw_suffix = ".txt";
        public static string event_con_suffix = ".event-con";
        public static string event_con_result_suffix = ".event-con-result";
        public static string time_con_suffix = ".time-con";
        public static string time_con_result_suffix = ".time-con-result";
        public static string section_suffix = ".section";
        public static string keytp_suffix = ".keytp";
        public static string keytp_result_suffix = ".keytp-result";
        public static string section_entity_suffix = ".sec-con";
        public static string section_entity_result_suffix = ".sec-con-result";
        public static string substitute_pmt_suffix = ".subpmt";
        public static string tlink_suffix = ".tlink";
        public static string tlink_suffix_result = ".tlink-result";
        public static string tlink_revise_suffix = ".ttlink";
        public static string tlink_change_record_suffix = ".tlink-change";
        public static string event_con_wiki_sufiix = ".con.wiki";

        //file name

        private string filename; //no suffix

        public string Filename
        {
            get { return filename; }
        }

        //words

        public List<List<Word>> words;
        public List<string> texts;

        //entities

        public List<Entity> entities;

        //tlinks

        public List<EntityLink> links;
        
        //special entities

        public Entity OpEntity; //operation entity (if there are multiple operations, reference the first one)
        public Entity AdEntity; //admission entity
        public Entity DcEntity; //discharge entity
        public Entity TrEntity; //transfer entity

        //Ad timepoints

        public TimePoint ad_tp;
        public TimePoint dc_tp;

        //sections

        public List<string> sections;

        //methods

        public FileRecord(string name)
        {
            filename = name;
        }

        private void RemoveDuplicateEntities()
        {
            List<Entity> new_entities = new List<Entity>();
            for (int i = 0; i < entities.Count; i++)
            {
                bool flag = true;
                for(int j = 0; j < i; j++)
                    if (i != j && entities[i].startLoc.line == entities[j].startLoc.line &&
                        entities[j].startLoc.col <= entities[i].startLoc.col && entities[i].endLoc.col <= entities[j].endLoc.col)
                    {
                        flag = false;
                        break;
                    }
                if (flag) new_entities.Add(entities[i]);
            }
            entities = new_entities;
        }

        private TimePoint get_tp_from_3digit(int d1, int d2, int d3)
        {
            if (d1 >= 1000)
            {
                return new TimePoint(d1, d2, d3);
            }
            if (d3 >= 1000)
            {
                return new TimePoint(d3, d1, d2);
            }
            if (d1 > 12 || d2 > 31)
            {
                //yy-mm-dd
                if (d1 >= 50) d1 += 1900; else d1 += 2000;
                return new TimePoint(d1, d2, d3);
            }
            {
                //mm-dd-yy
                if (d3 >= 50) d3 += 1900; else d3 += 2000;
                return new TimePoint(d3, d1, d2);
            }
        }

        private string append_zero(int x, int digit)
        {
            string ret = Convert.ToString(x);
            while (ret.Length < digit)
                ret = "0" + ret;
            return ret;
        }

        public virtual void Load()
        {

            string[] lines;

            Console.WriteLine("Loading " + Filename + " ...");
            string[] wikifile = File.ReadAllLines(@".\" + Filename + ".wiki");

            //Load words
            lines = File.ReadAllLines(filename + raw_suffix);
            words = new List<List<Word>>();
            texts = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                texts.Add(lines[i]);
                string[] terms = lines[i].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                List<Word> lst = new List<Word>();
                for (int j = 0; j < terms.Length; j++)
                {
                    if (terms[j] != ";" && terms[j].Contains(";") )
                        terms[j].Replace(";", "");
                    if (terms[j] != ":" && terms[j].Contains(":"))
                        terms[j].Replace(":", "");
                    Word word = new Word(terms[j], new TimeExtractor.units.TextIdentifier(Filename, i + 1, j));
                    word.Pos = j;
                    for (int c = 0; c < wikifile.Count(); c++)
                    {
                        if (wikifile[c].Substring(0, wikifile[c].IndexOf("|")).Contains(terms[j].ToLower())&&wikifile[c].Length > (wikifile[c].LastIndexOf('|') +1))
                            word.Wiki = (wikifile[c].Substring(wikifile[c].LastIndexOf('|') + 1)).Split(new char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    }

                        lst.Add(word);
                }
                words.Add(lst);
            }

            //Load time entities (including normalization)

            entities = new List<Entity>();

            if (File.Exists(filename + time_con_suffix))
            {
                lines = File.ReadAllLines(filename + time_con_suffix);
                foreach (string line in lines)
                {
                    Entity entity = EntityUtil.I2b2formToEntity(line, filename);
                    entity.Fr = this;
                    entities.Add(entity);
                }
            }

            //Load PMT entities

            if (File.Exists(filename + event_con_suffix))
            {
                lines = File.ReadAllLines(filename + event_con_suffix);
                List<Entity> e = new List<Entity>();
                foreach (string line in lines)
                {
                    Entity entity = EntityUtil.I2b2formToEntity(line, filename);
                    entity.Fr = this;
                    entities.Add(entity);
                }
                    
            }

            

            //remove duplicate entities
            RemoveDuplicateEntities();

            //Load TLinks
            if (File.Exists(filename + tlink_suffix))
            {
                lines = File.ReadAllLines(filename + tlink_suffix);
                links = new List<EntityLink>();
                foreach (string line in lines)
                {
                    EntityLink link = EntityUtil.I2b2formToTLink(line, filename, this);
                    if (link != null) links.Add(link);
                }
            }

            //Load Section entities

            if (File.Exists(filename + section_entity_suffix))
            {
                ad_tp = new TimePoint();
                dc_tp = new TimePoint();
                lines = File.ReadAllLines(filename + section_entity_suffix);
                List<Entity> e = new List<Entity>();
                int i = 0;
                foreach (string line in lines)
                {
                    Entity entity = EntityUtil.I2b2formToEntity(line, filename);
                    entity.Fr = this;
                    entity.is_section = true;
                    entities.Add(entity);
                    if (i == 0) ad_tp = entity.getFirstTimePoint();
                    if (i == 1) dc_tp = entity.getFirstTimePoint();
                    i++;
                }
            }
            else
            {
                //very naive way to extract the admission date and discharge date
                ad_tp = new TimePoint();
                dc_tp = new TimePoint();
                int adline = -1;
                int dcline = -1;
                Regex regex = new Regex(@"^(?<d1>[0-9]+)(\\|\/|\-)(?<d2>[0-9]+)(\\|\/|\-)(?<d3>[0-9]+)$");
                Regex regex2 = new Regex(@"^(?<d1>[0-9]{4})(?<d2>[0-9]{2})(?<d3>[0-9]{2})$");
                for(int i = 0; i < words.Count; i++)
                    if (i > 0 && (texts[i - 1].ToLower().Replace(" ", "") == "admissiondate:" ||
                        texts[i - 1].ToLower().Replace(" ", "") == "registrationdate:" ||
                        texts[i - 1].ToLower().Replace(" ", "") == "dischargedate:"))
                    {
                        for (int j = 0; j < words[i].Count; j++)
                        {
                            if (j > 0) break;
                            string s = words[i][j].WordText;
                            if (regex.IsMatch(s))
                            {
                                Match match = regex.Match(s);
                                int d1 = Convert.ToInt32(match.Groups["d1"].Value);
                                int d2 = Convert.ToInt32(match.Groups["d2"].Value);
                                int d3 = Convert.ToInt32(match.Groups["d3"].Value);
                                if (texts[i - 1].ToLower().Replace(" ", "") == "admissiondate:" ||
                        texts[i - 1].ToLower().Replace(" ", "") == "registrationdate:") 
                                {
                                    ad_tp = get_tp_from_3digit(d1, d2, d3);
                                    adline = i;
                                }
                                else 
                                {
                                    dc_tp = get_tp_from_3digit(d1, d2, d3);
                                    dcline = i;
                                }
                            }

                            if (regex2.IsMatch(s))
                            {
                                Match match = regex2.Match(s);
                                int d1 = Convert.ToInt32(match.Groups["d1"].Value);
                                int d2 = Convert.ToInt32(match.Groups["d2"].Value);
                                int d3 = Convert.ToInt32(match.Groups["d3"].Value);
                                if (texts[i - 1].ToLower().Replace(" ", "") == "admissiondate:" ||
                        texts[i - 1].ToLower().Replace(" ", "") == "registrationdate:")
                                {
                                    ad_tp = get_tp_from_3digit(d1, d2, d3);
                                    adline = i;
                                }
                                else
                                {
                                    dc_tp = get_tp_from_3digit(d1, d2, d3);
                                    dcline = i;
                                }
                            }

                        }
                    }
                //Store the section time into the section time file
                List<string> seccons = new List<string>();
                if (adline != -1)
                {
                    seccons.Add("SECTIME=\"" + words[adline][0].WordText + "\" " + (adline + 1) + ":0 " + (adline + 1) + ":0||type=\"ADMISSION\"||dvalue=\"" + append_zero(ad_tp.getYear(), 4) + "-" + append_zero(ad_tp.getMonth(), 2) + "-" + append_zero(ad_tp.getDay(), 2) + "\"");                
                }
                if (dcline != -1) 
                {
                    seccons.Add("SECTIME=\"" + words[dcline][0].WordText + "\" " + (dcline + 1) + ":0 " + (dcline + 1) + ":0||type=\"DISCHARGE\"||dvalue=\"" + append_zero(dc_tp.getYear(), 4) + "-" + append_zero(dc_tp.getMonth(), 2) + "-" + append_zero(dc_tp.getDay(), 2) + "\"");    
                }
                File.WriteAllLines(filename + section_entity_suffix, seccons.ToArray());
            }

            //Load Sections

            sections = new List<string>();
            if (File.Exists(filename + section_suffix))
            {
                lines = File.ReadAllLines(filename + section_suffix);
                foreach (string line in lines)
                {
                    int t = line.LastIndexOf("\t");
                    sections.Add(line.Substring(t + 1));
                }
            }
            else
            {
                for (int i = 0; i < texts.Count; i++)
                    sections.Add(texts[i]);
            }

            //Load and generate "virtual" entities (special entities)

            OpEntity = new TimeEntity();
            AdEntity = new TimeEntity();
            DcEntity = new TimeEntity();
            TrEntity = new TimeEntity();

            TimePoint admissionTP = new TimePoint();
            TimePoint dischargeTP = new TimePoint();

            if (File.Exists(Filename + keytp_suffix))
            {
                string text = File.ReadAllText(Filename + keytp_suffix);
                //text = text.Replace(" ", "");
                //text = text.Replace("\t", "");
                Regex adRegex = new Regex(@"admission=(?<year>[0-9]+)\s(?<month>[0-9]+)\s(?<day>[0-9]+)");
                if (adRegex.IsMatch(text))
                {
                    Match match = adRegex.Match(text);
                    admissionTP.setYear(Convert.ToInt32(match.Groups["year"].Value));
                    admissionTP.setMonth(Convert.ToInt32(match.Groups["month"].Value));
                    admissionTP.setDay(Convert.ToInt32(match.Groups["day"].Value));
                }
                Regex dcRegex = new Regex(@"discharge=(?<year>[0-9]+)\s(?<month>[0-9]+)\s(?<day>[0-9]+)");
                if (dcRegex.IsMatch(text))
                {
                    Match match = dcRegex.Match(text);
                    dischargeTP.setYear(Convert.ToInt32(match.Groups["year"].Value));
                    dischargeTP.setMonth(Convert.ToInt32(match.Groups["month"].Value));
                    dischargeTP.setDay(Convert.ToInt32(match.Groups["day"].Value));
                }
            }

            AdEntity.addTimePoint(admissionTP);
            DcEntity.addTimePoint(dischargeTP);

        }

        public virtual void ReloadEntities(string suffix)
        {
            entities = new List<Entity>();
            if (File.Exists(Filename + suffix))
            {
                string[] lines = File.ReadAllLines(Filename + suffix);
                foreach (string line in lines)
                {
                    Entity entity = EntityUtil.I2b2formToEntity(line, Filename);

                    //remove "problem, test, treatment" entities
                    /*if (entity.type == "PROBLEM" || entity.type == "TEST" || entity.type == "TREATMENT")
                        continue;*/

                    entity.Fr = this;
                    entities.Add(entity);
                }
                RemoveDuplicateEntities();
            }
        }

        public virtual void SaveTimeEntities(bool overwrite)
        {

            //Save the time entities
            string con_file = (overwrite) ? filename + time_con_suffix : filename + time_con_result_suffix;

            List<string> a = new List<string>();
            foreach (Entity entity in entities)
                if (entity is TimeEntity && !entity.is_section)
                    a.Add(entity.ToI2b2Format(false));

            File.WriteAllLines(con_file, a.ToArray());

        }

        public virtual void SaveEventEntities(bool overwrite)
        {

            //save the event entities

            string con_file = (overwrite) ? filename + event_con_suffix : filename + event_con_result_suffix;

            List<string> a = new List<string>();
            foreach (Entity entity in entities)
                if (entity.type == "IMPLICIT" || entity.type == "EXPLICIT" || entity.type == "TREATMENT" || entity.type == "OCCURRENCE" ||
                    entity.type == "CLINICAL_DEPT" || entity.type == "EVIDENTIAL")
                    a.Add(entity.ToI2b2Format(false));

            File.WriteAllLines(con_file, a.ToArray());

        }

        public virtual void SaveTLinks(bool overwrite)
        {

            //save the tlinks

            string tlink_file = (overwrite) ? filename + tlink_suffix : filename + tlink_suffix_result;

            List<string> a = new List<string>();
            foreach (EntityLink link in links)
                a.Add(link.ToI2b2Form());

            File.WriteAllLines(tlink_file, a.ToArray());

        }

        public virtual void SaveAsSubstitutePMTEvent()
        {
            StreamWriter sw = new StreamWriter(Filename + substitute_pmt_suffix);
            for (int i = 0; i < words.Count; i++)
            {
                int j = 0;
                while (j < words[i].Count)
                {
                    if (j > 0) sw.Write(" ");
                    //check whether "j" is that starting point of a PMT entity
                    Entity e = null;
                    foreach(Entity entity in entities)
                        if ((entity.type == "IMPLICIT" || entity.type == "EXPLICIT" || entity.type == "TREATMENT" || entity.type == "CLINICAL_DEPT")
                            && entity.startLoc.line == i + 1 && entity.endLoc.line == i + 1 &&
                            entity.startLoc.col == j)
                        {
                            e = entity;
                            break;
                        }
                    if (e != null)
                    {
                        if (e.type == "EXPLICIT")
                            sw.Write("explicit");
                        else if (e.type == "IMPLICIT")
                            sw.Write("implicit");
                        else if (e.type == "TREATMENT")
                            sw.Write("treatment");
                        else
                            sw.Write("hospital");
                        j = e.endLoc.col + 1;
                    }
                    else
                        sw.Write(words[i][j++].WordText);
                }
                sw.WriteLine();
            }
            sw.Close();
        }

        public virtual void Close()
        {
            words = null;
            texts = null;
            entities = null;
        }

        public virtual Entity GetCorrespondEntity(int line, int col)
        {
            foreach (Entity entity in entities)
                if (entity.startLoc.line == line && entity.startLoc.col <= col && col <= entity.endLoc.col)
                    return entity;
            return null;
        }

        public virtual void ClearAllNormalization()
        {
            foreach(Entity entity in entities)
                if (entity is TimeEntity)
                {
                    entity.clearRelation();
                    entity.setTimePoint(TimeExtractor.conf.TimeConstants.DEFAULT_TIME_POINT);
                    TimeEntity te = new TimeEntity();
                    te.duration = new Dictionary<DurationUnit, double>();
                    te.items = new List<Item>();
                    te.mode = TimeEntityMode.NA;
                    te.relation = TLinkType.OVERLAP;
                    te.repeat_times = 0;                    
                }
        }

        public void ClearModeAndPol()
        {
            foreach (Entity entity in entities)
                if (entity is PMTEntity)
                {
                    ((PMTEntity)entity).modality = Modality.FACTUAL;
                    ((PMTEntity)entity).polarity = Polarity.POS;
                }
                else if (entity is TimeEntity)
                {
                    ((TimeEntity)entity).mode = TimeEntityMode.NA;
                }
        }

        private bool is_same_concept(Entity e1, Entity e2)
        {
            return e1.startLoc.line == e2.startLoc.line && e1.startLoc.col == e2.startLoc.col && e1.endLoc.col == e2.endLoc.col;
        }

        private bool get_equivalent_tlink(EntityLink link, ref List<EntityLink> std_links, out TLinkType type)
        {
            Entity target_from = link.from;
            Entity target_to = link.to;
            type = TLinkType.OTHER;
            for (int i = 0; i < std_links.Count; i++)
            {
                Entity std_from = std_links[i].from;
                Entity std_to = std_links[i].to;
                if (is_same_concept(target_from, std_from) && is_same_concept(target_to, std_to))
                {
                    switch (std_links[i].type)
                    {
                        case TLinkType.AFTER: type = TLinkType.AFTER; break;
                        case TLinkType.BEFORE: type = TLinkType.BEFORE; break;
                        case TLinkType.OTHER: type = TLinkType.OTHER; break;
                        case TLinkType.OVERLAP: type = TLinkType.OVERLAP; break;
                    }
                    return true;
                }
                if (is_same_concept(target_from, std_to) && is_same_concept(target_to, std_from))
                {
                    switch (std_links[i].type)
                    {
                        case TLinkType.AFTER: type = TLinkType.BEFORE; break;
                        case TLinkType.BEFORE: type = TLinkType.AFTER; break;
                        case TLinkType.OTHER: type = TLinkType.OTHER; break;
                        case TLinkType.OVERLAP: type = TLinkType.OVERLAP; break;
                    }
                    return true;
                }
            }
            return false;
        }

    }
}
