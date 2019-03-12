using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvDocument
{
    /// <summary>
    /// Reads blocks form text
    /// separated by <see cref="Delimiter"/>
    /// with grouping by <see cref="Aggregate"/>
    /// </summary>
    internal class StringReader
    {
        /// <summary>
        /// Initializes <see cref="StringReader"/>
        /// with <paramref name="text"/>
        /// </summary>
        /// <param name="text"></param>
        public StringReader(string text, string aggregate, string delimiter)
        {
            int i = System.Text.RegularExpressions.Regex.Matches(text, aggregate).Count;
            if (delimiter == aggregate)
                throw new ArgumentOutOfRangeException(null, $"{nameof(delimiter)} must be different to {aggregate}");
            if (System.Text.RegularExpressions.Regex.Matches(text, aggregate).Count % 2 != 0)
                throw new ArgumentOutOfRangeException(nameof(aggregate), aggregate,
                    $"{nameof(aggregate)} must occur an event number of times");
            CurrentIndex = 0;
            Aggregate = aggregate;
            Delimiter = delimiter;
            Text = text;
            //Ensure it ends with a delimiter (for simplicity)
            if (!Text.EndsWith(delimiter))
                Text += delimiter;
        }

        /// <summary>
        /// String that denotes a 'string'
        /// (i.e. that encloses a Delimiter)
        /// </summary>
        public string Aggregate { get; private set; }

        /// <summary>
        /// Delimiter that separates Blocks
        /// </summary>
        public string Delimiter { get; private set; }

        /// <summary>
        /// Reads string to <paramref name="index"/>
        /// </summary>
        /// <param name="index">Index to read to</param>
        string ReadToIndex(int index, int move)
        {
            index = index == -1 ? (Text.Length - 1) : index;
            string s = Text.Splice(CurrentIndex, index);
            CurrentIndex = index + move;
            return s;
        }

        /// <summary>
        /// Reads to the next
        /// occurrence of either <see cref="Aggregate"/>
        /// or <see cref="Delimiter"/>
        /// </summary>
        string ReadString()
        {
            int[] indexes = new[]
            {
                Text.IndexOf(Aggregate,CurrentIndex),
                Text.IndexOf(Delimiter,CurrentIndex)
            };
            //This should never happen but it is good practice
            if (indexes[0] == -1 && indexes[1] == -1)
                throw new Exception(null);
            //If Aggregate is -1 or after delimiter
            if (indexes[0] == -1 || indexes[0] > indexes[1])
                return ReadToIndex(indexes[1], Delimiter.Length);
            //Otherwise read to Aggregate
            return ReadToIndex(indexes[0], Aggregate.Length);
        }

        /// <summary>
        /// Reads to the next occurrence
        /// of <paramref name="c"/>
        /// </summary>
        /// <param name="c"><see cref="string"/> to read to</param>
        string ReadString(string c)
            => ReadToIndex(Text.IndexOf(c, CurrentIndex), c.Length);

        /// <summary>
        /// Reads to the End of the next aggregate
        /// </summary>
        /// <param name="inAggregate">Indicates whether we are currently within an Aggregate</param>
        /// <returns></returns>
        string ReadToAggregate(bool inAggregate)
        {
            //Optimization:  If we are inAggregate
            //then just skip ahead to the next instance
            string str = inAggregate ? ReadString(Aggregate) : ReadString();
            if (str.StartsWith(Aggregate) || str.EndsWith(Aggregate))
                return str + ReadToAggregate(!inAggregate); //Recursively execute
            return str; //We have reached the end of the Block, return value
        }

        /// <summary>
        /// Removes <see cref="Delimiter"/>
        /// from end of <paramref name="block"/>
        /// </summary>
        /// <param name="block">Raw Value</param>
        /// <returns>Block Value</returns>
        string GetValue(string block)
            => block.Length == Delimiter.Length ? null : block.Splice(0, block.Length - 2);

        /// <summary>
        /// Reads the next Block of Text
        /// </summary>
        public string Read()
        {
            string block = ReadString();
            //Normal Occurrence (i.e. no aggregate)
            //Chop off Delimiter
            if (!block.StartsWith(Aggregate) && !block.EndsWith(Aggregate))
                return GetValue(block);
            //Otherwise we are in an aggregate
            block += ReadToAggregate(true);
            //Chop off Delimiter
            block = GetValue(block);
            //Chop off Aggregates
            if (block.StartsWith(Aggregate) && block.EndsWith(Aggregate))
                return block.Splice(Aggregate.Length, block.Length - 1 - Aggregate.Length);
            return block;
        }

        /// <summary>
        /// Text to Read
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

    internal static class StringExtensions
    {
        /// <summary>
        /// Returns a string
        /// starting at <paramref name="startIndex"/>
        /// and ending at <paramref name="endIndex"/>
        /// </summary>
        /// <param name="startIndex">Starting Index</param>
        /// <param name="endIndex">Ending Index</param>
        /// <remarks>
        /// Allow me to ask why this isn't included in
        /// the standard library?
        /// </remarks>
        internal static string Splice(this string str, int startIndex, int endIndex)
        {
            if (startIndex < 0 || startIndex >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex,
                    "Parameter must refer to a position within the string");
            if (endIndex < 0 || endIndex >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(endIndex), endIndex,
                    "Parameter must refer to a position within the string");
            if (endIndex < startIndex)
                throw new ArgumentOutOfRangeException(nameof(endIndex), endIndex,
                    $"{nameof(endIndex)} must be greater than or equal to {nameof(startIndex)}");
            return str.Substring(startIndex, endIndex - startIndex + 1);
        }
    }
}
