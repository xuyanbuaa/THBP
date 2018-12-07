using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Annotator.conf;
using Annotator.conf.entity;
using Annotator.util;

using TimeExtractor.util;
using TimeExtractor.units;

namespace Annotator
{
    public class Annotator
    {

        static Entity find(ref IEnumerable<Entity> entities, int line, int col)
        {
            foreach (Entity entity in entities)
                if (entity.startLoc.line == line && entity.startLoc.col == col)
                    return entity;
            return null;
        }

        public static void Annotate(string filename, string suffix, IEnumerable<Entity> entities, bool StemOnly)
        {
            if (!File.Exists(filename))
                return;
            int dotp = filename.LastIndexOf(".");
            string con_filename = filename.Substring(0, dotp) + suffix;

            string[] lines = File.ReadAllLines(filename);
            List<string> outputs = new List<string>();
            int lineNumber = 1;
            foreach (string line in lines)
            {
                string[] words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder sb = new StringBuilder();
                int p = 0;
                while (p < words.Length)
                {
                    Entity entity = find(ref entities, lineNumber, p);
                    if (p > 0) sb.Append(" ");
                    if (entity == null)
                        sb.Append(words[p++]);
                    else
                    {
                        string tagged_str = entity.ToAnnotateFormat(StemOnly);
                        sb.Append(tagged_str);
                        p = entity.endLoc.col + 1;
                    }
                }
                lineNumber++;
                outputs.Add(sb.ToString());
            }

            File.WriteAllLines(con_filename, outputs.ToArray());

        }

        public static Entity[] ReadAnnotate(string filename)
        {

            string[] lines = File.ReadAllLines(filename);

            List<Entity> ret = new List<Entity>();
            Regex timeRegex = new Regex(@"``(?<text>[^`]+(?:`[^`]+)*)``(?:[^\s]*)");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                line = Regex.Replace(line, @"\s+", " ");
                line = Regex.Replace(line, @" `` ", " ``");
                line = Regex.Replace(line, @" ``\{", "``{");

                MatchCollection timematches = timeRegex.Matches(line);
                foreach (Match match in timematches)
                {
                    string text = match.Groups["text"].Value;
                    string striptext = TimeExtractor.util.StringUtil.strip(text);
                    Entity entity = EntityUtil.AnnotateToEntity(match.Value);
                    int lineNumber = i + 1;
                    int startCol = StringUtil.wordIndex(line, match.Index);
                    int endCol = StringUtil.wordIndex(line, match.Index + match.Value.Length - 1);
                    entity.startLoc = new TextIdentifier(filename, lineNumber, startCol);
                    entity.endLoc = new TextIdentifier(filename, lineNumber, endCol);
                    ret.Add(entity);
                }
            }

            return ret.ToArray();

        }

        public static void UpdateOriginalData(string annotated_file, string orig_file)
        {

            //Use this carefully, it will cover the original data
            //Be sure to back up the original files

            Regex timeRegex = new Regex(@"``(?<text>[^`]+(?:`[^`]+)*)``[^\s]*");

            string[] lines = File.ReadAllLines(annotated_file);
            List<string> newlines = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                MatchCollection matches = timeRegex.Matches(line);
                foreach (Match match in matches)
                {
                    string orig = match.Value;
                    string text = match.Groups["text"].Value;
                    line = line.Replace(orig, text);
                }
                line = line.Replace("``", "");
                newlines.Add(line);
            }

            File.WriteAllLines(orig_file, newlines.ToArray());

        }

    }
}
