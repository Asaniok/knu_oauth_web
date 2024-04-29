using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNUAuthMYSQLConnector
{
    public class listUser
    {
        public int id { get; set; }
        public string user { get; set; }
        public string email { get; set; }
        public string surname { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public listUser(int Id, string User, string Email, string Surname, string Firstname, string Middlename)
        {
            id = Id;
            user = User;
            email = Email;
            surname = Surname;
            firstname = Firstname;
            middlename = Middlename;
        }
    }
    
}
