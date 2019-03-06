using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CsvDocument
{
    public class CsvSerializer<T> where T : class, new()
    {
        public CsvSerializer()
        {
            Delimiter = ",";
            Aggregate = "\"";
            LineDelimiter = "\r\n";
        }

        public CsvSerializer(string delimiter, string aggregate, string lineDelimiter) : this()
        {
            Delimiter = delimiter;
            Aggregate = aggregate;
            LineDelimiter = lineDelimiter;
        }

        public string Delimiter { get; set; }
        public string Aggregate { get; set; }
        public string LineDelimiter { get; set; }

        public Type ObjectType { get => typeof(T); }

        public CsvSchema<T> Schema { get; private set; }

        public T[] DeSerialize(string Text)
        {
            List<List<string>> Csv = new List<List<string>>(); //Csv File (as string)
            StringReader fileReader = new StringReader(Text, Aggregate, LineDelimiter);
            while (!fileReader.EOF)
            {
                Csv.Add(new List<string>());
                StringReader lineReader = new StringReader(fileReader.Read(), Aggregate, Delimiter);
                //Add Cell (replace double Aggregate with single)
                while (!lineReader.EOF)
                    Csv.Last().Add(lineReader.Read()?.Replace(Aggregate + Aggregate, Aggregate));
            }
            Schema = new CsvSchema<T>(Csv[0]);
            //Serialize into T
            List<T> list = new List<T>();
            for (int i = 1; i < Csv.Count; i++)
            {
                list.Add(new T());
                foreach (CsvColumn column in Schema.ColumnSchema)
                    if (column.ColumnNumber < Csv[i].Count)
                        column.Property.SetValue(list.Last(),Csv[i][column.ColumnNumber]);
            }
            return list.ToArray();
        }

        private string GetCellValue(string Text, int startIndex, int endIndex)
        {
            //Get the Raw Cell Value
            string cellValue = endIndex < startIndex ? string.Empty //Empty Cell
                    : Text.Splice(startIndex, endIndex);
            //If it is surrounded by aggregate then trim it
            if (cellValue.StartsWith(Aggregate) && cellValue.EndsWith(Aggregate))
                cellValue = cellValue.Splice(Aggregate.Length, cellValue.Length - Aggregate.Length - 1);
            //Add the Cell (replace Double aggregate with single)
            return cellValue.Replace(Aggregate + Aggregate, Aggregate);
        }

        private bool LineEnd(string Text, int index)
        {
            //Check if we are the end of the file
            if (index == Text.Length - LineDelimiter.Length - 1)
                return true;
            //Check if the next characters equals LineDelimiter
            return Text.Substring(index + 1, LineDelimiter.Length) == LineDelimiter;
        }

        [CsvColumn("Test")]
        public string str { get; set; }
    }

    /// <summary>
    /// Represents CSV Column Properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    class CsvColumnAttribute : Attribute
    {
        public CsvColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }
        public string ColumnName { get; private set; }
    }

    public class CsvSchema<T>
    {
        public CsvSchema(List<string> HeaderRow)
        {
            if (HeaderRow.Distinct().Count() != HeaderRow.Count())
                throw new ArgumentOutOfRangeException(nameof(HeaderRow), null, "Duplicate Column Names");
            ColumnSchema = new List<CsvColumn>();
            PropertyInfo[] propertyInfo =typeof(T).GetProperties();
            foreach (PropertyInfo property in propertyInfo)
            {
                string columnName = GetColumnName(property);
                int index = HeaderRow.IndexOf(columnName);
                if (index == -1)
                    throw new ArgumentOutOfRangeException(nameof(columnName), columnName, "No Column Found in CSV Document");
                ColumnSchema.Add(new CsvColumn(property, index));
            }
        }

        public List<CsvColumn> ColumnSchema { get; private set; }

        public string GetColumnName(PropertyInfo propertyInfo)
        {
            try
            {
                return propertyInfo.GetCustomAttribute<CsvColumnAttribute>(true).ColumnName;
            }
            catch (ArgumentNullException)
            {
                return propertyInfo.Name;
            }
        }
    }

    class CsvColumn
    {
        public CsvColumn(PropertyInfo property, int columnNumber)
        {
            Property = property;
            ColumnNumber = columnNumber;
        }

        public PropertyInfo Property { get; private set; }

        public int ColumnNumber { get; private set; }
    }

}
