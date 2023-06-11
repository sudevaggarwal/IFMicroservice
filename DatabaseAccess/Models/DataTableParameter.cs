using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DatabaseAccess.Models
{
    public class DataTableParameter
    {
        public string ParameterName { get; set; }
        public DataTable DataTable { get; set; }
    }
}
