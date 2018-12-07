using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using WikiAmbigNew;
using i2b2Relationship.Morph;
using Concept;


namespace Concept
{
    public class Entity
    {
        public string name;
        string line;
        string colum_start;
        string colum_end;
        string normalization;
        string type;
        public string wiki;
        string score;
        string normalization2;
        public List<string> position = new List<string>();
        public string withoutposition;
        public string standard;
        public List<string> AnatomyInWiki = new List<string>();

        public static List<List<Entity>> llentity = new List<List<Entity>>();
        public static Dictionary<string, List<string>> positionword = new Dictionary<string, List<string>>();
        public static List<string> all_positionword = new List<string>();


        public static void AnaEntity(string file)
        {
            List<List<string>> llstandard = IO.ReadContent_LL(file, ".xml.con.wiki");
            for (int i = 0; i < llstandard.Count(); i++)
            {
                List<Entity> lentity = new List<Entity>();
                foreach (string s in llstandard[i])
                {
                    Entity entity = new Entity();
                    Regex reg = new Regex(@"^c=(?<name>.+)\s(?<line>[0-9]+):(?<col_start>[0-9]+)\s(?<line>[0-9]+):(?<col_end>[0-9]+).+=(?<normalization>.+)..searchword=(?<searchword>.+)..t=(?<type>.+)..wiki=(?<wiki>.+)$");
                    Match match = reg.Match(s);
                    entity.name = match.Groups["name"].Value.Replace("\"", "");
                    entity.line = match.Groups["line"].Value;
                    entity.colum_start = match.Groups["col_start"].Value;
                    entity.colum_end = match.Groups["col_end"].Value;
                    entity.normalization = match.Groups["normalization"].Value.Replace("\"", "");
                    entity.withoutposition = match.Groups["searchword"].Value.Replace("\"", "");
                    entity.type = match.Groups["type"].Value.Replace("\"", "");
                    entity.wiki = match.Groups["wiki"].Value.Replace("\"", "");
                    entity.score = "0";
                    entity.normalization2 = "";

                    lentity.Add(entity);

                }
                llentity.Add(lentity);
            }
        }

        public static void Process()
        {
            List<Entity> lentity = new List<Entity>();
            int wikifind = 0;

            for (int i = 0; i < llentity.Count(); i++)
            {
                lentity = llentity[i];
                foreach (Entity entity in lentity)
                {
                    string nosense;
                    entity.standard = entity.name;
                   
                    //Position.CheckPosition(entity.name, out entity.position, out nosense);//(4)位置词
                    //entity.standard = Standarize(entity);//withoutpositon + position word（4）位置词


                    //Stemming ss = new Stemming();//在没有位置词处理时，对wiki内容stem——>测试代码
                    //string[] words = entity.name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //List<string> wordss = new List<string>();
                    //foreach (string word in words)
                    //{
                    //    string stemword = ss.Stem(word);
                    //    wordss.Add(stemword);
                    //}
                    //StringBuilder sbb = new StringBuilder();
                    //foreach (string word in wordss)
                    //{
                    //    if (sbb == null)
                    //        sbb.Append(word);
                    //    else
                    //    {
                    //        sbb.Append("");
                    //        sbb.Append(word);
                    //    }
                    //}
                    //entity.standard = sbb.ToString();//测试代码结束

                    if (entity.standard == "" || entity.standard == null)//for test
                        entity.standard = "";////(1)


                    Match(entity, i);
                    if(entity.normalization2=="" && entity.wiki!="")
                    {
                        wikifind++;
                    }
                    entity.score = GetScore(entity);
                }
            }
            Console.WriteLine(wikifind);
            Score(llentity);
            Error(llentity);
            OutAnatomyWiki();
            StreamWriter swk = new StreamWriter(@".\fuck.txt");
            if (Tree.IsContain1("urinary"))
            {
                swk.WriteLine("fuck");
            }
            else
            {
                swk.WriteLine("shit");
            }
            swk.Close();
        }


        public static void Match(Entity entity, int i)
        {
            string[] wiki = entity.wiki.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (Tree.IsContain(entity.standard))
                entity.normalization2 = GetKey(entity.standard.ToLower(), Tree.tree);

            else if (Tree.IsContain(entity.withoutposition))////(4)位置词 
                entity.normalization2 = entity.withoutposition;
          

        

            //else if (entity.wiki.Count() != 0)//only for testing when no wiki score
            //{
            //    foreach (string wikiword in wiki)
            //    {
            //        if (Tree.IsContain(wikiword))
            //        {
            //            entity.normalization2 = GetKey(wikiword, Tree.tree);
            //            break;
            //        }
            //    }
            //}         //baseline 



           //else
           //{
            if (wiki.Count() == 0 && (Entity.correference(entity, i).Count() != 0))
                entity.wiki = Entity.correference(entity, i);//（3）缩写

            
            if (entity.wiki.Count() != 0)//(6)评分
            {

                string sa = WikiAnatomyScore(wiki);

                string a = GetKey(sa, Tree.tree);
                a = Position.AddPosition(a, entity);
                entity.normalization2 = a;

            }

            else if (entity.normalization2 == "" && Concept.all_abbreviation.Contains(entity.standard.ToLower()))
            {
                entity.normalization2 = GetValue(entity.standard, Concept.abbreviation, true);
            }//（3）缩写

            if (entity.normalization2 == "")
                entity.normalization2 = entity.standard;

           //}//(1)
            
        }


        public static string GetScore(Entity entity)
        {
            string[] s = entity.normalization.Split(new char[] { '|' });
            string score = "0";
            foreach (string ss in s)
            {
                if (ss == entity.normalization2)
                {
                    score = "1";
                    break;
                }
            }
            return score;
        }


        public static void Score(List<List<Entity>> llentity)
        {
            float right = 0;
            float wrong = 0;
            float total = 0;
            float im_right = 0;
            float im_wrong = 0;
            float ex_right = 0;
            float ex_wrong = 0;
            float im_total = 0;
            float ex_total = 0;

            int hh = 0;
            for (int i = 0; i < llentity.Count(); i++)
            {
               
                foreach (Entity entity in llentity[i])
                {
                    

                    total++;
                    if (entity.score == "0")
                        wrong++;
                    else right++;
                    hh++;
                    
                    if (entity.type == "implicit")
                    {
                        im_total++;
                        if (entity.score == "0")
                            im_wrong++;
                        else im_right++;
                    }

                    if (entity.type == "explicit")
                    {
                        ex_total++;
                        if (entity.score == "0")
                            ex_wrong++;
                        else ex_right++;
                    }

                }
            }

            string precision = (right / total).ToString();
            string im_precision = (im_right / im_total).ToString();
            string ex_precesion = (ex_right / ex_total).ToString();
            string[] s = { "right = " + right.ToString(), "wrong = " + wrong.ToString(), "total = " + total.ToString(), "precision = " + precision, "explicit right = " + ex_right.ToString(), "explicit wrong = " + ex_wrong.ToString(), "explicit total = " + ex_total.ToString(), "explicit precision = " + ex_precesion, "implicit right = " + im_right.ToString(), "implicit wrong = " + im_wrong.ToString(), "implicit total = " + im_total.ToString(), "implicit precision = " + im_precision};
            
            StreamWriter sw = new StreamWriter(@".\result.txt");
            foreach (string ss in s)
                sw.WriteLine(ss);
            sw.Close();  
        }

        public static void Error(List<List<Entity>> llentity)
        {
            StreamWriter sww = new StreamWriter(@".\concet_normalization.txt");

            StreamWriter sw = new StreamWriter(@".\Error.txt");
            for (int i = 0; i < llentity.Count(); i++)
            {
                foreach (Entity entity in llentity[i])
                {
                    if (entity.score == "0")
                    {
                        string s1 = entity.name + " file" + (i + 1).ToString() + " " + entity.line + ":" + entity.colum_start + " " + entity.line + ":" + entity.colum_end + " " + entity.type.ToUpper();
                        string s2 = "normalization = " + entity.normalization + "; " + "normalization2 = " + entity.normalization2;
                        string s3 = "                                                                                                                    ";
                        string s4 = entity.wiki;
                        string s5 = "====================================================================================================================";
                        sw.WriteLine(s1);
                        sw.WriteLine(s2);
                        sw.WriteLine(s3);
                        sw.WriteLine(s4);
                        sw.WriteLine(s5);
                    }
                    sww.WriteLine(entity.normalization);
                }
            }
            sw.Close();
            sww.Close();
            
        }

        public static string Concat(string[] input, int k, int j)
        {
            StringBuilder sb = new StringBuilder();
            for (int l = 0; l < j; l++)
            {
                if (l + k + 1 > input.Count()) return sb.ToString();
                if (l > 0) sb.Append(" ");
                sb.Append(input[l + k]);
            }
            return sb.ToString();
        }


        public static string correference(Entity entity, int i)
        {
            string entity_wiki = "";
            string file = @".\correference\" + (i + 1).ToString() + ".xml.txt.cor";
            List<string> lines = new List<string>();
            lines = IO.Read(file);
            string s = "c=" + "\"" + entity.name + "\"" + " " + entity.line + ":" + entity.colum_start + " " + entity.line + ":" + entity.colum_end;

            foreach (string line in lines)
            {
                bool flag = false;
                string[] cor = line.Split(new string[] {"||"}, StringSplitOptions.RemoveEmptyEntries);
                
                List<string> corr = new List<string>();
                foreach (string ss in cor)
                {
                        Regex reg = new Regex(@"^c=(?<name>.+)\s(?<line>[0-9]+):(?<col_start>[0-9]+)\s(?<line>[0-9]+):(?<col_end>[0-9]+)$");
                        Match match = reg.Match(ss);
                        string name = match.Groups["name"].Value.Replace("\"", "");
                        corr.Add(name);
                }

                if (cor.Contains(s))
                {
                    foreach (string name in corr)
                    {
                        string wiki = Concept.GetWikiAna(name);
                        if (wiki != "")
                        {
                            entity_wiki = wiki;
                            flag = true;
                            break;
                        }
                    }
                }

                if (flag) break;
            }

            return entity_wiki;
        }

        

        public static string Standarize(Entity entity)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();

            if (Tree.IsContain(entity.withoutposition))
                entity.withoutposition = GetKey(entity.withoutposition, Tree.tree);

            for (int i = 0; i < entity.position.Count(); i++) 
            {
                if (i == 0)
                    sb.Append(entity.position[i]);
                else
                {
                    if (entity.position[i] == "bilateral")
                    {
                        sb1.Append(entity.position[i]);
                        sb1.Append(" ");
                        sb1.Append(sb);
                        sb = sb1;
                    }
                    else
                    {
                        sb.Append(" ");
                        sb.Append(entity.position[i]);
                    }
                }

            }
            if (entity.position.Count() != 0)
                sb.Append(" ");
            sb.Append(entity.withoutposition);
            return sb.ToString();
        }

        

        public static string stem(string input)
        {
            string[] terms = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> list = new List<string>();
            foreach (string term in terms)
            {
                Stemming ss = new Stemming();
                string s = ss.Stem(term);
                list.Add(s);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count(); i++)
            {
                if (i != 0)
                    sb.Append(" ");
                sb.Append(list[i]);
            }

            return sb.ToString();
        }

        public static string GetValue(string st, Dictionary<string, HashSet<string>> dict, bool flag1)
        {
            string output = "";
            foreach (var item in dict)
            {
                bool flag = false;
                if (item.Key == st)
                {
                    flag = true;
                    foreach (string s in item.Value)
                    {
                        bool a = false;
                        string[] ss = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int q = 0; q < s.Length; q++)
                        {
                            bool b = false;
                            for (int ii = 5; ii > 0; ii--)
                            {
                                string p = Concat(ss, q, ii);
                                if (Tree.IsContain(p))
                                {
                                    output = p;
                                    b = true;
                                    flag1 = false;
                                    break;
                                }
                            }
                            if (b)
                            {
                                a = true;
                                break;
                            }
                        }
                        if (a) break;
                    }
                }
                if (flag) break;
            }

            return output;
        }

        public static string GetKey(string st, Dictionary<string, HashSet<string>> dict)
        {
            string output = "";
            foreach (var item in dict)
            {
                if (item.Key == st || item.Value.Contains(st))
                {
                    output = item.Key;
                    break;
                }
            }

            return output;
        }

        public static void OutAnatomyWiki()
        {
            StreamWriter sw = new StreamWriter(@".\AnatomyWiki.txt");
            for (int i = 0; i < llentity.Count(); i++)
            {
                foreach (Entity ent in llentity[i])
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(ent.normalization);
                    sb.Append("||");
                    sb.Append(ent.normalization2);
                    if (ent.AnatomyInWiki.Count() != 0)
                    {
                        for (int j = 0; j < ent.AnatomyInWiki.Count(); j++)
                        {
                            sb.Append("||");
                            sb.Append(ent.AnatomyInWiki[j]);
                        }
                    }
                    sw.WriteLine(sb.ToString());
                }
            }
            sw.Close();
        }

        public static string WikiAnatomyScore(string[] wiki)
        {
            List<string> WikiAnatomyWord = new List<string>();
            List<int> WikiAnatomyDistance = new List<int>();
            string o = "";
            List<float> score = new List<float>();
            List<string> Word = new List<string>();
            List<int> Distance = new List<int>();
            List<float> Times = new List<float>();

            for (int k = 0; k < wiki.Count(); k++)       //using the most frequent word in all wiki content
            {
                for (int j = 5; j > 0; j--)
                {
                    string st = Entity.Concat(wiki, k, j);
                    if (Tree.IsContain(st.ToLower()))
                    {
                        WikiAnatomyWord.Add(st.ToLower());
                        WikiAnatomyDistance.Add(k);
                    }
                }
            }


            for (int p = 0; p < WikiAnatomyWord.Count; p++)
            {
                if (!Word.Contains(WikiAnatomyWord[p]))
                {
                    Word.Add(WikiAnatomyWord[p]);
                    Distance.Add(WikiAnatomyDistance[p]);
                    Times.Add(1);
                }
                else
                {
                    for (int u = 0; u < Word.Count; u++)
                    {
                        if (Word[u] == WikiAnatomyWord[p])
                        {
                            Distance[u] = Distance[u] + WikiAnatomyDistance[p];
                            Times[u] += 1; 
                        }
                    }
                }
            }

            for (int i = 0; i < WikiAnatomyWord.Count(); i++)
            {
                float a = 0;
                float b = 0;
                float percentage = 0.4f;

                int totaldis = 0;
                foreach (int dis in Distance)
                {
                    totaldis += dis;
                }

                int largest = 0;
                foreach (int dis in WikiAnatomyDistance)
                {
                    if (dis > largest)
                        largest = dis;
                }



                for (int j = 0; j < Word.Count(); j++)
                {
                    if (Word[j] == WikiAnatomyWord[i])
                    {
                        a = Times[j];
                        if (i == 0)
                            a = a / percentage;
                        float pi = Convert.ToSingle(Math.PI);
                        b = Convert.ToSingle(Math.Cos(Distance[j] * pi / (totaldis * 2)));
                        float e = Convert.ToSingle(Math.E);
                        float canshu = 0.5f;
                        float x = Distance[j] * 2.0f / largest;
                        //b = Convert.ToSingle((1 / Math.Sqrt(2 * pi * canshu)) * Math.Pow(e, -(Math.Pow(x, 2) / (2 * canshu))));
                        break;
                    }
                }

                float Pi = Convert.ToSingle(Math.PI);
                b = Convert.ToSingle(Math.Cos(WikiAnatomyDistance[i] * Pi / (largest * 2)));
               // b = largest - WikiAnatomyDistance[i];

                score.Add(a+15f*b);
            }

            //for (int i = 0; i < Word.Count; i++)
            //{
            //    a = Times[i];

            //    percentage = 0.4f;
            //    if (i== 0)
            //        a = a / percentage;


            //   totaldis = 0;
            //    foreach (int dis in Distance)
            //    {
            //        totaldis += dis;
            //    }
                
            //    float pi = Convert.ToSingle(Math.PI);
            //     b = Convert.ToSingle(Math.Cos(Distance[i] * pi / (totaldis * 2)));
            //    float x = Convert.ToSingle(Distance[i] * pi / (totaldis * 2));
            //    double y = Math.Cos(3.14);
            //    score.Add(a + b * 10);
            //}


            float q = 0;
            for (int i = 0; i < score.Count; i++)
            {
                string s = WikiAnatomyWord[i];
                if (score[i] > q)
                {
                    q = score[i];
                    o = s;
                }
            }

            return o;

        }

    }
}
