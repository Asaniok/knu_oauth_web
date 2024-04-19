using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNUAuthMYSQLConnector
{
    public class Connector
    {
        public string server { get; set; }
        public int port { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string database { get; set; }
    }
}
