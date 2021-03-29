using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreUtility
{
    public class ExcelTestRunner
    {
        #region Members
        private readonly IExcelProcessor _excelUtil;
        private static IEnumerable<KeyValuePair<string, string>> _cellPropertyMap = null;
        #endregion


        #region Constructor
        public ExcelTestRunner(IExcelProcessor excelUtil)
        {
            _excelUtil = excelUtil;

        }

        static ExcelTestRunner()
        {
            _cellPropertyMap = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Id", nameof(Employee.Id)),
                new KeyValuePair<string, string>("Name",nameof(Employee.Name)),
                new KeyValuePair<string, string>("DOB",nameof(Employee.DOB)),
                new KeyValuePair<string, string>("Is Admin?",nameof(Employee.IsAdmin)),
                new KeyValuePair<string, string>("Commission",nameof(Employee.Commission))
            };
        }
        #endregion


        #region Public Methods
        public void Run()
        {
            if (File.Exists("Employee.xlsx"))
            {
                Byte[] fileContent = File.ReadAllBytes("Employee.xlsx");
                IEnumerable<Employee> employees = _excelUtil.Process<Employee>(fileContent, _cellPropertyMap);
                foreach (var item in employees)
                {
                    Console.WriteLine(item.ToString());
                }
            }
            else
            {
                Console.WriteLine("Missing File");
            }
        }
        #endregion
    }

    internal class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? DOB { get; set; }
        public bool? IsAdmin { get; set; }
        public double Commission { get; set; }

        public override string ToString()
        {
            return $"{Id} {Name} {DOB} {IsAdmin} {Commission}";
        }
    }
}
