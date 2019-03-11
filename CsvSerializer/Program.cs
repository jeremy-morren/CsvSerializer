using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvDocument
{

    class Product
    {
        [CsvColumn("Product Code",1)]
        public string Code { get; set; }

        [CsvColumn("Group Code",0)]
        public string Groupcode { get; set; }

        [CsvColumn("Product Description",2)]
        public string Description { get; set; }

        [CsvColumn("Instance Number")]
        //[CsvIgnore]
        public int InstanceNumber { get; set; }

        public DateTime Date { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CsvSerializer<Product> csvSerializer = 
                new CsvSerializer<Product>();
            Product[] products = csvSerializer.DeSerialize(
                System.IO.File.ReadAllText("Test.csv"), true);
            csvSerializer.CsvStyle = new CsvStyle(CsvCharacterStyle.WindowsText);
            string csv = csvSerializer.Serialize(products);
            Console.Write(csv);
            Console.ReadKey();
        }
    }
}
