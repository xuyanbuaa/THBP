using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Annotator.conf;
using Annotator.conf.entity;
using Annotator.util;

namespace Annotator.conf
{
    public class EntityLink
    {

        public Entity from, to;
        public TLinkType type;
        public FileRecord fr;

        private static string get_entity_output(Entity entity)
        {
            string s = "";
            if (entity is PMTEntity)
            {
                s = "EVENT=\"" + entity.text + "\"";
            }
            else
            {
                s = "TIMEX3=\"" + entity.text + "\"";
            }
            s += " " + entity.startLoc.line + ":" + entity.startLoc.col + " " + entity.endLoc.line + ":" + entity.endLoc.col;
            return s;
        }

        private static string get_type_string(TLinkType type)
        {
            switch (type)
            {
                case TLinkType.AFTER: return "AFTER";
                case TLinkType.BEFORE: return "BEFORE";
                case TLinkType.OVERLAP: return "OVERLAP";
                case TLinkType.OTHER: return "OTHER";
            }
            return "OVERLAP";
        }

        public virtual string ToI2b2Form()
        {
            string s1 = get_entity_output(from);
            string s2 = get_entity_output(to);
            string s = s1 + "||" + s2 + "||type=\"" + get_type_string(type) + "\"";
            return s;
        }

    }
}
