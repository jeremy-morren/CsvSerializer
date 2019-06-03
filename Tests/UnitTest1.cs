using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvDocument;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Tests DeSerializing and Serializing <see cref="CSV"/>
        /// And Asserts that the result is equal
        /// </summary>
        [TestMethod]
        public void TestDeSerializeAndSerialize()
        {
            CsvSerializer<CsvItem> serializer = new CsvSerializer<CsvItem>(); //New Serializer
            TextReader reader = new StringReader(CSV); //Create a Stream with Csv Text
            IEnumerable<CsvItem> csvItems = serializer.DeSerialize(reader); //Deserialize Csv Text
            TextWriter writer = new StringWriter(); //Create a new Stream to write Csv Text to
            serializer.Serialize(writer, csvItems); //Serialize Items back to Csv
            string s = writer.ToString();
            Assert.AreEqual<string>(writer.ToString(), CSV);
        }

        /// <summary>
        /// Tests the <see cref="CsvIgnoreAttribute"/>
        /// </summary>
        [TestMethod]
        public void TestIgnore()
        {
            CsvSerializer<CsvItem> serializer = new CsvSerializer<CsvItem>(); //New Serializer
            List<CsvItem> csvItems = new List<CsvItem>();
            TextWriter writer = new StringWriter();
            serializer.Serialize(writer, csvItems); //Serialize Items back to Csv
            Assert.IsFalse(writer.ToString().Contains("Ignore"));
        }

        /// <summary>
        /// Tests that Deserializing <see cref="CSV"/>
        /// is equal to <see cref="CsvItems"/>
        /// </summary>
        [TestMethod]
        public void TestDeserialize()
        {
            CsvSerializer<CsvItem> serializer = new CsvSerializer<CsvItem>(); //New Serializer
            TextReader reader = new StringReader(CSV); //Create a Stream with Csv Text
            IEnumerable<CsvItem> csvItems = serializer.DeSerialize(reader); //Deserialize Csv Text
            CollectionAssert.AreEqual(csvItems.ToList(), CSVItems.ToList());
        }

        /// <summary>
        /// Sample CSV
        /// </summary>
        static readonly string CSV
            = "Boolean Column,Integer Column,Text Column\r\nTrue,5,Row 1\r\nFalse,1,\"Row, 2\"\r\n";

        /// <summary>
        /// Deserialized Items for <see cref="CSV"/>
        /// </summary>
        static readonly IEnumerable<CsvItem> CSVItems = 
            new List<CsvItem>()
            {
                new CsvItem { Boolean = true, Integer = 5, Text = "Row 1" },
                new CsvItem { Boolean = false, Integer = 1, Text = "Row, 2" }
            };
    }

    class CsvItem
    {
        [CsvColumn("Boolean Column", 0)]
        public bool Boolean { get; set; }

        [CsvColumn("Integer Column", 1)]
        public int Integer { get; set; }

        [CsvColumn("Text Column", 2)]
        public string Text { get; set; }

        [CsvIgnore]
        public decimal Ignore { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CsvItem);
        }

        public bool Equals(CsvItem item)
        {
            if (ReferenceEquals(item, null))
                return false;
            if (ReferenceEquals(this, item))
                return true;
            return item.Boolean == Boolean
                && item.Integer == Integer
                && item.Text == Text
                && item.Ignore == Ignore;
        }
    }
}
