using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DatabaseAccess.Models
{
    public class QueryParameterForSqlMapper
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public ParameterDirection ParameterDirection { get; set; }
        public DbType? DbType { get; set; }
        public SqlDbType? SqlDbType { get; set; }
        public int Size { get; set; }
    }
}
