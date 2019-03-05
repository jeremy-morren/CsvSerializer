using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvDocument
{
    /// <summary>
    /// Class That Reads through string
    /// using provided Delimiters
    /// </summary>
    class StringReader
    {
        /// <summary>
        /// Initializes <see cref="StringReader"/>
        /// with <paramref name="text"/>
        /// </summary>
        /// <param name="text"></param>
        public StringReader(string text)
        {
            CurrentIndex = 0;
            Text = text; //Ensure there is another character at the very end
        }

        /// <summary>
        /// Reads string to <paramref name="index"/>
        /// </summary>
        /// <param name="index">Index to read to</param>
        public string ReadToIndex(int index)
        {
            index = index == -1 ? Text.Length : index;
            //index = index >= Text.Length ? (Text.Length - 1) : index;
            string s = Text.Splice(CurrentIndex, index);
            CurrentIndex = index + 1;
            return s;
        }

        /// <summary>
        /// Reads to the first occurence
        /// of any member of <paramref name="chars"/>
        /// </summary>
        public string Read(string[] chars)
        {
            List<KeyValuePair<string, int>> index = new List<KeyValuePair<string, int>>();
            int length = Text.Length;
            char a = Text[CurrentIndex];
            foreach (string c in chars)
                index.Add(new KeyValuePair<string,int>(c,Text.IndexOf(c, CurrentIndex)));
            if (index.Count(e => e.Value != -1) == 0)
                throw new InvalidOperationException("No character found!");
            KeyValuePair<string, int> pair = index.Where(e => e.Value != -1).OrderBy(e => e.Value).First();
            return ReadToIndex(pair.Value + (pair.Key.Length - 1));
        }

        /// <summary>
        /// Reads to the next occurrence
        /// of <paramref name="c"/>
        /// </summary>
        /// <param name="c"><see cref="string"/> to read to</param>
        public string Read(string c)
            => ReadToIndex(Text.IndexOf(c, CurrentIndex));

        /// <summary>
        /// Reads to the next character
        /// </summary>
        public string Read()
            => ReadToIndex(CurrentIndex);

        /// <summary>
        /// Class Text to Read
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Current position within <see cref="Text"/>
        /// </summary>
        public int CurrentIndex { get; private set; }

        /// <summary>
        /// Gets a value indicating
        /// whether the end of <see cref="Text"/>
        /// has been reached
        /// </summary>
        public bool EOF
        { get => CurrentIndex == 0 ? false : CurrentIndex == Text.Length; }
    }
}
