using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvDocument
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a string
        /// starting at <paramref name="startIndex"/>
        /// and ending at <paramref name="endIndex"/>
        /// </summary>
        /// <param name="startIndex">Starting Index</param>
        /// <param name="endIndex">Ending Index</param>
        public static string Splice(this string str, int startIndex, int endIndex)
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

    class Product
    {
        public string Groupcode { get; set; }
        public string Productcode { get; set; }
        public string Productdescription { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string delimiter = ",";
            string aggregate = "\"";
            string lineEnd = "\r\n";
            string csv = System.IO.File.ReadAllText("Test.csv");
            //Ensure it ends with lineEnd
            if (!csv.EndsWith(lineEnd))
                csv += lineEnd;
            bool inAggregate = false; //Tracks whether we are currently within an Aggregate 'string'
            StringReader reader = new StringReader(csv);
            List<CsvCell> cells = new List<CsvCell>();
            CsvCell currentCell = new CsvCell(0);
            while (!reader.EOF)
            {
                //If we are in aggregate skip straight to the next occurrence
                string str = inAggregate ? reader.Read(aggregate) : reader.Read(new[] { aggregate, delimiter, lineEnd });
                if (str.StartsWith(aggregate) || str.EndsWith(aggregate))
                    inAggregate = !inAggregate;
                else if (!inAggregate)
                {
                    //CurrentIndex is always the first Character of the next cell
                    int endLength = str.EndsWith(delimiter) ? delimiter.Length : lineEnd.Length;
                    currentCell.EndIndex = reader.CurrentIndex - (endLength + 1);
                    cells.Add(currentCell);
                    currentCell = new CsvCell(reader.CurrentIndex);
                }
            }
            reader = new StringReader(csv);
            List<List<string>> Csv = new List<List<string>>();
            List<string> currentLine = new List<string>();
            foreach (CsvCell cell in cells)
            {
                string cellValue = cell.EndIndex < cell.StartIndex ? string.Empty //Empty Cell
                    : csv.Splice(cell.StartIndex, cell.EndIndex);
                //If it is surrounded by aggregate then trim it
                if (cellValue.StartsWith(aggregate) && cellValue.EndsWith(aggregate))
                    cellValue = cellValue.Splice(aggregate.Length, cellValue.Length - aggregate.Length - 1);
                //Add the Cell (replace Double aggregate with single)
                currentLine.Add(cellValue.Replace(aggregate + aggregate, aggregate));
                //Check if we are at a line end or End of File
                if (cell.EndIndex == csv.Length - lineEnd.Length - 1 
                    || csv.Substring(cell.EndIndex + 1, lineEnd.Length) == lineEnd)
                {
                    Csv.Add(currentLine);
                    currentLine = new List<string>();
                }  
            }
            List<KeyValuePair<System.Reflection.PropertyInfo, int>> columns = 
                new List<KeyValuePair<System.Reflection.PropertyInfo, int>>();
            System.Reflection.PropertyInfo[] propertyInfo = typeof(Product).GetProperties();
            for (int i = 0; i < Csv[0].Count; i++)
                columns.Add(new KeyValuePair<System.Reflection.PropertyInfo, int>
                    (propertyInfo.First(e => e.Name == Csv[0][i]), i));
            List<Product> products = new List<Product>();
            for (int i = 1; i < Csv.Count;i++)
            {
                Product product = new Product();
                foreach (var x in columns)
                    x.Key.SetValue(product, Csv[i][x.Value]);
                products.Add(product);
            }
        }
    }
}
