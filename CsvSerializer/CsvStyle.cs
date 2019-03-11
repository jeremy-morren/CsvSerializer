using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvDocument
{

    /// <summary>
    /// Helper class to represent
    /// a Set of Csv Special Characters
    /// </summary>
    public class CsvStyle
    {
        /// <summary>
        /// Initializes a Csv Style
        /// </summary>
        /// <param name="delimiter">Character to separate Csv Cells</param>
        /// <param name="aggregate">
        /// Character used to surround a cell
        /// that contains either <see cref="Delimiter"/>
        /// or <see cref="LineDelimiter"/>
        /// </param>
        /// <param name="lineDelimiter">Character used to Separate Lines</param>
        public CsvStyle(string delimiter, string aggregate, string lineDelimiter)
        {
            Delimiter = delimiter;
            Aggregate = aggregate;
            LineDelimiter = lineDelimiter;
        }

        /// <summary>
        /// Character used to Separate Csv Cells
        /// </summary>
        public string Delimiter { get; set; }

        /// <summary>
        /// Character used to surround a cell
        /// that contains either <see cref="Delimiter"/>
        /// or <see cref="LineDelimiter"/>
        /// </summary>
        public string Aggregate { get; set; }

        /// <summary>
        /// Character used to Separate Lines
        /// </summary>
        public string LineDelimiter { get; set; }

        /// <summary>
        /// Initializes based on <paramref name="Style"/>
        /// </summary>
        /// <param name="Style">Template Csv Style</param>
        public CsvStyle(CsvCharacterStyle Style)
        {
            switch (Style)
            {
                case CsvCharacterStyle.Windows:
                    Delimiter = ",";
                    Aggregate = "\"";
                    LineDelimiter = "\r\n";
                    break;
                case CsvCharacterStyle.Macintosh:
                    Delimiter = ",";
                    Aggregate = "\"";
                    LineDelimiter = "\r";
                    break;
                case CsvCharacterStyle.Unix:
                    Delimiter = ",";
                    Aggregate = "\"";
                    LineDelimiter = "\n";
                    break;
                case CsvCharacterStyle.WindowsText:
                    Delimiter = "\t";
                    Aggregate = "\"";
                    LineDelimiter = "\r\n";
                    break;
                case CsvCharacterStyle.MacintoshText:
                    Delimiter = "\t";
                    Aggregate = "\"";
                    LineDelimiter = "\r";
                    break;
                case CsvCharacterStyle.UnixText:
                    Delimiter = "\t";
                    Aggregate = "\"";
                    LineDelimiter = "\n";
                    break;
                default:
                    throw new InvalidOperationException("Unknown Template Style");
            }
        }
    }

    public enum CsvCharacterStyle
    {
        /// <summary>
        /// <list type="bullet">
        ///     <item>"\r\n" (CRLF) as LineEnding</item>
        ///     <item>"," As Cell Separation</item>
        ///     <item>"\"" As Aggregate</item>
        /// </list>
        /// </summary>
        Windows,
        /// <summary>
        /// <list type="bullet">
        ///     <item>"\r" (CR) as LineEnding</item>
        ///     <item>"," As Cell Separation</item>
        ///     <item>"\"" As Aggregate</item>
        /// </list>
        /// </summary>
        Macintosh,
        /// <summary>
        /// <list type="bullet">
        ///     <item>"\n" (LF) as LineEnding</item>
        ///     <item>"," As Cell Separation</item>
        ///     <item>"\"" As Aggregate</item>
        /// </list>
        /// </summary>
        Unix,
        /// <summary>
        /// <list type="bullet">
        ///     <item>"\r\n" (CRLF) as LineEnding</item>
        ///     <item>"\t" (TAB) As Cell Separation</item>
        ///     <item>"\"" As Aggregate</item>
        /// </list>
        /// </summary>
        WindowsText,
        /// <summary>
        /// <list type="bullet">
        ///     <item>"\r" (CR) as LineEnding</item>
        ///     <item>"\t" (TAB) As Cell Separation</item>
        ///     <item>"\"" As Aggregate</item>
        /// </list>
        /// </summary>
        MacintoshText,
        /// <summary>
        /// <list type="bullet">
        ///     <item>"\n" (LF) as LineEnding</item>
        ///     <item>"\t" (TAB) As Cell Separation</item>
        ///     <item>"\"" As Aggregate</item>
        /// </list>
        /// </summary>
        UnixText
    }

    
}
