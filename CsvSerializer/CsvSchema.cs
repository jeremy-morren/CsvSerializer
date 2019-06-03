using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CsvDocument
{
    /// <summary>
    /// Set Csv Column Properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvColumnAttribute : Attribute
    {
        /// <summary>
        /// Set Column Name
        /// </summary>
        /// <param name="columnName">Column Name</param>
        public CsvColumnAttribute(string columnName)
        {
            ColumnName = columnName;
            ColumnNumber = -1;
        }

        /// <summary>
        /// Sets Csv column Number
        /// </summary>
        /// <param name="columnNumber">0 index based Column Number</param>
        public CsvColumnAttribute(int columnNumber)
        {
            ColumnNumber = columnNumber;
        }

        /// <summary>
        /// Sets Csv Column Name and Number
        /// </summary>
        /// <param name="columnName">Csv Column Name</param>
        /// <param name="columnNumber">0 index based Column Number</param>
        public CsvColumnAttribute(string columnName, int columnNumber)
        {
            ColumnName = columnName;
            ColumnNumber = columnNumber;
        }

        /// <summary>
        /// CSV Column Name
        /// </summary>
        internal string ColumnName { get; private set; }

        /// <summary>
        /// Csv Column Number
        /// </summary>
        /// <remarks>
        /// Will be used if there are 2 instances
        /// of <see cref="ColumnName"/>
        /// </remarks>
        internal int ColumnNumber { get; private set; }
    }

    /// <summary>
    /// Indicates that this column
    /// should be ignore when class is
    /// serialized to Csv
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvIgnoreAttribute : Attribute
    {
        
    }

    /// <summary>
    /// Represent a Csv Column
    /// </summary>
    public class CsvColumn
    {
        /// <summary>
        /// Sets <see cref="ColumnName"/>
        /// and <see cref="ColumnNumber"/>
        /// from <paramref name="property"/> attributes
        /// </summary>
        /// <param name="property">class property</param>
        public CsvColumn(PropertyInfo property)
        {
            if (property.MemberType != MemberTypes.Property)
                throw new ArgumentOutOfRangeException(nameof(property), null,
                    "Property must be a class property");
            Property = property;
            ColumnName = property.GetCsvColumnName();
            ColumnNumber = property.GetCsvColumnNumber();
        }

        /// <summary>
        /// Initializes class with Column Name,
        /// Column Number and class Property
        /// </summary>
        /// <param name="property">class property</param>
        /// <param name="columnNumber">Column Number (0-index based)</param>
        /// <param name="columnName">Column Number</param>
        public CsvColumn(PropertyInfo property, int columnNumber, string columnName)
            : this(property)
        {
            ColumnNumber = columnNumber;
            ColumnName = columnName;
        }

        

        /// <summary>
        /// Sets <see cref="ColumnName"/>
        /// and <see cref="ColumnNumber"/>
        /// based on <paramref name="property"/> attributes
        /// and <paramref name="HeaderRow"/>
        /// </summary>
        /// <param name="property">class property</param>
        /// <param name="HeaderRow">Csv Header Row</param>
        /// <param name="strict">
        /// Indicates whether to throw exception if no matching
        /// column is found in <paramref name="HeaderRow"/>
        /// </param>
        internal CsvColumn(PropertyInfo property, List<string> HeaderRow, bool strict)
            : this(property)
        {
            IEnumerable<string> headers = HeaderRow.Where(s => s == ColumnName);
            if (headers.Count() == 0)
            {
                if (strict)
                {
                    throw new ArgumentOutOfRangeException(nameof(ColumnName), ColumnName,
                        "Csv Column Name does not exist in Csv Headers.  " +
                        "Check if you are using the Correct Style");
                }
                else
                    ColumnNumber = -1; //Indicate we are ignoring this column
            }   
            else if (headers.Count() == 1)
            {
                ColumnNumber = HeaderRow.IndexOf(ColumnName);
            }
            else
            {
                //Something is wrong
                if (ColumnNumber == -1)
                    throw new ArgumentOutOfRangeException(nameof(property), null,
                        "Multiple Column Headers in file and no column number set");
                if (HeaderRow[ColumnNumber] != ColumnName)
                    throw new ArgumentOutOfRangeException(nameof(ColumnNumber), null,
                        "Header name at Column Number does not match property Column Name");
            }
        }

        

        /// <summary>
        /// class Property for this Csv Column
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// 0-index based Column Number
        /// </summary>
        /// <remarks>
        /// When no column is set then defaults to -1
        /// </remarks>
        public int ColumnNumber { get; private set; } = -1;

        /// <summary>
        /// Csv Column Name
        /// </summary>
        public string ColumnName { get; private set; }
    }

    /// <summary>
    /// Helper class for PropertyInfo Extensions
    /// </summary>
    static internal class PropertyInfoExtensions
    {
        /// <summary>
        /// Gets the Csv Column Name
        /// </summary>
        /// <returns>
        /// <see cref="CsvColumnAttribute.ColumnName"/> 
        /// if present for <paramref name="property"/>,
        /// otherwise PropertyInfo.Name
        /// </returns>
        public static string GetCsvColumnName(this PropertyInfo property)
        {
            CsvColumnAttribute attr = property.GetCustomAttribute<CsvColumnAttribute>(true);
            return attr == null || attr.ColumnName == null ? property.Name : attr.ColumnName;
        }

        /// <summary>
        /// Gets the Csv Column Number
        /// to use when serializing to Csv
        /// </summary>
        /// <returns>
        /// <see cref="CsvColumnAttribute.ColumnNumber"/>
        /// if present against <paramref name="property"/>,
        /// otherwise -1
        /// </returns>
        public static int GetCsvColumnNumber(this PropertyInfo property)
        {
            CsvColumnAttribute attr = property.GetCustomAttribute<CsvColumnAttribute>(true);
            return attr == null ? -1 : attr.ColumnNumber;
        }

        /// <summary>
        /// Gets a value indicating whether
        /// <paramref name="property"/> should
        /// be ignored when serializing to Csv
        /// </summary>
        /// <returns>
        /// true if <see cref="CsvIgnoreAttribute"/>
        /// is set against <paramref name="property"/>,
        /// otherwise false
        /// </returns>
        public static bool CsvIgnore(this PropertyInfo property)
            => property.GetCustomAttribute<CsvIgnoreAttribute>(true) != null;
    }
}
