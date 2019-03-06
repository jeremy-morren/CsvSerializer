using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CsvDocument
{
    /// <summary>
    /// Class used for
    /// working with CSV Files
    /// </summary>
    /// <typeparam name="T">Class to Serialize/Deserialize to</typeparam>
    public class CsvSerializer<T> where T : class, new()
    {
        /// <summary>
        /// Initializes a new instance
        /// using Default CSV Special Characters
        /// </summary>
        public CsvSerializer()
        {
            Schema = new CsvSchema<T>();
            Delimiter = ",";
            Aggregate = "\"";
            LineDelimiter = "\r\n";
        }

        /// <summary>
        /// Initializes a new Class
        /// with Custom CSV Characters
        /// </summary>
        /// <param name="delimiter">Character used to Separate Cells</param>
        /// <param name="aggregate">
        /// Character used to surround a cell that 
        /// contains either <paramref name="delimiter"/>
        /// or <paramref name="lineDelimiter"/>
        /// </param>
        /// <param name="lineDelimiter">Character that separates Lines</param>
        public CsvSerializer(string delimiter, string aggregate, string lineDelimiter) : this()
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
        /// Gets the Csv Schema for
        /// <typeparamref name="T"/>
        /// </summary>
        public CsvSchema<T> Schema { get; private set; }

        /// <summary>
        /// Deserializers <paramref name="CsvText"/>
        /// in Array of <typeparamref name="T"/>
        /// </summary>
        /// <param name="CsvText">Csv File Text</param>
        /// <returns>Array of <typeparamref name="T"/> with values from CSV File</returns>
        public T[] DeSerialize(string CsvText)
        {
            if (CsvText == string.Empty)
                return new T[0];
            List<List<string>> Csv = new List<List<string>>(); //Csv File (as string)
            StringReader fileReader = new StringReader(CsvText, Aggregate, LineDelimiter);
            while (!fileReader.EOF)
            {
                Csv.Add(new List<string>());
                StringReader lineReader = new StringReader(fileReader.Read(), Aggregate, Delimiter);
                //Add Cell (replace double Aggregate with single)
                while (!lineReader.EOF)
                    Csv.Last().Add(lineReader.Read()?.Replace(Aggregate + Aggregate, Aggregate));
            }
            //Get the Schema for this file
            CsvSchema<T> DocumentSchema = new CsvSchema<T>(Csv[0]);
            //Serialize into T
            List<T> list = new List<T>();
            for (int i = 1; i < Csv.Count; i++)
            {
                list.Add(new T());
                foreach (CsvColumn column in DocumentSchema.ColumnSchema)
                    if (column.ColumnNumber < Csv[i].Count)
                        column.Property.SetValue(list.Last(),Csv[i][column.ColumnNumber]);
            }
            return list.ToArray();
        }

        public string Serialize(T[] data)
        {
            List<string> lines = new List<string>();
            //Add the Header Row
            lines.Add(string.Join(Delimiter,
                from c in Schema.ColumnSchema
                orderby c.ColumnNumber
                select ConvertToCsvCell(c.ColumnName)));
            //Add all lines
            //For Each line convert it to a string
            //For Each cell use ConvertToCsvCell
            lines.AddRange(
                from t in data
                select string.Join(Delimiter,
                    from e in Schema.ColumnSchema
                    orderby e.ColumnNumber
                    select ConvertToCsvCell(e.Property.GetValue(t)?.ToString() ?? string.Empty)
                ));
            //Ensure it ends with a LineDelimiter
            return string.Join(LineDelimiter, lines) + LineDelimiter;
        }

        /// <summary>
        /// Converts <paramref name="val"/>
        /// to a Csv-formatted Cell
        /// </summary>
        /// <param name="val">Value to Convert</param>
        string ConvertToCsvCell(string val)
        {
            if (!val.Contains(Aggregate) && !val.Contains(Delimiter) && !val.Contains(LineDelimiter))
                return val;
            return $"{Aggregate}{val.Replace(Aggregate, Aggregate + Aggregate)}{Aggregate}";
        }
    }
}
