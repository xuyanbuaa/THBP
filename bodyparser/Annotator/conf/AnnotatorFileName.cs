using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Annotator.conf
{
    public class AnnotatorFileName
    {


        //Dictionaries
        public static string dict_dir = @"..\..\..\Dictionary\";
        public static string dict_drug_file = dict_dir + "NewDrugName.txt";
        public static string dict_equip_file = dict_dir + @"Equipment\equipment.dict";
        public static string dict_blood_urine_file = dict_dir + @"Blood_Urine_Dict\blood_urine_index.dict";
        public static string dict_dosage_file = dict_dir + @"dosage\dosage.txt";
        public static string dict_timeunit_file = dict_dir + @"timeunit.txt";
        public static string dict_month_file = dict_dir + @"month.txt";
        public static string dict_weekday_file = dict_dir + @"weekday.txt";
        public static string dict_prep_time = dict_dir + @"preposition_time.txt";
        public static string dict_apm_file = dict_dir + @"apm.txt";
        public static string dict_key_section = dict_dir + @"keysection.txt";
        public static string dict_key_timepoint = dict_dir + @"keytimepoint.txt";
        public static string dict_noons = dict_dir + @"noons.txt";
        public static string dict_time_adverb = dict_dir + @"timeadverb.txt";
        public static string dict_department = dict_dir + @"Department_Name\Medical_Department_List.txt";
        public static string dict_doctorname = dict_dir + @"Doctor_Name\doctor_name_List.txt";
        public static string dict_occurrence = dict_dir + @"occurrence\occurrence_train.txt";
        public static string dict_occurrence_verb = dict_dir + @"occurrence\verbs.txt";
        public static string dict_evidential = dict_dir + @"evidential\evidential";
        public static string dict_evidential_verb = dict_dir + @"evidential\evidential_total.verb.txt";
        public static string dict_concept = dict_dir + @"ADictionary\concept_st.dict";
        public static string dict_mtrees = dict_dir + @"ADictionary\mtrees2010.dict";
        public static string dict_RedLex = dict_dir + @"ADictionary\RedLex.dict";
        public static string dict_partslist = dict_dir + @"ADictionary\partslist.dict";
        public static string dict_anatomy = dict_dir + @"ADictionary\AnatomyDict.dict";
        public static string dict_position = dict_dir + @"position.txt";
        public static string dict_anatmypart = dict_dir + @"ADictionary\PartOfAD.dict";
        public static string dict_abbreviation = dict_dir + @"abbreviation.txt";
        public static string dict_wikicut = dict_dir + @"wiki_cut.txt";
        public static string dict_appearwords = dict_dir + @"appearwords.txt";
    }
}
