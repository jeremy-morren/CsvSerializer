using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CsvDocument
{
    public class CsvSerializer<T>
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

        public T[] DeSerialize(string Text)
        {
            //Ensure it ends with LineDelimiter (to avoid overflow issues)
            if (!Text.EndsWith(LineDelimiter))
                Text += LineDelimiter;
            bool inAggregate = false; //Tracks whether we are currently within an Aggregate 'string'
            StringReader reader = new StringReader(Text);
            List<List<string>> Csv = new List<List<string>>(); //Csv File (as string)
            List<string> currentLine = new List<string>();
            int currentCellStart = 0;
            while (!reader.EOF)
            {
                //If we are in aggregate skip straight to the next occurrence
                //Otherwise Search for the next occurrence of any item
                string cell = inAggregate ? reader.Read(Aggregate) : 
                    reader.Read(new[] { Aggregate, Delimiter, LineDelimiter });
                //If it starts with or ends with Aggregate then change current Status
                if (cell.StartsWith(Aggregate) || cell.EndsWith(Aggregate))
                    inAggregate = !inAggregate;
                else if (!inAggregate)
                {
                    //Get the length of the separation (Delimiter and LineDelimiter may be 2 different lengths)
                    int endLength = cell.EndsWith(Delimiter) ? Delimiter.Length : LineDelimiter.Length;
                    //CurrentIndex is always the first Character of the next cell
                    //Add the Cell
                    currentLine.Add(Text.Splice(currentCellStart, reader.CurrentIndex - endLength - 1));
                    //Reset currentCell
                    currentCellStart = reader.CurrentIndex;
                    //If we have reached the end of a line
                    //then add to overall csv and reset
                    if (LineEnd(Text, reader.CurrentIndex))
                    {
                        Csv.Add(currentLine);
                        currentLine = new List<string>();
                    }
                }
            }
            //Now Serialize into T
            List<KeyValuePair<System.Reflection.PropertyInfo, int>> columns =
                new List<KeyValuePair<System.Reflection.PropertyInfo, int>>();
            System.Reflection.PropertyInfo[] propertyInfo = typeof(T).GetProperties();
            //Get the Column Index of each Property
            for (int i = 0; i < Csv[0].Count; i++)
                columns.Add(new KeyValuePair<System.Reflection.PropertyInfo, int>
                    (propertyInfo.First(e => e.Name == Csv[0][i]), i));
            List<Product> products = new List<Product>();
            for (int i = 1; i < Csv.Count; i++)
            {
                Product product = new Product();
                foreach (var x in columns)
                    x.Key.SetValue(product, Csv[i][x.Value]);
                products.Add(product);
            }
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

    }

    class CsvColumn
}
