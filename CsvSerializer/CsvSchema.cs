using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CsvDocument
{
    /// <summary>
    /// Represents CSV Column Properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    class CsvColumnAttribute : Attribute
    {
        /// <summary>
        /// Represents a CSV Column
        /// </summary>
        /// <param name="columnName">Column Name</param>
        public CsvColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        /// <summary>
        /// CSV Column Name
        /// </summary>
        internal string ColumnName { get; private set; }
    }

    /// <summary>
    /// Represents a Csv File Schema for <typeparamref name="T"/>
    /// </summary>
    public class CsvSchema<T>
    {
        /// <summary>
        /// Creates Schema for <typeparamref name="T"/>
        /// from an existing Csv File
        /// using <paramref name="HeaderRow"/>
        /// </summary>
        /// <param name="HeaderRow">Header Row of CSV File</param>
        public CsvSchema(List<string> HeaderRow)
        {
            if (HeaderRow.Distinct().Count() != HeaderRow.Count())
                throw new ArgumentOutOfRangeException(nameof(HeaderRow), null, "Duplicate Column Names");
            ColumnSchema = new List<CsvColumn>();
            PropertyInfo[] propertyInfo = typeof(T).GetProperties();
            foreach (PropertyInfo property in propertyInfo)
            {
                string columnName = GetColumnName(property);
                int index = HeaderRow.IndexOf(columnName);
                if (index == -1)
                    throw new ArgumentOutOfRangeException(nameof(columnName), columnName, "No Column Found in CSV Document");
                ColumnSchema.Add(new CsvColumn(property, index, columnName));
            }
        }

        /// <summary>
        /// Creates Schema for <typeparamref name="T"/>
        /// </summary>
        public CsvSchema()
        {
            ColumnSchema = new List<CsvColumn>();
            PropertyInfo[] propertyInfo = typeof(T).GetProperties();
            int i = 0;
            foreach (PropertyInfo property in propertyInfo)
                ColumnSchema.Add(new CsvColumn(property, i++, GetColumnName(property)));
        }

        /// <summary>
        /// Column Schema
        /// </summary>
        internal List<CsvColumn> ColumnSchema { get; private set; }

        /// <summary>
        /// Helper function to get the Column Name
        /// </summary>
        /// <param name="propertyInfo">Property to get Column Name for</param>
        /// <remarks>
        /// If the Property has CsvColumnAttribute 
        /// </remarks>
        string GetColumnName(PropertyInfo propertyInfo)
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

    internal class CsvColumn
    {
        public CsvColumn(PropertyInfo property, int columnNumber, string columnName)
        {
            Property = property;
            ColumnNumber = columnNumber;
            ColumnName = columnName;
        }

        public PropertyInfo Property { get; private set; }

        public int ColumnNumber { get; private set; }

        public string ColumnName { get; private set; }
    }

}
