using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TimeExtractor.units;
using TimeExtractor.util;

namespace Annotator.conf.entity
{

    public enum Polarity
    {
        POS, NEG
    }

    public enum Modality
    {
        FACTUAL, CONDITIONAL, POSSIBLE, PROPOSED
    }

    public enum Assertion
    {
        PRESENT, ABSENT, POSSIBLE, CONDITIONAL, HYPOTHETICAL, SOMEONEELSE
    }

    public class PMTEntity : Entity
    {

        public Polarity polarity = Polarity.POS;
        public Modality modality = Modality.FACTUAL;
        public Assertion assertion = Assertion.PRESENT;

        public override string ToI2b2Format(bool StemOnly)
        {

            string stem = "c=\"" + text + "\" " + startLoc.line + ":" + startLoc.col + " " + endLoc.line + ":" + endLoc.col;
            stem += "||t=\"" + type + "\"";
            stem += "||modality=\"";
            switch (modality)
            {
                case Modality.CONDITIONAL: stem += "CONDITIONAL"; break;
                case Modality.FACTUAL: stem += "FACTUAL"; break;
                case Modality.POSSIBLE: stem += "POSSIBLE"; break;
                case Modality.PROPOSED: stem += "PROPOSED"; break;
            }
            stem += "\"";
            stem += "||polarity=\"";
            switch (polarity)
            {
                case Polarity.POS: stem += "POS"; break;
                case Polarity.NEG: stem += "NEG"; break;
            }
            stem += "\"";

            return stem;

        }

        public override string ToAnnotateFormat(bool StemOnly)
        {
            string stem = "";
            /*if (type == "problem") stem = "@@" + text + "@@";
            else if (type == "treatment") stem = "##" + text + "##";
            else stem = "^^" + text + "^^";*/
            stem = "##" + text + "##" + "(" + type + ")";
            if (StemOnly)
                return stem;
            return stem;
            
        }

    }

}
