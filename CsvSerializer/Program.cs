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
        [CsvColumn("Group Code")]
        public string Groupcode { get; set; }
        [CsvColumn("Product Code")]
        public string Code { get; set; }
        [CsvColumn("Product Description")]
        public string Description { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CsvSerializer<Product> csvSerializer = new CsvSerializer<Product>();
            Product[] products = csvSerializer.DeSerialize(System.IO.File.ReadAllText("Test.csv"));
            
            int i = 0;
        }
    }
}
