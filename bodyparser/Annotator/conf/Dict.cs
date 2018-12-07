using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Annotator.conf
{
    public class Dict
    {

        private Dictionary<string, HashSet<string>> dict = new Dictionary<string, HashSet<string>>();
        private HashSet<string> all_dict = new HashSet<string>();
        private Dictionary<string, string> dict_abb = new Dictionary<string, string>();

        public virtual void Load(string file)
        {
            string[] lines = File.ReadAllLines(file);
            dict = new Dictionary<string, HashSet<string>>();
            all_dict = new HashSet<string>();
            foreach (string line in lines)
            {
                string[] terms = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (terms.Length <= 0) continue;
                string name = terms[0];
                HashSet<string> set = new HashSet<string>();
                for (int i = 0; i < terms.Length; i++)
                {
                    if (Regex.IsMatch(terms[i], @"^[0-9\.]+$")) continue;
                    if (!set.Contains(terms[i].ToLower()))
                        set.Add(terms[i].ToLower());
                    if (!all_dict.Contains(terms[i].ToLower()))
                        all_dict.Add(terms[i].ToLower());
                }
                if (!dict.ContainsKey(name))
                    dict.Add(name, set);
            }
        }

        public virtual void Load(string file, Dict outdict)
        {
            string[] lines = File.ReadAllLines(file);
            dict = new Dictionary<string, HashSet<string>>();
            all_dict = new HashSet<string>();
            foreach (string line in lines)
            {
                string[] terms = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (terms.Length <= 0) continue;
                string name = terms[0];
                HashSet<string> set = new HashSet<string>();
                for (int i = 0; i < terms.Length; i++)
                {
                    if (Regex.IsMatch(terms[i], @"^[0-9\.]+$")) continue;
                    if (!set.Contains(terms[i].ToLower()))
                        set.Add(terms[i].ToLower());
                    if (!all_dict.Contains(terms[i].ToLower()))
                        all_dict.Add(terms[i].ToLower());
                }

                if (outdict.dict.ContainsKey(name))
                {
                    foreach (string val in outdict.dict[name])
                    {
                        if (!set.Contains(val))
                            set.Add(val);
                        if (!all_dict.Contains(val))
                            all_dict.Add(val);
                    }
                }

                dict.Add(name, set);
            }
        }

        public virtual void Load_abb(string file)
        {
            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                string[] s = line.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                string key = s[0].ToLower();
                string value = s[1].ToLower();
                dict_abb.Add(key, value);
            }
        }

        public virtual string[] GetAllValue()
        {
            return all_dict.ToArray();
        }

        public virtual string[] GetAllValue(string name)
        {
            if (dict.ContainsKey(name))
                return dict[name].ToArray();
            else
                return new string[0];
        }

        public virtual bool IsContain(string s)
        {
            return all_dict.Contains(s);
        }

        public virtual bool IsContain(string s, string name)
        {
            if (!dict.ContainsKey(name))
                return false;
            else
                return dict[name].Contains(s);
        }

        public virtual string IsCountain_abb(string s)
        {
            string full = "";
            if (dict_abb.ContainsKey(s))
            {
                full = dict_abb[s];
            }
            return full;
        }


        public virtual int CountCates()
        {
            return dict.Count;
        }

        public virtual int CountAll()
        {
            return all_dict.Count;
        }

    }

    public class DictVariables
    {

        public static Dict TimeUnitDict = new Dict();
        public static Dict MonthExprDict = new Dict();
        public static Dict WeekExprDict = new Dict();
        public static Dict NoonsDict = new Dict();
        public static Dict ApmDict = new Dict();

        public static Dict PrepTimeDict = new Dict();

        public static Dict KeyTimePointDict = new Dict();
        public static Dict KeySectionDict = new Dict();

        public static Dict TimeAdverbDict = new Dict();

        public static Dict DosageDict = new Dict();

        public static Dict DepartmentDict = new Dict();
        public static Dict DoctornameDict = new Dict();

        public static Dict OccurrenceDict = new Dict();
        public static Dict OccurrenceDict_verb = new Dict();
        public static Dict EvidentialDict = new Dict();
        public static Dict EvidentialDict_verb = new Dict();

        public static Dict AnatomyDict_concept = new Dict();
        public static Dict AnatomyDict_mtrees = new Dict();
        public static Dict AnatomyDict_RedLex = new Dict();
        public static Dict AnatomyDict_partslist = new Dict();
        public static Dict AnatomyDict = new Dict();
        public static Dict AnatomyDictPart = new Dict();

        public static Dict PositionDict = new Dict();
        public static Dict AbbreviationDict = new Dict();
        public static Dict WikiCutDict = new Dict();
        public static Dict WikiAppearDict = new Dict();

        public static void LoadDictionary()
        {

            TimeUnitDict.Load(AnnotatorFileName.dict_timeunit_file);
            MonthExprDict.Load(AnnotatorFileName.dict_month_file);
            WeekExprDict.Load(AnnotatorFileName.dict_weekday_file);
            NoonsDict.Load(AnnotatorFileName.dict_noons);
            ApmDict.Load(AnnotatorFileName.dict_apm_file);

            PrepTimeDict.Load(AnnotatorFileName.dict_prep_time);

            KeyTimePointDict.Load(AnnotatorFileName.dict_key_timepoint);
            KeySectionDict.Load(AnnotatorFileName.dict_key_section);

            TimeAdverbDict.Load(AnnotatorFileName.dict_time_adverb);

            DosageDict.Load(AnnotatorFileName.dict_dosage_file);

            DepartmentDict.Load(AnnotatorFileName.dict_department);
            DoctornameDict.Load(AnnotatorFileName.dict_doctorname);

            OccurrenceDict.Load(AnnotatorFileName.dict_occurrence);
            OccurrenceDict_verb.Load(AnnotatorFileName.dict_occurrence_verb);
            EvidentialDict.Load(AnnotatorFileName.dict_evidential);
            EvidentialDict_verb.Load(AnnotatorFileName.dict_evidential_verb);

            AnatomyDict_concept.Load(AnnotatorFileName.dict_concept);
            AnatomyDict_mtrees.Load(AnnotatorFileName.dict_mtrees);
            AnatomyDict_RedLex.Load(AnnotatorFileName.dict_RedLex);
            AnatomyDict_partslist.Load(AnnotatorFileName.dict_partslist);
            AnatomyDict.Load(AnnotatorFileName.dict_anatomy);
            PositionDict.Load(AnnotatorFileName.dict_position);
            AnatomyDictPart.Load(AnnotatorFileName.dict_anatmypart);
            AbbreviationDict.Load_abb(AnnotatorFileName.dict_abbreviation);
            WikiCutDict.Load(AnnotatorFileName.dict_wikicut);
            WikiAppearDict.Load(AnnotatorFileName.dict_appearwords);
        }

    }

}
