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
using TimeExtractor.conf;

namespace Annotator
{
    class Program
    {
        static void Main(string[] args)
        {

            Entity.Preprocess();

            int op = 3;

            if (op > 0)
            {
                //TimeExtractor.Preprocessing.preprocess();
            }

            for (int packageNo = 1; packageNo <= 6; packageNo++)
            {
                string folder = "docs\\Package " + packageNo;
               // string folder = "docs";
                string[] files = Directory.GetFiles(folder, "*.txt");
                foreach (string file in files)
                {

                    switch (op)
                    {

                        case 0:
                            {
                                //Tag with stemmed PMT tags

                                Regex fileregex = new Regex(@"[0-9]+\.txt");
                                Match match = fileregex.Match(file);
                                if (match == null) break;
                                string concept_filename = "concepts\\" + match.Value.Replace("txt", "con");
                                if (!File.Exists(concept_filename))
                                {
                                    Console.WriteLine("Uoh, concept file \"" + concept_filename + "\" not found.");
                                    break;
                                }

                                string[] cons = File.ReadAllLines(concept_filename);
                                List<Entity> entities = new List<Entity>();
                                foreach (string con in cons)
                                {
                                    if (con.Length <= 0) continue;
                                    Entity entity = EntityUtil.I2b2formToEntity(con, file);
                                    entities.Add(entity);
                                }

                                IEnumerable<Entity> PMTEntities =
                                    from entity in entities
                                    where entity.type == "problem" || entity.type == "treatment" || entity.type == "test"
                                    select entity;

                                Annotator.Annotate(file, ".PMTstem.con", entities, true);

                                break;
                            }

                        case 1:
                            {
                                TimeExtractor.tools.Init.setFilePath(file);
                                TimeExtractor.TimeMapping.process(false);

                                List<SenseGroup> sensegroups = TimeVariables.TIME_ENTITIES;
                                List<Entity> timeEntities = new List<Entity>();
                                foreach (SenseGroup sg in sensegroups)
                                {
                                    Entity entity = new TimeEntity();
                                    entity.text = sg.getWords()[0];
                                    entity.startLoc = sg.startLoc;
                                    entity.endLoc = sg.endLoc;
                                    entity.setTimePoint(sg.getTimePeriod().getFirstTimePoint());
                                    timeEntities.Add(entity);
                                }

                                Annotator.Annotate(file, ".time.con", timeEntities, false);

                                break;
                            }

                        case 2:
                            {

                                //Tag with PMT tags + guessed classifications

                                Regex fileregex = new Regex(@"[0-9]+\.txt");
                                Match match = fileregex.Match(file);
                                if (match == null) break;
                                string concept_filename = "concepts\\" + match.Value.Replace("txt", "con");
                                if (!File.Exists(concept_filename))
                                {
                                    Console.WriteLine("Uoh, concept file \"" + concept_filename + "\" not found.");
                                    break;
                                }

                                string sectionfile = file.Replace("txt", "section");
                                string[] sections = File.ReadAllLines(sectionfile);

                                string[] cons = File.ReadAllLines(concept_filename);
                                List<Entity> entities = new List<Entity>();
                                foreach (string con in cons)
                                {
                                    if (con.Length <= 0) continue;
                                    PMTEntity entity = (PMTEntity) EntityUtil.I2b2formToEntity(con, file);

                                    int lineNumber = entity.startLoc.line;

                                    int ptab = sections[lineNumber - 1].IndexOf("\t");
                                    string no = sections[lineNumber - 1].Substring(0, ptab);

                                    string[] wa_nos = { "1.1", "5.34", "5.34.78", "5.34.78.93", "5.34.78.93.35", "5.34.78.93.38", "5.34.78.96", "5.34.78.96.45", "5.34.79", "5.34.79.103.60", "5.35", "5.35.84", "5.35.91.108" };
                                    string[] a_nos = { "5.15", "5.22.44" };
                                    string[] ad_nos = { "5.37.106.125" };

                                    entities.Add(entity);

                                }

                                TimeExtractor.tools.Init.setFilePath(file);
                                TimeExtractor.TimeMapping.process(false);

                                List<SenseGroup> sensegroups = TimeVariables.TIME_ENTITIES;
                                foreach (SenseGroup sg in sensegroups)
                                {
                                    Entity entity = new TimeEntity();
                                    entity.text = sg.getWords()[0];
                                    entity.startLoc = sg.startLoc;
                                    entity.endLoc = sg.endLoc;
                                    entity.setTimePoint(sg.getTimePeriod().getFirstTimePoint());
                                    entities.Add(entity);
                                }

                                IEnumerable<Entity> PMTEntities =
                                    from entity in entities
                                    where entity.type == "problem" || entity.type == "treatment" || entity.type == "test" || entity.type == "time"
                                    select entity;

                                Annotator.Annotate(file, ".PMTrelation.con", entities, false);

                                break;

                            }

                        case 3:
                            {
                                //From annotated time expression (including normalized) to concept files

                                string annotate_file = file.Replace(".txt", ".time.con");
                                string con_file = file.Replace(".txt", ".time-con");
                                Entity[] entities = Annotator.ReadAnnotate(annotate_file);
                                EntityUtil.ExportConcept(con_file, entities, false);

                                break;
                            }

                        case 4:
                            {
                                //Update the original data
                                //WARNING: THINK BEFORE YOU DO THIS

                                string annotate_file = file.Replace(".txt", ".time.con");
                                string raw_file = file;
                                Annotator.UpdateOriginalData(annotate_file, raw_file);

                                break;
                            }

                        case 5:
                            {
                                //Tag with standard PMT tags + revised time taggings (including normalizations)

                                string time_con_file = file.Replace(".txt", ".time-con");
                                string pmt_con_file = "concepts\\" + FileNameUtil.FileNameNoSuffix(file) + ".con";
                                
                                Entity[] TIMEentities = EntityUtil.ImportConcept(time_con_file, "*.time-con", "*.txt");
                                Entity[] PMTentities = EntityUtil.ImportConcept(pmt_con_file, "*.con", "*.txt");
                                List<Entity> entities = new List<Entity>();
                                foreach (Entity entity in TIMEentities)
                                    entities.Add(entity);
                                foreach (Entity entity in PMTentities)
                                    entities.Add(entity);

                                Annotator.Annotate(file, ".PMTrelation.con", entities, false);

                                break;
                            }

                    }

                }
            }

        }

    }
}
