using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseAccess.Models
{
    public class DBExecption: Exception
    {
        public DBExecption()
        {

        }
        public DBExecption(string message) : base(message)
        {

        }
    }
}
