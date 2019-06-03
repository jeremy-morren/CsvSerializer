using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            foreach (PropertyInfo property in Properties)
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
        /// Initializes with Custom Column Schema
        /// </summary>
        /// <param name="columnSchema"><see cref="CsvColumn"/> schema</param>
        public CsvSerializer(List<CsvColumn> columnSchema)
        {
            CsvStyle = new CsvStyle(CsvCharacterStyle.Windows);
            ColumnSchema = columnSchema;
        }

        /// <summary>
        /// Initializes with Custom CSV style and Column Schema
        /// </summary>
        /// <param name="csvStyle">Template CSV Style</param>
        /// <param name="columnSchema"><see cref="CsvColumn"/> columnSchema</param>
        public CsvSerializer(CsvStyle csvStyle, List<CsvColumn> columnSchema) : this(columnSchema)
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
        /// Gets or sets a value
        /// indicating whether an exception
        /// should be thrown when a property
        /// does not have a matching column found
        /// in the CSV File (defaults to false)
        /// </summary>
        public bool Strict { get; set; } = false;

        /// <summary>
        /// DeSerializes text read from <paramref name="FileName"/>
        /// to <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="FileName">Path to CSV File</param>
        /// <param name="UseFirstRowAsHeaders">
        /// When true, the First row of the CSV File will be used as column headers.
        /// When false, the Column Index defined in <see cref="CsvColumnAttribute"/> will be used.
        /// </param>
        /// <returns></returns>
        public IEnumerable<T> DeSerialize(string FileName, bool UseFirstRowAsHeaders = true)
            => DeSerializeText(System.IO.File.ReadAllText(FileName), UseFirstRowAsHeaders);

        /// <summary>
        /// DeSerializes text of <paramref name="TextReader"/>
        /// to <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="TextReader">Source <see cref="System.IO.TextReader"/></param>
        /// <param name="UseFirstRowAsHeaders">
        /// When true, the First row of the CSV File will be used as column headers.
        /// When false, the Column Index defined in <see cref="CsvColumnAttribute"/> will be used.
        /// </param>
        /// <returns><see cref="IEnumerable{T}"/> with values from <paramref name="TextReader"/></returns>
        public IEnumerable<T> DeSerialize(System.IO.TextReader TextReader, bool UseFirstRowAsHeaders = true)
            => DeSerializeText(TextReader.ReadToEnd(), UseFirstRowAsHeaders);

        /// <summary>
        /// Deserializes <paramref name="CsvText"/>
        /// in Array of <typeparamref name="T"/>
        /// </summary>
        /// <param name="CsvText">Csv File Text</param>
        /// <param name="UseFirstRowAsHeaders">
        /// When true, the First row of the CSV File will be used as column headers.
        /// When false, the Column Index defined in <see cref="CsvColumnAttribute"/> will be used.
        /// </param>
        /// <returns>Array of <typeparamref name="T"/> with values from CSV File</returns>
        private IEnumerable<T> DeSerializeText(string CsvText, bool UseFirstRowAsHeaders)
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
                foreach (PropertyInfo property in Properties)
                    ColumnSchema.Add(new CsvColumn(property, Csv[0], Strict));
            }
            //Serialize into T
            List<T> list = new List<T>();
            //Start at first row if not using first row as headers
            for (int i = UseFirstRowAsHeaders ? 1 : 0; i < Csv.Count; i++)
            {
                list.Add(new T());
                //Process all columns that we are not ignoring
                foreach (CsvColumn column in ColumnSchema.Where(e => e.ColumnNumber != -1))
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
        /// Writes CSV Representation of <paramref name="CSVData"/>
        /// to <paramref name="FileName"/>
        /// </summary>
        /// <param name="FileName">Filepath to write CSV to</param>
        /// <param name="CSVData"><see cref="IEnumerable{T}"/> of Values to Serialize to CSV</param>
        /// <param name="IncludeHeaderRow">
        /// Indicates whether to add a header row at the top of the file with the Column Names
        /// </param>
        public void Serialize(string FileName, IEnumerable<T> CSVData, bool IncludeHeaderRow = true)
            => System.IO.File.WriteAllText(FileName, Serialize(CSVData, IncludeHeaderRow));

        /// <summary>
        /// Writes CSV Representation of <paramref name="CSVData"/>
        /// to <paramref name="TextWriter"/>
        /// </summary>
        /// <param name="TextWriter"><see cref="System.IO.TextWriter"/> destination to write CSV Text</param>
        /// <param name="CSVData"><see cref="IEnumerable{T}"/> of Values to Serialize to CSV</param>
        /// <param name="IncludeHeaderRow">
        /// Indicates whether to add a header row at the top of the file with the Column Names
        /// </param>
        public void Serialize(System.IO.TextWriter TextWriter, IEnumerable<T> CSVData, bool IncludeHeaderRow = true)
            => TextWriter.Write(Serialize(CSVData, IncludeHeaderRow));

        /// <summary>
        /// Serializes <see cref="IEnumerable{T}"/> to
        /// string representation of Csv file
        /// </summary>
        /// <param name="Data"><see cref="IEnumerable{T}"/> to serialize</param>
        /// <param name="IncludeHeaderRow">
        /// Indicates whether to add a header row at the top of the file with the Column Names
        /// </param>
        /// <returns>Csv String Representation of <paramref name="Data"/></returns>
        private string Serialize(IEnumerable<T> Data, bool IncludeHeaderRow)
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
                from t in Data
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
            if (propertyType == typeof(string))
                return val;
            //return a new instance of propertyType
            if (val == null)
                return Activator.CreateInstance(propertyType);
            //Use Convert
            //It should throw an exception if there is a parsing error
            return Convert.ChangeType(val, propertyType);
            //If we reach this far then we have an unknown type
            throw new InvalidOperationException(
                $"Property Type {propertyType.Name} " +
                $"is not supported at this time. " +
                $"Consider using a code-behind property");
        }

        /// <summary>
        /// Gets the Properties relevant
        /// to the Csv Serializer
        /// </summary>
        /// <remarks>
        /// All Primitive types and <c>string</c>
        /// </remarks>
        IEnumerable<PropertyInfo> Properties
        {
            get
            {
                List<Type> IConvertibleTypes = new List<Type>
                {
                    typeof(byte),
                    typeof(bool),
                    typeof(char),
                    typeof(DateTime),
                    typeof(decimal),
                    typeof(double),
                    typeof(int),
                    typeof(SByte),
                    typeof(Single),
                    typeof(string),
                    typeof(uint)
                };
                //Ensure we are using the underlying type for nullable types
                return
                    from p in typeof(T).GetProperties()
                    where !p.CsvIgnore() &&
                        IConvertibleTypes.Contains(Nullable.GetUnderlyingType(p.PropertyType) != null ?
                                Nullable.GetUnderlyingType(p.PropertyType) : p.PropertyType)
                    select p;
            }
        }
    }
}
