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
            //Initialize the Column Schema
            //with Default Values
            ColumnSchema = new List<CsvColumn>();
            foreach (PropertyInfo property in 
                typeof(T).GetProperties().Where(p => !p.CsvIgnore() && p.PropertyType.IsPrimitive))
                ColumnSchema.Add(new CsvColumn(property));
            CsvStyle = new CsvStyle(CsvCharacterStyle.Windows);
        }

        /// <summary>
        /// Initializes with Custom CSV Style
        /// </summary>
        /// <paramref name="csvStyle">Template Style</paramref>
        public CsvSerializer(CsvStyle csvStyle) : this()
        {
            CsvStyle = csvStyle;
        }

        /// <summary>
        /// Gets or sets the 
        /// Special Characters used when
        /// Serializing and Deserializing
        /// </summary>
        public CsvStyle CsvStyle { get; set; }

        /// <summary>
        /// Gets or sets the Csv Schema for
        /// <typeparamref name="T"/>
        /// </summary>
        public List<CsvColumn> ColumnSchema { get; set; }

        /// <summary>
        /// Deserializes <paramref name="CsvText"/>
        /// in Array of <typeparamref name="T"/>
        /// </summary>
        /// <param name="CsvText">Csv File Text</param>
        /// <param name="UseFirstRowAsHeaders">
        /// Indicates whether the first row of the Csv File should be used as column Headers
        /// </param>
        /// <returns>Array of <typeparamref name="T"/> with values from CSV File</returns>
        public T[] DeSerialize(string CsvText, bool UseFirstRowAsHeaders = true)
        {
            if (!UseFirstRowAsHeaders 
                && ColumnSchema.Count(e => e.ColumnNumber == -1) > 0)
                throw new InvalidOperationException("Column Number not set");
            if (CsvText == string.Empty)
                return new T[0];
            List<List<string>> Csv = new List<List<string>>(); //Csv File (as string)
            StringReader fileReader = new StringReader(CsvText, 
                CsvStyle.Aggregate, CsvStyle.LineDelimiter);
            while (!fileReader.EOF)
            {
                Csv.Add(new List<string>());
                StringReader lineReader = new StringReader(fileReader.Read(),
                    CsvStyle.Aggregate, CsvStyle.Delimiter);
                //Add Cell (replace double Aggregate with single)
                while (!lineReader.EOF)
                    Csv.Last().Add(lineReader.Read()?.Replace(
                        CsvStyle.Aggregate + CsvStyle.Aggregate,
                        CsvStyle.Aggregate));
            }
            if (UseFirstRowAsHeaders)
            {
                //Get the Schema for this file
                ColumnSchema = new List<CsvColumn>();
                foreach (PropertyInfo property in 
                    typeof(T).GetProperties().Where(p => !p.CsvIgnore() && p.PropertyType.IsPrimitive))
                    ColumnSchema.Add(new CsvColumn(property, Csv[0]));
            }
            //Serialize into T
            List<T> list = new List<T>();
            //Start at first row if not using first row as headers
            for (int i = UseFirstRowAsHeaders ? 1 : 0; i < Csv.Count; i++)
            {
                list.Add(new T());
                foreach (CsvColumn column in ColumnSchema)
                {
                    if (column.ColumnNumber < Csv[i].Count)
                    {
                        try
                        {
                            column.Property.SetValue(list.Last(),
                                GetValue(column.Property, Csv[i][column.ColumnNumber]));
                        }
                        catch (System.FormatException)
                        {
                            //Throw parsing error exception
                            throw new InvalidOperationException(
                                $"Error Converting Value to desired format on " +
                                $"Line Number {i}, Column '{column.ColumnName}'/" +
                                $"Column No '{column.ColumnNumber}'. " +
                                $"See inner exception for further details",
                                new InvalidOperationException(
                                    $"Target Type was {column.Property.PropertyType}. " +
                                    $"Value was '{Csv[i][column.ColumnNumber]}'"));
                        }
                    }
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Serializes Array of <typeparamref name="T"/>
        /// string representation of Csv file
        /// </summary>
        /// <param name="data">Array of <typeparamref name="T"/> to serialize</param>
        /// <param name="IncludeHeaderRow">
        /// Indicates whether
        /// to add a header row at the top of the file with the Column Names
        /// </param>
        /// <returns>Csv String Representation of <paramref name="data"/></returns>
        public string Serialize(T[] data, bool IncludeHeaderRow = true)
        {
            List<string> lines = new List<string>();
            //Add the Header Row
            if (IncludeHeaderRow)
            {
                lines.Add(string.Join(CsvStyle.Delimiter,
                    from c in ColumnSchema
                    orderby c.ColumnNumber, c.ColumnName
                    select ConvertToCsvCell(c.ColumnName)));
            }
            //Add all lines
            //For Each line convert it to a string
            //For Each cell use ConvertToCsvCell
            lines.AddRange(
                from t in data
                select string.Join(CsvStyle.Delimiter,
                    from c in ColumnSchema
                    orderby c.ColumnNumber, c.ColumnName
                    select ConvertToCsvCell(c.Property.GetValue(t)?.ToString() ?? string.Empty)
                ));
            //Ensure it ends with a LineDelimiter
            return string.Join(CsvStyle.LineDelimiter, lines) + CsvStyle.LineDelimiter;
        }

        /// <summary>
        /// Converts <paramref name="val"/>
        /// to a Csv-formatted Cell
        /// </summary>
        /// <param name="val">Value to Convert</param>
        string ConvertToCsvCell(string val)
        {
            if (!val.Contains(CsvStyle.Aggregate) &&
                !val.Contains(CsvStyle.Delimiter) &&
                !val.Contains(CsvStyle.LineDelimiter))
            {
                return val;
            }
            //Surround Cell with Aggregate
            //Replace any occurrences with Double Occurrences
            return $"{CsvStyle.Aggregate}" +
                $"{val.Replace(CsvStyle.Aggregate, CsvStyle.Aggregate + CsvStyle.Aggregate)}" +
                $"{CsvStyle.Aggregate}";
        }

        /// <summary>
        /// Converts <paramref name="val"/>
        /// into <see cref="PropertyInfo.PropertyType"/>
        /// </summary>
        /// <param name="property">Property to Convert <paramref name="val"/> to</param>
        /// <param name="val">Value to Convert</param>
        /// <returns>Converted Value</returns>
        object GetValue(PropertyInfo property, string val)
        {
            Type propertyType = property.PropertyType;
            //Handle Null Types
            if (Nullable.GetUnderlyingType(property.PropertyType) != null)
            {
                if (val == string.Empty || val == null)
                    return null;
                else
                    propertyType = Nullable.GetUnderlyingType(property.PropertyType);
            }
            //Use Convert
            //It should throw an exception if there is a parsing error
            Convert.ChangeType(val, propertyType);
            //If we reach this far then we have an unknown type
            throw new InvalidOperationException(
                $"Property Type {property.PropertyType.Name} " +
                $"is not supported at this time. " +
                $"Consider using a code behind property");
        }
    }
}
