using System;
using System.Collections.Generic;
using System.Text;

namespace CommonUtilitesForAll.Models
{
    public class DataSetDetail
    {
        public IList<Dept> dept { get; set; }
        public IList<Employee> employee { get; set; }
        public int num { get; set; }
        public string name { get; set; }
    }
    public class Dept
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class Employee
    {
        public int id { get; set; }
        public string ename { get; set; }
        public int managerId { get; set; }
    }
}
