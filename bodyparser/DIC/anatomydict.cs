using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DIC
{
    class anatomydict
    {
        public static void setdict()
        {
            List<string> dict_concept = new List<string>();
            List<string> dict_mtrees = new List<string>();
            List<string> dict_Red = new List<string>();
            List<string> dict_partslist = new List<string>();
            string[] lines;
            string dict_path = @"..\..\..\Dictionary\";
            string rawfile_path = @"..\..\..\AnatomyDic\";
            string[] filenames = Directory.GetFiles(rawfile_path);
            StreamWriter sw_concept = new StreamWriter(dict_path + @"\ADictionary\concept_st.dict");
            StreamWriter sw_mtrees = new StreamWriter(dict_path + @"\ADictionary\mtrees2010.dict");
            StreamWriter sw_Red = new StreamWriter(dict_path + @"\ADictionary\RedLex.dict");
            StreamWriter sw_partslist = new StreamWriter(dict_path + @"\ADictionary\partslist.dict");
            foreach (string filename in filenames)
            {
                lines = File.ReadAllLines(filename);
                if (filename.Contains("st.txt"))
                {
                    for (int i = 0; i < lines.Count(); i++)
                    {
                        string[] terms = lines[i].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        if (terms[1].Contains("bpoc") || terms[1].Contains("tisu") || terms[1].Contains("blor") || terms[1].Contains("bsoj"))
                        {
                            dict_concept.Add(terms[0].ToLower());
                        }
                    }
                }
                else if (filename.Contains(".bin"))
                {
                    for (int i = 0; i < lines.Count(); i++)
                    {
                        string[] terms = lines[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (terms[1].Contains("A01") || terms[1].Contains("A02") || terms[1].Contains("A07") || terms[1].Contains("A14"))
                        {
                            dict_mtrees.Add(terms[0]);
                        }
                        else if (terms[1].Contains("A09") && terms[1].Length <= 15)
                        {
                            dict_mtrees.Add(terms[0]);
                        }
                        else if (terms[1].Contains("A03") && terms[1].Length <= 11)
                        {
                            dict_mtrees.Add(terms[0]);
                        }
                        else if ((terms[1].Contains("A04") || terms[1].Contains("A05") || terms[1].Contains("A12") || terms[1].Contains("A08") || terms[1].Contains("A15")) && terms[1].Length <= 7)
                        {
                            dict_mtrees.Add(terms[0]);
                        }
                        else if ((terms[1].Contains("A06") || terms[1].Contains("A10") || terms[1].Contains("A11") || terms[1].Contains("A13")) && terms[1].Length <= 3)
                        {
                            dict_mtrees.Add(terms[0].ToLower());
                        }
                    }
                }
                else if (filename.Contains(".dic"))
                {
                    foreach (string line in lines)
                    {

                        if (line.Contains("\t"))
                        {
                            string[] terms = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string term in terms)
                                dict_Red.Add(term);
                        }
                        else
                            dict_Red.Add(line.ToLower());
                    }
                }
                else if (filename.Contains("e.txt"))
                {
                    foreach (string line in lines)
                    {
                        string[] terms = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        dict_partslist.Add(terms[1].ToLower());
                    }
                    dict_partslist.RemoveAt(0);
                }
            }
            foreach (string e in dict_concept)
            {
                sw_concept.WriteLine(e);
            }
            foreach (string e in dict_mtrees)
            {
                sw_mtrees.WriteLine(e);
            }
            foreach (string e in dict_Red)
            {
                sw_Red.WriteLine(e);
            }
            foreach (string e in dict_partslist)
            {
                sw_partslist.WriteLine(e);
            }
            sw_concept.Close();
            sw_mtrees.Close();
            sw_Red.Close();
            sw_partslist.Close();
        }

        public static void setpartdict()
        {
            string path = @"..\..\..\Dictionary\ADictionary\AnatomyDict.dict";
            List<string> newdict = new List<string>();
            string[] anatomis = File.ReadAllLines(path);
            foreach (string anatomy in anatomis)
            {
                if (anatomy.LastIndexOf(@" of ") + 4 > 3)
                {
                    string partofanatomy = anatomy.Substring(anatomy.LastIndexOf(@" of ") + 4);
                    if (partofanatomy.IndexOf("the") == 0)
                        partofanatomy = partofanatomy.Substring(4);
                    newdict.Add(partofanatomy);
                    //string partofanatomy_ = anatomy.Substring(0, anatomy.LastIndexOf(@" of "));
                    //if (partofanatomy_.IndexOf("the") == 0)
                    //    partofanatomy = partofanatomy_.Substring(4);
                    //newdict.Add(partofanatomy_);
                }
            }

            StreamWriter sw = new StreamWriter(@"..\..\..\Dictionary\ADictionary\PartOfAD.dict");
            foreach (string s in newdict)
            {
                sw.WriteLine(s);
            }
            sw.Close();
        }
    }
}
