using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Snowball;

namespace WikiParser
{
    public class SRComparer : IComparer<string>
    {
        protected Dictionary<string, int> SR;
        public SRComparer(Dictionary<string, int> SR)
        {
            this.SR = SR;
        }
        public int Compare(string X, string Y)
        {
            return SR[Y].CompareTo(SR[X]);
        }
    }

    public class IntEncoder
    {
        protected const string ENCODE_STRING = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ=@";
        protected static Dictionary<char, int> DECODE_TABLE;

        static IntEncoder()
        {
            DECODE_TABLE = new Dictionary<char, int>(ENCODE_STRING.Length);
            for (int i = 0; i < ENCODE_STRING.Length; i++)
            {
                DECODE_TABLE[ENCODE_STRING[i]] = i;
            }
        }

        public static string Encode(int Val)
        {
            if (Val < 64)
            {
                return ENCODE_STRING.Substring(Val, 1);
            }
            else if (Val < 64 * 64)
            {
                // performance branch, not needed!
                return ENCODE_STRING.Substring(Val >> 6, 1) + ENCODE_STRING.Substring(Val & 0x3f, 1);
            }
            else if (Val < 64 * 64 * 64)
            {
                // performance branch, not needed!
                return
                    ENCODE_STRING.Substring(Val >> 6 * 2, 1) +
                    ENCODE_STRING.Substring((Val >> 6) & 0x3f, 1) +
                    ENCODE_STRING.Substring(Val & 0x3f, 1);
            }
            else
            {
                return Encode(Val >> 6) + Encode(Val & 0x3f);
            }
        }

        public static int Decode(string Val)
        {
            int Res = 0;
            for (int i = 0; i < Val.Length; i++)
            {
                Res = Res << 6;
                Res += DECODE_TABLE[Val[i]];
            }
            return Res;
        }
    }

    public class WikipediaEntry
    {
        public string Name;
        public string[] Category;
        public string[] Link;
        public string[] Redirect;
        public string Text;
        public string Description;

        public Dictionary<string, int> TermFrequency = new Dictionary<string, int>();
        public Dictionary<string, int> TermFrequencyTitle = new Dictionary<string, int>();
        public Dictionary<string, int> TermFrequencyDescription = new Dictionary<string, int>();

        public override string ToString()
        {
            return Name;
        }

        public void WriteToFile(StreamWriter Writer, bool bText=true)
        {
            StringWriter sw = new StringWriter();

            Writer.Write(Name);
            Writer.Write("\t");

            foreach (var title in Redirect)
            {
                sw.Write(title);
                sw.Write(";;");
            }
            Writer.Write(sw.ToString().Trim(';') + '\t');

            sw.Flush();
            foreach (var category in Category)
            {
                sw.Write(category);
                sw.Write(";");
            }
            Writer.Write(sw.ToString().Trim(';') + '\t');

            //bool First = true;
            //foreach (string Term in TermFrequency.Keys)
            //{
            //    if (!First)
            //    {
            //        Writer.Write(",");
            //    }
            //    else
            //    {
            //        First = false;
            //    }
            //    Writer.Write(Term);
            //    Writer.Write(":");
            //    Writer.Write(IntEncoder.Encode(TermFrequency[Term]));
            //}

            WriteVector(Writer, TermFrequencyTitle);
            Writer.Write("\t");
            WriteVector(Writer, TermFrequencyDescription);
            Writer.Write("\t");
            WriteVector(Writer, TermFrequency);

            Writer.Write("\n");
        }

        public void WriteVector(StreamWriter Writer, Dictionary<string, int> TermFrequency)
        {
            bool First = true;
            foreach (string Term in TermFrequency.Keys)
            {
                if (!First)
                {
                    Writer.Write(",");
                }
                else
                {
                    First = false;
                }
                Writer.Write(Term);
                Writer.Write(":");
                Writer.Write(TermFrequency[Term]);
                //Writer.Write(IntEncoder.Encode(TermFrequency[Term]));
            }
        }

        public static Dictionary<string, int> ReadVector(string Line, ref int FieldStart)
        {
            Dictionary<string, int> TermFrequency = new Dictionary<string, int>();

            string Term = "";
            int FrequencyStart = FieldStart;
            int Frequency;
            for (int i = FieldStart; i < Line.Length; i++)
            {
                if (Line[i] == ':')
                {
                    Term = Line.Substring(FieldStart, i - FieldStart);
                    FrequencyStart = i + 1;
                }
                else if (Line[i] == ',')
                {
                    Frequency = IntEncoder.Decode(Line.Substring(FrequencyStart, i - FrequencyStart));
                    TermFrequency[Term] = Frequency;
                    FieldStart = i + 1;
                }
                else if (Line[i] == '\t')
                {
                    FieldStart = i + 1;
                    break;
                }
            }

            return TermFrequency;
        }

        public static WikipediaEntry ReadFromLine(string Line)
        {
            WikipediaEntry Result = new WikipediaEntry();

            // add sentinel at end of line..
            Line += ",";
            int FieldStart = 0;
            for (int i = 0; i < Line.Length; i++)
            {
                if (Line[i] == '\t')
                {
                    Result.Name = Line.Substring(0, i);
                    FieldStart = i + 1;
                    break;
                }
            }

            int akaStart = FieldStart;
            for (int i = akaStart; i < Line.Length; i++)
            {
                if (Line[i] == '\t')
                {
                    Result.Redirect = Line.Substring(akaStart, i - akaStart).Split(';');
                    FieldStart = i + 1;
                    break;
                }
            }

            int CategoryStart = FieldStart;
            for (int i = CategoryStart; i < Line.Length; i++)
            {
                if (Line[i] == '\t')
                {
                    Result.Category = Line.Substring(CategoryStart, i - CategoryStart).Split(';');
                    FieldStart = i + 1;
                    break;
                }
            }
            
            // Title may contain ","
            //if (Result.Name.Contains(',') | Result.Name.Contains(':'))
            //{
            //    Line = Line.Replace(Result.Name, Result.Name.Replace(",", "").Replace(":", ""));
            //}

            //string Term = "";
            //int FrequencyStart = FieldStart;
            //int Frequency;
            //for (int i = FieldStart; i < Line.Length; i++)
            //{
            //    if (Line[i] == ':')
            //    {
            //        Term = Line.Substring(FieldStart, i - FieldStart);
            //        FrequencyStart = i + 1;
            //    }
            //    else if (Line[i] == ',')
            //    {
            //        Frequency = IntEncoder.Decode(Line.Substring(FrequencyStart, i - FrequencyStart));
            //        Result.TermFrequency[Term] = Frequency;
            //        FieldStart = i + 1;
            //    }
            //}

            Result.TermFrequencyTitle = ReadVector(Line, ref FieldStart);
            Result.TermFrequencyDescription = ReadVector(Line, ref FieldStart);
            Result.TermFrequency = ReadVector(Line, ref FieldStart);

            return Result;
        }

    }

    public interface WordFilter
    {
        bool IsWordIgnored(string Word);
    }

    public class WordFilterLocalSearch : WordFilter
    {
        protected static string[] IGNORE_WORDS = new string[]{
			"alternativname",
			"bildbeschreibung",
			"geburtsdatum",
			"geburtsort",
			"personendaten",
			"px",
			"sterbedatum",
			"sterbeort"
		};

        protected static Dictionary<string, bool> IGNORE_WORDS_HASH;

        protected static Regex FourIdenticalConsecutiveChars;
        protected static Regex StartsWithThreeIdenticalChars;

        static WordFilterLocalSearch()
        {
            IGNORE_WORDS_HASH = new Dictionary<string, bool>();
            foreach (string Word in IGNORE_WORDS)
            {
                IGNORE_WORDS_HASH[Word] = true;
            }

            FourIdenticalConsecutiveChars = new Regex(@"(\w)\1\1\1");
            StartsWithThreeIdenticalChars = new Regex(@"^(\w)\1\1");
        }

        public bool IsWordIgnored(string Word)
        {
            string WordLower = Word.ToLower();
            if (IGNORE_WORDS_HASH.ContainsKey(WordLower) ||
                //StartsWithThreeIdenticalChars.IsMatch(WordLower) ||
                FourIdenticalConsecutiveChars.IsMatch(WordLower))
            {
                Console.WriteLine("Ignoring word: " + Word);
                return true;
            }
            foreach (char C in Word)
            {
                // Unicode Character Code
                if (((int)C) > 255)
                {
                    // Word contains non-Latin1 char..
                    Console.WriteLine("Ignoring word: " + Word);
                    return true;
                }
            }
            return false;
        }
    }

    public class WikipediaPageFilter
    {
        protected static Regex RE_ONLY_NUMBERS;
        protected static Regex RE_DATE;
        protected static Regex RE_DATE2;
        protected static Regex RE_DATE3;
        protected static Regex RE_LIST;
        protected static Regex RE_NEKROLOG;

        static WikipediaPageFilter()
        {
            RE_ONLY_NUMBERS = new Regex("^[0-9][0-9]*$");

            //RE_DATE = new Regex(@"^[0-9][0-9]*\.\s*(Januar|Februar|März|April|Mai|Juni|Juli|August|September|Oktober|November|Dezember)$");
            RE_DATE = new Regex(@"^[0-9][0-9]*\.\s*(January|February|March|April|May|June|July|August|September|October|November|December)$");

            // alles vor Christus ignorieren => ignore everything before Christ
            RE_DATE2 = new Regex(@".*[0-9][0-9]*\s*v\.\s*Chr\..*");

            // Jahrhunderte ignorieren => ignore centuries
            //RE_DATE3 = new Regex(@"^[0-9][0-9]*\.\s*Jahrhundert$");
            RE_DATE3 = new Regex(@"^[0-9][0-9]*..\s*century$");

            //RE_LIST = new Regex(@"^Liste (der|von) .*$");
            //RE_LIST = new Regex(@"^List of .*$");

            //RE_NEKROLOG = new Regex(@"^Nekrolog\s*[1-9][0-9][0-9][0-9]$");
            RE_NEKROLOG = new Regex(@"^Deaths\s*[1-9][0-9][0-9][0-9]$");
        }

        public bool IsPageIgnored(string Title, string Text)
        {
            if (Title.IndexOf(":") > -1)
            {
                return true;
            }
            if (RE_ONLY_NUMBERS.IsMatch(Title) ||
                RE_DATE.IsMatch(Title) ||
                RE_DATE2.IsMatch(Title) ||
                RE_DATE3.IsMatch(Title) ||
                //RE_LIST.IsMatch(Title) ||
                RE_NEKROLOG.IsMatch(Title)
                || Title.Contains("Template:")
                || Title.Contains("(disambiguation)")
                )
            {
                return true;
            }
            // ignore persons..
            // see http://en.wikipedia.org/wiki/Special_Pages
            if (
                //Text.Contains("[[Category:America") ||
                Text.Contains("[[Category:Star name disambiguations") ||
                Text.Contains("[[Category:Disambiguation") ||
                Text.Contains("[[Category:Georgia") ||
                Text.Contains("[[Category:Lists of political parties by generic name") ||
                Text.Contains("[[Category:Galaxy name disambiguations") ||
                Text.Contains("[[Category:Lists of two-letter combinations") ||
                Text.Contains("[[Category:Disambiguation categories") ||
                Text.Contains("[[Category:Disambiguation pages") ||
                Text.Contains("[[Category:Towns in Italy (disambiguation)") ||
                Text.Contains("[[Category:Redirects to disambiguation pages") ||
                Text.Contains("[[Category:Birmingham") ||
                Text.Contains("[[Category:Mathematical disambiguation") ||
                Text.Contains("[[Category:Public schools in Montgomery County") ||
                Text.Contains("[[Category:Structured lists") ||
                Text.Contains("[[Category:Identical titles for unrelated songs") ||
                Text.Contains("[[Category:Signpost articles") ||
                Text.Contains("[[Category:Township disambiguation") ||
                Text.Contains("[[Category:County disambiguation") ||
                Text.Contains("[[Category:Disambiguation pages in need of cleanup") ||
                Text.Contains("[[Category:Disambiguation pages") ||
                Text.Contains("[[Category:Human name disambiguation") ||
                Text.Contains("[[Category:Number disambiguations") ||
                Text.Contains("[[Category:Letter and number combinations") ||
                Text.Contains("[[Category:4-letter acronyms") ||
                Text.Contains("[[Category:Acronyms that may need to be disambiguated") ||
                Text.Contains("[[Category:Lists of roads sharing the same title") ||
                Text.Contains("[[Category:List disambiguations") ||
                Text.Contains("[[Category:3-digit Interstate disambiguations") ||
                Text.Contains("[[Category:Geographical locations sharing the same title") ||
                Text.Contains("[[Category:Tropical cyclone disambiguation") ||
                Text.Contains("[[Category:Repeat-word disambiguations") ||
                Text.Contains("[[Category:Song disambiguations") ||
                Text.Contains("[[Category:Disambiguated phrases") ||
                Text.Contains("[[Category:Subway station disambiguations") ||
                Text.Contains("[[Category:Lists of identical but unrelated album titles") ||
                Text.Contains("[[Category:5-letter acronyms") ||
                Text.Contains("[[Category:Three-letter acronym disambiguations") ||
                Text.Contains("[[Category:Miscellaneous disambiguations") ||
                Text.Contains("[[Category:Two-letter acronym disambiguations") ||
                Text.Contains("[[Category:Days") ||
                Text.Contains("[[Category:Eastern Orthodox liturgical days"))
            {
                Console.WriteLine("INFO: Ignoring person or non-relevant category: {0}", Title);
                return true;
            }
            return false;
        }
    }

    public class AttributeVector
    {
        public string Name;
        public Dictionary<int, int> TFIDF = new Dictionary<int, int>();
        public Dictionary<string, int> sTFIDF = new Dictionary<string, int>();
        public Dictionary<string, int> sTF = new Dictionary<string, int>();

        public override string ToString()
        {
            return Name;
        }

        public void SaveToFile(StreamWriter Writer)
        {
            Writer.Write(Name);
            Writer.Write("\t");
            bool First = true;
            foreach (var TermId in sTF.Keys)
            {
                if (!First)
                {
                    Writer.Write(",");
                }
                else
                {
                    First = false;
                }
                Writer.Write(TermId);
                Writer.Write(":");
                Writer.Write(sTF[TermId]);
                //Writer.Write(":");
                //Writer.Write(sTFIDF[TermId]);
            }
            Writer.Write("\n");
        }

        public void WriteToFile(StreamWriter Writer)
        {
            Writer.Write(Name);
            Writer.Write("\t");
            bool First = true;
            foreach (int TermId in TFIDF.Keys)
            {
                if (!First)
                {
                    Writer.Write(",");
                }
                else
                {
                    First = false;
                }
                Writer.Write(IntEncoder.Encode(TermId));
                Writer.Write(":");
                Writer.Write(IntEncoder.Encode(TFIDF[TermId]));
            }
            Writer.Write("\n");
        }

        public static AttributeVector ReadFromLine(string Line)
        {
            AttributeVector Result = new AttributeVector();

            // add sentinel at end of line..
            if (!Line.EndsWith(","))
            {
                Line += ",";
            }
            int FieldStart = 0;
            for (int i = 0; i < Line.Length; i++)
            {
                if (Line[i] == '\t')
                {
                    Result.Name = Line.Substring(0, i);
                    FieldStart = i + 1;
                    break;
                }
            }
            int TermId = 0;
            int FrequencyStart = FieldStart;
            for (int i = FieldStart; i < Line.Length; i++)
            {
                if (Line[i] == ':')
                {
                    TermId = IntEncoder.Decode(Line.Substring(FieldStart, i - FieldStart));
                    FrequencyStart = i + 1;
                }
                else if (Line[i] == ',')
                {
                    try
                    {
                        Result.TFIDF[TermId] = IntEncoder.Decode(Line.Substring(FrequencyStart, i - FrequencyStart));
                    }
                    catch
                    {
                        Console.WriteLine("Error while reading attribute vector from line:");
                        Console.WriteLine(Line);
                        throw;
                    }
                    FieldStart = i + 1;
                }
            }

            return Result;
        }
    }

    class Program
    {
        protected static string[] STOP_WORDS_EN = new string[]{
            //"a",
            //"about",
            //"after",
            //"against",
            //"all",
            //"along",
            //"also",
            //"although",
            //"am",
            //"among",
            //"an",
            //"are",
            //"as",
            //"at",
            //"be",
            //"became",
            //"because",
            //"been",
            //"being",
            //"between",
            //"but",
            //"by",
            //"can",
            //"caution",
            //"come",
            //"could",
            //"details",
            //"did",
            //"does",
            //"doing",
            //"done",
            //"during",
            //"early",
            //"far",
            //"found",
            //"from",
            //"had",
            //"has",
            //"have",
            //"he",
            //"her",
            //"here",
            //"his",
            //"how",
            //"however",
            //"I",
            //"if",
            //"in",
            //"including",
            //"into",
            //"is",
            //"it",
            //"its",
            //"late",
            //"later",
            //"made",
            //"many",
            //"may",
            //"med",
            //"might",
            //"more",
            //"most",
            //"near",
            //"no",
            //"non",
            //"none",
            //"nor",
            //"not",
            //"note",
            //"of",
            //"off",
            //"on",
            //"only",
            //"other",
            //"over",
            //"saw",
            //"see",
            //"seen",
            //"several",
            //"she",
            //"since",
            //"so",
            //"some",
            //"such",
            //"than",
            //"that",
            //"the",
            //"their",
            //"them",
            //"then",
            //"there",
            //"these",
            //"they",
            //"those",
            //"through",
            //"tip",
            //"to",
            //"too",
            //"under",
            //"up",
            //"us",
            //"use",
            //"very",
            //"was",
            //"we",
            //"were",
            //"when",
            //"where",
            //"which",
            //"who",
            //"you"
            };

        protected static string[] STOP_WORDS_GLOBAL = new string[]{
			"cellpadding",
			"cellspacing",
			"bgcolor"
			};

        /* other words to ignore:
         * cellspacing, bgcolor, ffffff, efefef
         * words with four same characters, 
         */

        protected const int MIN_WORD_LENGTH = 2;
        protected const int MAX_WORD_LENGTH = 44;

        protected static Dictionary<string, bool> STOP_WORDS_EN_HASH;
        protected static Dictionary<string, bool> STOP_WORDS_GLOBAL_HASH;

        protected static List<string> FilterTokens(List<string> Tokens, string Lang, WordFilter WordFilt)
        {
            string TokLower;
            List<string> Result = new List<string>(Tokens.Count);

            // Lang can be language code (de, en, fr, ..) or locale (de-DE, en-GB, ..)
            if (Lang.StartsWith("en"))
            {
                char[] ValidChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
                foreach (string Tok in Tokens)
                {
                    TokLower = Tok.ToLower();
                    if (TokLower.IndexOfAny(ValidChars) > -1 &&
                        TokLower.Length >= MIN_WORD_LENGTH &&
                        TokLower.Length <= MAX_WORD_LENGTH &&
                        !STOP_WORDS_EN_HASH.ContainsKey(TokLower) &&
                        !STOP_WORDS_GLOBAL_HASH.ContainsKey(TokLower))
                    {
                        if (WordFilt == null || !WordFilt.IsWordIgnored(Tok))
                        {
                            Result.Add(Tok);
                        }
                    }
                }
                return Result;
            }
            else
            {
                foreach (string Tok in Tokens)
                {
                    TokLower = Tok.ToLower();
                    if (TokLower.Length >= MIN_WORD_LENGTH &&
                        TokLower.Length <= MAX_WORD_LENGTH &&
                        !STOP_WORDS_GLOBAL_HASH.ContainsKey(TokLower))
                    {
                        if (WordFilt == null || !WordFilt.IsWordIgnored(Tok))
                        {
                            Result.Add(Tok);
                        }
                    }
                }
                return Result;
            }
        }

        public static string[] ExtracLinks(string Text)
        {
            string links = "";
            //Regex Rx = new Regex(@"[[See also:");

            return links.Split(';');
        }

        public static string[] ExtracCategory(string Text, Dictionary<string, int> CategoryList=null)
        {
            string category = "";
            Regex Rx = new Regex(@"\[\[Category:([\w]+.*?)\]\]");
            string c;
            foreach (Match m in Rx.Matches(Text))
	        {
                c = m.Groups[1].Value as string;
                c = c.ToLower();
                c.Trim(new char[] { ' ', '_', '|' });
                c = c.Replace(' ', '_');
                
                if (CategoryList.ContainsKey(c))
                    category += c + ";";
	        }
            if (category != "")
            {
                return category.TrimEnd(';').Split(';');
            }
            else
            {
                return null;
            }
        }

        public static string[] ExtracTitle(string Text)
        {
            string title = "";
            Regex Rx = new Regex(@"{{Redirect\|([\w]+.*?)}}");
            foreach (Match m in Rx.Matches(Text))
            {
                string t = m.Groups[1].Value as string;
                if (t == "other uses")
                    break;
                title += t + "|";
            }
            if (title=="")
            {
                return new string[]{};
            }
            else
            {
                return title.TrimEnd('|').Split('|');
            }
        }

        public static string ExtracDescription(string Text)
        {
            string description = "";
            int pos = Text.IndexOf("==");
            if (pos > -1)
            {
                description = Text.Substring(0, pos);
            }
            else
            {
                description = Text;
            }
            return description;
        }

        public static string MediaWikiMarkupToPlainTextDescription(string Text)
        {
            Regex Rx;

            Rx = new Regex(@"<!--.*?-->");
            Text = Rx.Replace(Text, "");
            Rx = new Regex(@"<[bB][rR]\s*/?>");
            Text = Rx.Replace(Text, " ");
            Rx = new Regex(@"\[\[File:.*\]\]");
            Text = Rx.Replace(Text, " ");

            // remove urls
            Rx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
            Text = Rx.Replace(Text, " ");

            Rx = new Regex(@"==[\w\s]*==");
            Text = Rx.Replace(Text, " ");

            Rx = new Regex(@"<ref.*?</ref>", RegexOptions.Singleline);
            Text = Rx.Replace(Text, " ");

            Rx = new Regex(@"{{[^{]*?}}", RegexOptions.Singleline);
            Text = Rx.Replace(Text, " ");

            // recursive once
            Rx = new Regex(@"{{[^{]*?}}", RegexOptions.Singleline);
            Text = Rx.Replace(Text, " ");

            return Text;
        }

        public static string MediaWikiMarkupToPlainText(string Text)
        {
            Regex Rx;

            Rx = new Regex(@"<!--.*?-->");
            Text = Rx.Replace(Text, "");
            Rx = new Regex(@"<ref>.*?</ref>");
            Text = Rx.Replace(Text, "");
            Rx = new Regex(@"<[bB][rR]\s*/?>");
            Text = Rx.Replace(Text, " ");
            Rx = new Regex(@"\[\[[fF]ile:.*\]\]");
            Text = Rx.Replace(Text, " ");

            // remove common templates
            Text = Text.Replace("{{Infobox ", " ");
            Text = Text.Replace("{{Infobox_", " ");
            Text = Text.Replace("{{Vorlage:Infobox ", " ");
            Text = Text.Replace("{{Vorlage:Infobox_", " ");
            Text = Text.Replace("{{Navigationsleiste ", " ");
            Text = Text.Replace("{{Vorlage:Navigationsleiste ", " ");
            Text = Text.Replace("{{DEFAULTSORT:", " ");

            Rx = new Regex(@"==[\w\s]*==");
            Text = Rx.Replace(Text, " ");
            Rx = new Regex(@"{{Cite.*?}}", RegexOptions.Singleline);
            Text = Rx.Replace(Text, " ");
            Rx = new Regex(@"<ref.*?</ref>", RegexOptions.Singleline);
            Text = Rx.Replace(Text, " ");

            // remove urls
            Rx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
            Text = Rx.Replace(Text, " ");

            // remove category prefix
            Text = Text.Replace("[[Category:", "[[");

            // remove redirect prefix
            Text = Text.Replace("{{Redirect", "{{");
            Text = Text.Replace("{{Link", "{{");
            Text = Text.Replace("{{Main", "{{");
            Text = Text.Replace("{{See also", "{{");
            Text = Text.Replace("{{Related articles", "{{");
            Text = Text.Replace("{{about", "{{");

            // remove picture formatting
            Text = Text.Replace("|thumb|", "|");
            Text = Text.Replace("|left|", "|");
            Text = Text.Replace("|right|", "|");

            // remove Weblinks section
            Rx = new Regex(@"==\s*Weblinks\s*==");
            Text = Rx.Replace(Text, " ");

            // remove language links...
            Rx = new Regex(@"\[\[[a-z][a-z][a-z]?:.*?\]\]");
            Text = Rx.Replace(Text, "");
            Rx = new Regex(@"\[\[[^|\]]*?\|([^\]]*?)\]\]");
            Text = Rx.Replace(Text, "$1");
            Rx = new Regex(@"\[\[(.*?)\]\]");
            Text = Rx.Replace(Text, "$1");

            // remove bold italics
            Rx = new Regex(@"'''''([^']*?)'''''");
            Text = Rx.Replace(Text, "$1");
            Rx = new Regex(@"'''([^']*?)'''");
            Text = Rx.Replace(Text, "$1");
            Rx = new Regex(@"''([^']*?)''");
            Text = Rx.Replace(Text, "$1");

            // filter html attributes
            Rx = new Regex(@"(class|align|valign|style|colspan|width|height)=""[^""]*""");
            Text = Rx.Replace(Text, "");

            Text = Text.Replace("http://www.", "");
            Text = Text.Replace("http://", "");
            Text = Text.Replace("https://", "");

            Text = Text.Replace("&nbsp;", " ");
            Text = Text.Replace("&uuml;", "ü");
            Text = Text.Replace("&ouml;", "ö");
            Text = Text.Replace("&auml;", "ä");
            Text = Text.Replace("&Uuml;", "Ü");
            Text = Text.Replace("&Ouml;", "Ö");
            Text = Text.Replace("&Auml;", "Ä");
            Text = Text.Replace("&szlig;", "ß");

            return Text;
        }

        public static List<string> TokenizeNgram(string Text, Dictionary<String, int> Terms)
        {
            if (Char.IsLetter(Text[Text.Length - 1]) | Char.IsDigit(Text[Text.Length - 1]))
            {
                Text = Text + ".";
            }
            List<string> Tokens = new List<string>();

            int TokenStart = 0;
            int NextStart = 0;
            int StrPos = 0;
            int Ngram = 0;

            bool NextSentence = false;
            bool NextNgram = false;

            while (NextStart < Text.Length & StrPos < Text.Length)
            {
                char CurrentChar = Text[StrPos];
                if (!Char.IsLetter(CurrentChar) & !Char.IsDigit(CurrentChar) & CurrentChar!='-')
                {
                    //if (CurrentChar != ' ')
                    //{
                    //    NextSentence = true;
                    //}

                    Ngram++;
                    if (Ngram >= 5 | StrPos == NextStart | StrPos == Text.Length - 1)
                    {
                        NextNgram = true;
                    }

                    if (NextStart == 0)
                    {
                        NextStart = StrPos + 1;
                    }

                    if (CurrentChar != ' ') // not space, reach end of a sentence
                    {
                        if (Ngram == 1) // move to next sentence
                        {
                            NextSentence = true;
                        }
                        else // move to next ngram
                        {
                            NextNgram = true;
                        }
                    }
                    
                    string TokenText = Text.Substring(TokenStart, StrPos - TokenStart);
                    if ((Ngram == 1 & TokenText.Length > 0) | (Ngram > 1 & Terms.ContainsKey(TokenText)))
                    {
                        Tokens.Add(TokenText);
                    }

                    // move to next ngram
                    if ( NextNgram )
                    {
                        NextNgram = false;
                        TokenStart = NextStart;
                        StrPos = TokenStart;
                        NextStart = 0;
                        Ngram = 0;
                        continue;
                    }

                    // move to next sentence
                    if (NextSentence)
                    {
                        NextSentence = false;
                        NextNgram = false;

                        TokenStart = StrPos + 1;
                        StrPos = TokenStart;
                        NextStart = 0;
                        Ngram = 0;
                        continue;
                    }
                }
                StrPos++;
            }
            if (StrPos > TokenStart)
            {
                Tokens.Add(Text.Substring(TokenStart));
            }

            return Tokens;
        }

        public static List<string> Tokenize(string Text)
        {
            List<string> Tokens = new List<string>();

            int TokenStart = 0;
            int StrPos = 0;
            while (StrPos < Text.Length)
            {
                char CurrentChar = Text[StrPos];
                if (!Char.IsLetter(CurrentChar))
                {
                    string TokenText = Text.Substring(TokenStart, StrPos - TokenStart);
                    if (TokenText.Length > 0)
                    {
                        Tokens.Add(TokenText);
                    }
                    TokenStart = StrPos + 1;
                }
                StrPos++;
            }
            if (StrPos > TokenStart)
            {
                Tokens.Add(Text.Substring(TokenStart));
            }

            return Tokens;
        }

        protected static string NormalizeTerm(string Term, string Lang)
        {
            Snowball.Stemmers.Stemmer Stemmer = Snowball.Snowball.GetStemmerForLocale(Lang);
            if (Stemmer == null)
            {
                return Term;
            }
            return Snowball.Snowball.Stem(Term.ToLower(), Stemmer);
        }

        public static void WriteToFile(string Name, Dictionary<string, int> TermFrequency, StreamWriter Writer)
        {
            Writer.Write(Name);
            Writer.Write("\t");
            bool First = true;
            foreach (string Term in TermFrequency.Keys)
            {
                if (!First)
                {
                    Writer.Write(",");
                }
                else
                {
                    First = false;
                }
                Writer.Write(Term);
                Writer.Write(":");
                Writer.Write(IntEncoder.Encode(TermFrequency[Term]));
            }
            Writer.Write("\n");
        }

        public static Dictionary<string, int> ParseText(string Text, WordFilter WordFilt, int MinWords, string Lang, Dictionary<string, int> TermDict)
        {
            Text = Text.ToLower();

            //List<string> Tokens = FilterTokens(Tokenize(Text), Lang, WordFilt);
            List<string> Tokens = FilterTokens(TokenizeNgram(Text, TermDict), Lang, WordFilt);

            Dictionary<string, int> TermFrequency = new Dictionary<string, int>();

            int Freq = 0;
            string Term;
            foreach (string Token in Tokens)
            {
                Term = Token.ToLower(); // No normalization, avoid false positive matching
                TermFrequency.TryGetValue(Term, out Freq);
                if (Freq <= 0)
                {
                    Freq = 1;
                }
                else
                {
                    Freq++;
                }
                TermFrequency[Term] = Freq;
            }
            return TermFrequency;
        }

        private static string ParseTitle(string title, Dictionary<string, int> terms)
        {
            int begin = 0;
            int end = -1;
            for (int i = 0; i < title.Length - 1; i++)
            {
                //if (i == Title.Length - 1 | (Title[i] > 96 & Title[i] < 123 & Title[i + 1] < 91 & Title[i + 1] > 64))
                if (i == title.Length - 2)
                {
                    end = i + 1;
                }
                else if (title[i] > 96 & title[i] < 123 & title[i + 1] < 91 & title[i + 1] > 64)
                {
                    end = i;
                }
                else
                {
                    end = -1;
                }

                if (end > 0)
                {
                    string subTerm = title.Substring(begin, end - begin + 1);
                    if (subTerm.Length > 1 & terms.ContainsKey(subTerm) & i == end)
                    {
                        title = title.Insert(end + 1, " ");
                        begin = end + 2;
                    }
                }
            }
            return title;
        }

        //public static WikipediaEntry FilterPage(string Title, string Text, Dictionary<string, int> CategoryList)
        //{
        //    WikipediaEntry Entry = new WikipediaEntry();

        //    Entry.Name = Title.ToLower();
        //    Entry.Category = ExtracCategory(Text, CategoryList);
        //    if (Entry.Category == null)
        //    {
        //        return null;
        //    }

        //    Entry.Link = ExtracLinks(Text);
        //    Entry.Title = ExtracTitle(Text);
        //    Text = MediaWikiMarkupToPlainText(Text).ToLower();
        //    Entry.Text = Text;

        //    return Entry;
        //}

        public static WikipediaEntry ParsePage(string Title, string Text, WordFilter WordFilt, int MinWords, string Lang,
            Dictionary<string, int> TermDict, Dictionary<string, int> CategoryList = null, bool bText=false)
        {
            // Added by Bin: Add title as phase to tokens
            int begin = 0;
            int end = -1;
            for (int i = 0; i < Title.Length - 1; i++)
            {
                //if (i == Title.Length - 1 | (Title[i] > 96 & Title[i] < 123 & Title[i + 1] < 91 & Title[i + 1] > 64))
                if (i == Title.Length - 2)
                {
                    end = i + 1;
                }
                else if (Title[i] > 96 & Title[i] < 123 & Title[i + 1] < 91 & Title[i + 1] > 64)
                {
                    end = i;
                }
                else
                {
                    end = -1;
                }

                if (end > 0)
                {
                    string subTerm = Title.Substring(begin, end - begin + 1);
                    if (subTerm.Length > 1 & TermDict.ContainsKey(subTerm) & i == end)
                    {
                        Title = Title.Insert(end + 1, " ");
                        begin = end + 2;
                    }
                }
            }

            WikipediaEntry Entry = new WikipediaEntry();
            Entry.Name = Title;
            Entry.Category = ExtracCategory(Text, CategoryList);
            if (Entry.Category == null)
            {
                return null;
            }
            Entry.Link = ExtracLinks(Text);
            Entry.Redirect = ExtracTitle(Text);
            Entry.Description = MediaWikiMarkupToPlainTextDescription(ExtracDescription(Text));

            foreach (var t in Entry.Redirect)
            {
                Title = Title + "." + t.Replace('_', ' ').Replace(":", string.Empty).Replace(",", string.Empty);
            }

            // add category info to title
            //foreach (var c in Entry.Category)
            //{
            //    Title = Title + "." + c.Replace('_', ' ').Replace(":", string.Empty).Replace(",", string.Empty);
            //}
            
            if (bText)
            {
                Text = MediaWikiMarkupToPlainText(Text);

                foreach (var c in Entry.Category)
                {
                    Text = Text + "." + c.Replace('_', ' ').Replace(":", string.Empty).Replace(",", string.Empty);
                }

                //Entry.Text = Text;
                //Text = Text.ToLower();
                //Tokens.Add(Title);
                // prepend title to emphasize title words (make them more important)!
                //Text = Title + "." + Text;
            }
            else
            {
                Text = Title; 
            }

            ////List<string> Tokens = FilterTokens(Tokenize(Text), Lang, WordFilt);
            //List<string> Tokens = FilterTokens(TokenizeNgram(Text, TermDict), Lang, WordFilt);
            //Dictionary<string, int> TermFrequency = new Dictionary<string, int>();

            //int Freq = 0;
            //string Term;
            //foreach (string Token in Tokens)
            //{
            //    //Term = NormalizeTerm(Token, Lang);
            //    Term = Token.ToLower(); // No normalization, avoid false positive matching
            //    TermFrequency.TryGetValue(Term, out Freq);
            //    if (Freq <= 0)
            //    {
            //        Freq = 1;
            //    }
            //    else
            //    {
            //        Freq++;
            //    }
            //    TermFrequency[Term] = Freq;
            //}

            if (bText)
            {
                Entry.TermFrequency = BuildVector(Text, TermDict, Lang, WordFilt);
            }

            Entry.TermFrequencyTitle = BuildVector(Title, TermDict, Lang, WordFilt);
            Entry.TermFrequencyDescription = BuildVector(Entry.Description, TermDict, Lang, WordFilt);

            //Console.WriteLine(Entry);
            return Entry;
        }

        protected static Dictionary<string, int> BuildVector(string Text, Dictionary<string, int> TermDict, string Lang, WordFilter WordFilt)
        {
            Text = Text.ToLower();

            Dictionary<string, int> TermFrequency = new Dictionary<string, int>();

            if (Text == "")
                return TermFrequency;

            List<string> Tokens = FilterTokens(TokenizeNgram(Text, TermDict), Lang, WordFilt);
            int Freq = 0;
            string Term;
            foreach (string Token in Tokens)
            {
                Term = Token.ToLower(); // No normalization, avoid false positive matching
                TermFrequency.TryGetValue(Term, out Freq);
                if (Freq <= 0)
                {
                    Freq = 1;
                }
                else
                {
                    Freq++;
                }
                TermFrequency[Term] = Freq;
            }
            return TermFrequency;
        }

        protected static void BuildTitleDict(string XmlFilename, string OutFilename)
        {
            int MAX_VALUE_WIDTH = Convert.ToString(Int32.MaxValue).Length;

            FileStream FS = new FileStream(OutFilename, FileMode.Create);
            StreamWriter Writer = new StreamWriter(FS);
            for (int i = 0; i < MAX_VALUE_WIDTH; i++)
            {
                Writer.Write(" ");
            }
            Writer.Write("\n");

            //XmlReader Reader = XmlReader.Create(XmlFilename);
            XmlReader Reader = XmlReader.Create(new StreamReader(XmlFilename, Encoding.GetEncoding("ISO-8859-9")));

            string TagName = null;
            string PageTitle = null;

            while (Reader.Read())
            {
                switch (Reader.NodeType)
                {
                    case XmlNodeType.Element:
                        TagName = Reader.Name;
                        break;
                    case XmlNodeType.Text:
                        if (TagName == "title")
                        {
                            PageTitle = Reader.Value;
                            Writer.WriteLine(PageTitle);
                        }
                        break;
                }
            }
            Writer.Close();
            Reader.Close();
        }

        protected static void BuildCategoryTree(string XmlFilename, string OutFilename)
        {
            int MAX_VALUE_WIDTH = Convert.ToString(Int32.MaxValue).Length;

            FileStream FS = new FileStream(OutFilename, FileMode.Create);
            StreamWriter Writer = new StreamWriter(FS);
            for (int i = 0; i < MAX_VALUE_WIDTH; i++)
            {
                Writer.Write(" ");
            }
            Writer.Write("\n");

            //XmlReader Reader = XmlReader.Create(XmlFilename);
            XmlReader Reader = XmlReader.Create(new StreamReader(XmlFilename, Encoding.GetEncoding("ISO-8859-9")));

            string TagName = null;
            string PageTitle = null;
            string PageText = null;
            while (Reader.Read())
            {
                switch (Reader.NodeType)
                {
                    case XmlNodeType.Element:
                        TagName = Reader.Name;
                        break;
                    case XmlNodeType.Text:
                        if (TagName == "title")
                        {
                            PageTitle = Reader.Value;
                        }
                        else if (TagName == "text")
                        {
                            PageText = Reader.Value;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (Reader.Name == "page")
                        {
                            if (PageTitle.StartsWith("Category:"))
                            {
                                Writer.WriteLine(PageTitle);
                            }
                        }
                        break;
                }
            }
            Writer.Close();
        }

        protected static void ExtractRedirects(string XmlFilename, string OutFilename)
        {
            int MAX_VALUE_WIDTH = Convert.ToString(Int32.MaxValue).Length;

            FileStream FS = new FileStream(OutFilename, FileMode.Create);
            StreamWriter Writer = new StreamWriter(FS);
            XmlReader Reader = XmlReader.Create(new StreamReader(XmlFilename, Encoding.GetEncoding("ISO-8859-9")));

            string TagName = null;
            string PageTitle = null;
            string PageText = null;
            while (Reader.Read())
            {
                switch (Reader.NodeType)
                {
                    case XmlNodeType.Element:
                        TagName = Reader.Name;
                        break;
                    case XmlNodeType.Text:
                        if (TagName == "title")
                        {
                            PageTitle = Reader.Value;
                        }
                        else if (TagName == "text")
                        {
                            PageText = Reader.Value;
                            PageText = PageText.Replace("\r", "");
                            PageText = PageText.Replace("\n", "");
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (Reader.Name == "page")
                        {
                            if (PageText.StartsWith("#REDIRECT [["))
                            {
                                Writer.WriteLine(PageTitle + '\t' + PageText.TrimEnd() + '\r');
                            }
                        }
                        break;
                }
            }
            Writer.Close();
        }

        protected static Dictionary<string, int> LoadList(string FileName, Dictionary<string, int> Terms=null)
        {
            if (Terms == null)
            {
                Terms = new Dictionary<string, int>();
            }

            Regex Rx = new Regex(@"(\(.*\))");

            string[] lines = System.IO.File.ReadAllLines(FileName);
            foreach (var line in lines)
            {
                string[] items = line.ToLower().Split('\t');
                items[0] = items[0].Replace(",", "").Replace(":", "");
                items[0] = Rx.Replace(items[0], "").Trim();

                if (!Terms.ContainsKey(items[0]))
                {
                    if (items.Length == 2)
                    {
                        Terms.Add(items[0], Int32.Parse(items[1]));
                    }
                    else if (items.Length == 1)
                    {
                        Terms.Add(items[0], 1);
                    }
                    else
                    {
                        Console.Write("error");
                    }
                }
            }
            return Terms;
        }

        protected static void ParseWikiXML(string XmlFilename, string OutFilename, WikipediaPageFilter PageFilter, WordFilter WordFilt, int MinWords, int MaxEntries, string Lang, 
            Dictionary<string, int> TermList, Dictionary<string, int> CategoryList, bool bText=false)
        {
            int EntriesCount = 0;
            int IgnoreCount = 0;
            int SkipCount = 0;
            int MAX_VALUE_WIDTH = Convert.ToString(Int32.MaxValue).Length;

            FileStream FS = new FileStream(OutFilename, FileMode.Create);
            StreamWriter Writer = new StreamWriter(FS);
            for (int i = 0; i < MAX_VALUE_WIDTH; i++)
            {
                Writer.Write(" ");
            }
            Writer.Write("\n");

            //XmlReader Reader = XmlReader.Create(XmlFilename);
            XmlReader Reader = XmlReader.Create(new StreamReader(XmlFilename, Encoding.GetEncoding("ISO-8859-9")));

            string TagName = null;
            string PageTitle = null;
            string PageText = null;
            while (Reader.Read())
            {
                switch (Reader.NodeType)
                {
                    case XmlNodeType.Element:
                        TagName = Reader.Name;
                        break;
                    case XmlNodeType.Text:
                        if (TagName == "title")
                        {
                            PageTitle = Reader.Value;
                        }
                        else if (TagName == "text")
                        {
                            PageText = Reader.Value;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (Reader.Name == "page")
                        {
                            if (PageTitle != null && PageText != null)
                            {
                                if (PageFilter != null && PageFilter.IsPageIgnored(PageTitle, PageText))
                                {
                                    Console.WriteLine("Ignoring " + PageTitle);
                                    IgnoreCount++;
                                }
                                else
                                {
                                    WikipediaEntry Entry = ParsePage(PageTitle, PageText, WordFilt, MinWords, Lang, TermList, CategoryList, bText);
                                    if (Entry != null)
                                    {
                                        Console.WriteLine("Adding " + PageTitle);
                                        Entry.WriteToFile(Writer, bText);
                                        EntriesCount++;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Skipping " + PageTitle);
                                        SkipCount++;
                                    }
                                }
                            }
                            PageTitle = null;
                            PageText = null;
                        }
                        TagName = null;
                        break;
                }
                if (EntriesCount >= MaxEntries)
                {
                    break;
                }
            }

            Writer.Flush();

            FS.Seek(0, SeekOrigin.Begin);
            Writer.Write(EntriesCount);
            Writer.Close();

            Console.WriteLine("{0} pages ignored", IgnoreCount);
            Console.WriteLine("{0} pages skipped", SkipCount);
            Console.WriteLine("{0} pages added", EntriesCount);
        }

        protected static void ParseProduct(string Filename, string OutFilename, Dictionary<string, int> TermDict)
        {
            WikipediaPageFilter PageFilter = new WikipediaPageFilter();
            WordFilter WordFilt = new WordFilterLocalSearch();

            int MinWords = 64;
            string Lang = "en";

            STOP_WORDS_EN_HASH = new Dictionary<string, bool>();
            foreach (string Word in STOP_WORDS_EN)
            {
                STOP_WORDS_EN_HASH[Word] = true;
            }

            STOP_WORDS_GLOBAL_HASH = new Dictionary<string, bool>();
            foreach (string Word in STOP_WORDS_GLOBAL)
            {
                STOP_WORDS_GLOBAL_HASH[Word] = true;
            }

            StreamReader reader = new StreamReader(File.OpenRead(Filename));
            StreamWriter writer = new StreamWriter(File.OpenWrite(OutFilename));

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split('\t');
                string name = items[0];
                string text = items[1];

                Dictionary<string, int> Terms = ParseText(text, WordFilt, MinWords, Lang, TermDict);

                WriteToFile(name, Terms, writer);
            }
        }

        protected static void BuildAttributeVectors(string EntriesFilename, string TermsFilename, string AVecFilename)
        {
            const int TFIDF_SCALE_FACTOR = Int16.MaxValue;

            StreamReader Reader;
            FileStream FS;
            StreamWriter Writer;
            string Line;
            WikipediaEntry Entry;
            int NumberOfDocuments;
            int NumberOfDocumentsContainingTerm;

            FileStream fs = new FileStream(EntriesFilename, FileMode.Open, FileAccess.Read);
            Dictionary<string, int> NumberOfDocumentsContainingTermHash;

            try
            {
                Reader = new StreamReader(fs);
                NumberOfDocuments = Convert.ToInt32(Reader.ReadLine());
                Console.WriteLine("Number of docs: " + NumberOfDocuments);
                NumberOfDocumentsContainingTermHash = new Dictionary<string, int>(NumberOfDocuments * 3);

                Line = "";
                int EntryCount = 0;
                int FieldStart;
                while ((Line = Reader.ReadLine()) != null)
                {
                    // add sentinel at end of line..
                    Line += ",";
                    FieldStart = 0;
                    for (int i = 0; i < Line.Length; i++)
                    {
                        if (Line[i] == '\t')
                        {
                            FieldStart = i + 1;
                            break;
                        }
                    }
                    int akaStart = FieldStart;
                    for (int i = akaStart; i < Line.Length; i++)
                    {
                        if (Line[i] == '\t')
                        {
                            FieldStart = i + 1;
                            break;
                        }
                    }
                    int CategoryStart = FieldStart;
                    for (int i = CategoryStart; i < Line.Length; i++)
                    {
                        if (Line[i] == '\t')
                        {
                            FieldStart = i + 1;
                            break;
                        }
                    }

                    string Term = "";
                    for (int i = FieldStart; i < Line.Length; i++)
                    {
                        if (Line[i] == ':')
                        {
                            Term = Line.Substring(FieldStart, i - FieldStart);
                            Term = Term.ToLower();
                        }
                        else if (Line[i] == ',')
                        {
                            NumberOfDocumentsContainingTermHash.TryGetValue(Term, out NumberOfDocumentsContainingTerm);
                            NumberOfDocumentsContainingTermHash[Term] = NumberOfDocumentsContainingTerm + 1;
                            FieldStart = i + 1;
                        }
                    }
                    if (EntryCount % 1000 == 0)
                    {
                        Console.Write(".");
                    }
                    EntryCount++;
                }
                Console.Write("\n");
            }
            finally
            {
                fs.Close();
            }

            Console.WriteLine("Number of terms: " + NumberOfDocumentsContainingTermHash.Count);

            Dictionary<string, int> TermToId = new Dictionary<string, int>(NumberOfDocumentsContainingTermHash.Count);
            List<string> Terms = new List<string>(NumberOfDocumentsContainingTermHash.Count);
            foreach (string Term in NumberOfDocumentsContainingTermHash.Keys)
            {
                Terms.Add(Term);
            }
            // sort terms by frequency (descending)
            // => frequent terms get a lower ID!
            SRComparer SRComp = new SRComparer(NumberOfDocumentsContainingTermHash);
            Terms.Sort(SRComp);

            FS = new FileStream(TermsFilename, FileMode.Create);
            Writer = new StreamWriter(FS);
            Writer.Write(Terms.Count);
            Writer.Write("\n");
            int TermId = 1;
            foreach (string Term in Terms)
            {
                Writer.Write(Term + "\n");
                TermToId[Term] = TermId;
                TermId++;
            }
            Writer.Close();
            Terms = null;

            FS = new FileStream(AVecFilename, FileMode.Create);
            Writer = new StreamWriter(FS);
            Writer.Write(NumberOfDocuments);
            Writer.Write("\n");

            const double MIN_TFIDF = 0.001;
            int AllTermsFrequency;
            Dictionary<int, int> TFIDF;
            Dictionary<string, int> sTFIDF;
            Dictionary<string, int> sTF;
            Dictionary<string, int> sIDF = new Dictionary<string, int>();
            double TF;
            double IDF;
            double Temp;
            AttributeVector AttrVec;

            fs = new FileStream(EntriesFilename, FileMode.Open, FileAccess.Read);
            Reader = new StreamReader(fs);
            Reader.ReadLine(); // first line: number of docs
            while ((Line = Reader.ReadLine()) != null)
            {
                Entry = WikipediaEntry.ReadFromLine(Line);

                Console.WriteLine(Entry.Name);
                AllTermsFrequency = 0;
                foreach (int TermFreq in Entry.TermFrequency.Values)
                {
                    AllTermsFrequency += TermFreq;
                }
                TFIDF = new Dictionary<int, int>(Entry.TermFrequency.Count);
                sTFIDF = new Dictionary<string, int>(Entry.TermFrequency.Count);
                sTF = new Dictionary<string, int>(Entry.TermFrequency.Count);
                foreach (string Term in Entry.TermFrequency.Keys)
                {
                    //TF = Entry.TermFrequency[Term] * 1.0 / AllTermsFrequency;
                    TF = Entry.TermFrequency[Term];
                    sTF[Term] = Entry.TermFrequency[Term];

                    //NumberOfDocumentsContainingTermHash.TryGetValue(Term, out NumberOfDocumentsContainingTerm);
                    //if (NumberOfDocumentsContainingTerm < 1)
                    //{
                    //    NumberOfDocumentsContainingTerm = 1;
                    //}
                    //IDF = Math.Log(NumberOfDocuments * 1.0 / NumberOfDocumentsContainingTerm);
                    //sIDF[Term] = Convert.ToInt32(IDF * TFIDF_SCALE_FACTOR);

                    //Temp = TF * IDF;
                    //if (Temp > MIN_TFIDF)
                    //{
                    //    TFIDF[TermToId[Term]] = Convert.ToInt32(Temp * TFIDF_SCALE_FACTOR);
                    //    sTFIDF[Term] = Convert.ToInt32(Temp * TFIDF_SCALE_FACTOR);
                    //}
                }
                AttrVec = new AttributeVector();
                AttrVec.Name = Entry.Name;
                //AttrVec.sTFIDF = sTFIDF;
                AttrVec.sTF = sTF;

                // Revised by Bin
                // AttrVec.WriteToFile(Writer);
                AttrVec.SaveToFile(Writer); // without decoding
            }
            Reader.Close();
            Writer.Close();

            SaveIDF(sIDF, "IDF.txt");
        }

        public static void SaveIDF(Dictionary<string, int> IDF, string fileName)
        {
            using (var file = new StreamWriter(fileName))
                foreach (var entry in IDF)
                    file.WriteLine("{0} {1}", entry.Key, entry.Value);
        }

        public static void ParseWiki(string XmlFileName, string OutFilename, Dictionary<string, int> Terms, Dictionary<string, int> CategoryList, bool bText=false)
        {
            WikipediaPageFilter PageFilter = new WikipediaPageFilter();
            WordFilter WordFilt = new WordFilterLocalSearch();

            int MinWords = 64;
            int MaxEntries = Int32.MaxValue;
            string Lang = "en";

            STOP_WORDS_EN_HASH = new Dictionary<string, bool>();
            foreach (string Word in STOP_WORDS_EN)
            {
                STOP_WORDS_EN_HASH[Word] = true;
            }

            STOP_WORDS_GLOBAL_HASH = new Dictionary<string, bool>();
            foreach (string Word in STOP_WORDS_GLOBAL)
            {
                STOP_WORDS_GLOBAL_HASH[Word] = true;
            }

            ParseWikiXML(XmlFileName, OutFilename, PageFilter, WordFilt, MinWords, MaxEntries, Lang, Terms, CategoryList, bText);
        }

        static void Main(string[] args)
        {
            //string Text = "dfgdgdfg (aaa.bbb) sfd (c) fdsdfg (   ,ddd   (eee) abc )";
            //Regex Rx = new Regex(@"\(([^)]+?)\)");
            //Text = Rx.Replace(Text, " ");

            // Test
            //WordFilter WordFilt = new WordFilterLocalSearch();
            //string Text = "cannon eos 5d ab-cd1";
            //List<string> Tokens = FilterTokens(TokenizeNgram(Text, Terms), "en", WordFilt);
            //BuildCategoryTree(@"d:\bincao\Data\Wiki\raw_data\enwiki-2012-pages-articles.xml", @"d:\bincao\Data\Wiki\categories.2012.txt");

            //string version = @"wiki.2012.films";
            //string path = @"d:\bincao\Workspace\Adselection\data\films\";
            string version = @"wiki.2012.commerce_companies_industry.top10";
            string path = @"d:\bincao\Workspace\Adselection\data\business\";
            //string ProdFileName = @"d:\bincao\Data\Ad\categories.info.txt";
            //string ProdOutFilename = @"d:\bincao\Data\Ad\categories.info.dat";

            // step 1: get all the titles
            //BuildTitleDict(@"d:\bincao\Data\Wiki\raw_data\enwiki-2012-pages-articles.xml", @"d:\bincao\Data\Wiki\titles.2012.txt");

            // step 2: load terms
            Dictionary<string, int> Terms = LoadList(@"d:\bincao\Data\Wiki\titles.2012.txt");
            Terms = LoadList(path + @"terms_from_redirects_en.txt", Terms);
            Terms = LoadList(path + @"wiki.2012.redirect.terms.txt", Terms);
            Dictionary<string, int> CategoryList = LoadList(path + version + @".list");
            //Dictionary<string, int> CategoryList = LoadList(@"d:\bincao\Data\Wiki\commerce_and_companies_and_industry.top10.list");

            // step 3: parse wiki
            string WikiXmlFileName = @"d:\bincao\Data\Wiki\raw_data\enwiki-2012-pages-articles.xml";
            string WikiOutFilename = path + version + ".entries";
            ParseWiki(WikiXmlFileName, WikiOutFilename, Terms, CategoryList, true);
            
            // step 3.1 get redirects
            //ExtractRedirects(WikiXmlFileName, version + ".redirect");

            // step 4: parse products
            //ParseProduct(ProdFileName, ProdOutFilename, Terms);

            // step 5: build vector space model
            // step 5.1 merge entries.20111102.dat and categories.info.dat
            //string EntriesFilename = path + version + ".entries";
            //string TermsFilename = path + version + ".terms";
            //string AVecFilename = path + version + ".avectors";
            //BuildAttributeVectors(EntriesFilename, TermsFilename, AVecFilename);

            Console.WriteLine("Done");
        }
    }
}
