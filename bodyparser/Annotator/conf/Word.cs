using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TimeExtractor.units;

namespace Annotator.conf
{
    public class Word
    {

        private string word;
        private string lower_word;
        private TextIdentifier id;
        private int pos;
        private string[] wiki;

        public string WordText
        {
            get { return word; }
            set
            {
                word = value;
                lower_word = value.ToLower();
            }
        }

        public string LowerWord
        {
            get { return lower_word; }
        }

        public TextIdentifier Id
        {
            get { return id; }
            set { id = value; }
        }

        public int Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        public string[] Wiki
        {
            get { return wiki; }
            set { wiki = value; }
        }

        public Word(string word, TextIdentifier id)
        {
            this.word = word;
            this.id = id;
        }


    }
}
