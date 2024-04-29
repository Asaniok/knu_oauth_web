namespace KNUAuthMYSQLConnector
{
    public class dbUser
        {
            public int id { get; set; }
            public string user { get; set; }
            public string email { get; set; }
            public string surname { get; set; }
            public string firstname { get; set; }
            public string middlename { get; set; }
        public dbUser(int Id, string User, string Email, string Surname, string Firstname, string Middlename)
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
