using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Security.Cryptography;

namespace KNUAuthMYSQLConnector
{
    public class MySQL
    {
        public static bool Initialize(Connector c) {
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database};charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * from users;";
                cmd.ExecuteNonQuery();
                conn.CloseAsync();
            }
            return true; 
        }
        public static bool Execute(Connector c, string exec)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database};charset=utf8";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = exec;
                cmd.ExecuteNonQuery();
                conn.CloseAsync();
            }
            catch { return false; }
            return true;
        }
        public static bool AddTokenB(Connector c, string token, int user, string refresh_token, int exptime, string type, string scope = "")
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database} ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO tokens (token,exptime,refresh_token,scope,type,user) VALUES ('{token}',{exptime},'{refresh_token}','{scope}','{type}',{user})";
            cmd.ExecuteNonQuery();
            conn.CloseAsync();
            return true;
        }
        public static bool refreshTokenR(Connector c, string refresh_token, string access_token, string new_refresh_token, int exptime)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database} ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            try
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = $"UPDATE tokens SET token='{access_token}', exptime={exptime}, refresh_token='{new_refresh_token}' WHERE refresh_token='{refresh_token}'; commit;";
                int count = cmd.ExecuteNonQuery();
                conn.CloseAsync();
                if (count == 0) { return false; }
            }catch (Exception) { return false; }
            return true;
        }
        public static bool firstOfDefault(Connector c, string username)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database} ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            try
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT count(user) FROM users WHERE USER='{username}';";
                int count = int.Parse(cmd.ExecuteScalar().ToString());
                conn.CloseAsync();
                if (count != 0) { return false; }
            }
            catch (Exception) { return false; }
            conn.CloseAsync();
            return true;
        }
        public static bool checkEmailUnique(Connector c, string email)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            try
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT count(email) FROM users WHERE email='{email}';";
                int count = int.Parse(cmd.ExecuteScalar().ToString());
                conn.CloseAsync();
                if (count != 0) { return false; }
            }
            catch (Exception) { return false; }
            conn.CloseAsync();
            return true;
        }
        public static bool addUser(Connector c, dbUser user, string password, string status = "user")
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO users (user,password,email,surname,lastname,firstname,status) VALUES ('{user.user}','{password}','{user.email}','{user.surname}','{user.lastname}','{user.firstname}','{status}');";
            int count = cmd.ExecuteNonQuery();
            conn.CloseAsync();
            if (count == 0) { return false; }
            return true;
        }
        public static int checkAuth(Connector c, string username, string password)
        {
            if (c == null) { return 0; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            try
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT id FROM users WHERE USER='{username}' and password='{password}';";
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int id = int.Parse(reader["id"].ToString());
                    conn.CloseAsync();
                    if (id != 0) { return id; }else { return 0; }
                }
            }
            catch (Exception) { return 0; }
            conn.CloseAsync(); return 0;
        }
        public static bool checkClient(Connector c, int client_id)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT id FROM clients WHERE id={client_id};";
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int id = int.Parse(reader["id"].ToString());
                conn.CloseAsync();
                if (id != client_id) { return false; } else { conn.CloseAsync(); return true; }
            }
            return false;
        }
        public static bool addCode(Connector c, string code, int exptime, string scope, int client_id, int user_id)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO codes (code,exptime,scope,client_id,user_id) VALUES ('{code}',{exptime},'{scope}',{client_id},{user_id});";
            int count = cmd.ExecuteNonQuery();
            conn.CloseAsync();
            if (count == 0) {return false; }
            return true;
        }
        public static string checkCode(Connector c, string code, int client_id)
        {
            if (c == null) { return "IE01"; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT count(code) as c FROM codes WHERE code='{code}';";
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int count = int.Parse(reader["c"].ToString());
                if (count == 0) {  conn.CloseAsync(); return "IE01"; }
            }
            conn.CloseAsync();
            conn.Open();
            cmd = conn.CreateCommand();
            cmd.CommandText = $"DELETE FROM codes WHERE (UNIX_TIMESTAMP(CURRENT_TIMESTAMP)-(UNIX_TIMESTAMP(created_at))>exptime);" +
                $" SELECT ((UNIX_TIMESTAMP(created_at)-UNIX_TIMESTAMP(CURRENT_TIMESTAMP))+exptime) as time, scope FROM codes WHERE client_id={client_id} and code='{code}';";
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int time = int.Parse(reader["time"].ToString());
                string scope = reader["scope"].ToString();
                Execute(c, $"DELETE FROM codes WHERE client_id={client_id} and code='{code}';");
                conn.CloseAsync();
                if (time<=0) { return "TS_EXPIRED"; }
                else if(time>0) { return scope; }
            }
            else{  return "IE01"; }
            return "IE01";
        }
        public static bool checkToken(Connector c, string token, int user_id)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT count(token) as c FROM tokens WHERE token='{token}';";
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int count = int.Parse(reader["c"].ToString());
                if (count == 0) { conn.CloseAsync(); return false; }
            }
            conn.CloseAsync();
            conn.Open();
            cmd = conn.CreateCommand();
            cmd.CommandText = $"DELETE  FROM tokens where  (UNIX_TIMESTAMP(CURRENT_TIMESTAMP)-(UNIX_TIMESTAMP(created_at))>2*exptime);" +
                $" SELECT ((UNIX_TIMESTAMP(created_at)-UNIX_TIMESTAMP(CURRENT_TIMESTAMP))+exptime) as time FROM tokens WHERE client_id={user_id} and token='{token}';";
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int time = int.Parse(reader["time"].ToString());
                Execute(c, $"DELETE FROM tokens WHERE user={user_id} and token='{token}';");
                conn.CloseAsync();
                if (time <= 0) { return false; }
                else if (time > 0) { return true; }
            }
            else { return false; }
            return true;
        }
        public static string getActualToken(Connector c, int user_id)
        {
            if (c == null) { return "IE01"; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT token FROM tokens  WHERE (UNIX_TIMESTAMP(CURRENT_TIMESTAMP)-(UNIX_TIMESTAMP(created_at))<exptime) AND user={user_id};";
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string token = reader["token"].ToString();
                conn.CloseAsync();
                if (token == null) { return "KAS_ERROR_NULL_TOKEN"; }
                else {return token; }
            }
            return "IE01";
        }
        public static int getUserIdByCode(Connector c, string code)
        {
            if (c == null) { return 0; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT user_id FROM codes WHERE `code`='{code}';";
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int user_id = int.Parse(reader["user_id"].ToString());
                conn.CloseAsync();
                if (user_id==0) { return 0; }
                else { return user_id; }
            }
            return 0;
        }
        public static string getUserNameByToken(Connector c, string token)
        {
            if (c == null) { return "IE01"; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT U.`user` FROM tokens AS T, users AS U WHERE T.token= '{token}' AND T.`user`=U.id;";
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string username = reader["user"].ToString();
                conn.CloseAsync();
                if (username == null) { return "IE01"; }
                else { return username; }
            }
            return "IE01";
        }
        public static dbUser getUserByToken(Connector c, string token, string access = null)
        {
            if (c == null) { return null; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            if(access!=null)
                cmd.CommandText = $"SELECT U.`user`,U.id,U.email,U.surname,U.lastname,U.firstname FROM tokens AS T, users AS U WHERE T.token= '{token}' AND T.`user`=U.id AND T.scope='{access}';";
            else
                cmd.CommandText = $"SELECT U.`user`,U.id,U.email,U.surname,U.lastname,U.firstname FROM tokens AS T, users AS U WHERE T.token= '{token}' AND T.`user`=U.id;";
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                try
                {
                    dbUser user = new dbUser
                    {
                        id = int.Parse(reader["id"].ToString()),
                        email = reader["email"].ToString(),
                        user = reader["user"].ToString(),
                        surname = reader["surname"].ToString(),
                        firstname = reader["firstname"].ToString(),
                        lastname = reader["lastname"].ToString()
                    };
                    return user;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        public static List<listUser> adminGetUsers(Connector c, int limit = 10, int startid=1)
        {
            if (c == null) { return null; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT id, user, email, surname, firstname, lastname FROM users WHERE id>={startid} LIMIT {limit};";
            MySqlDataReader reader = cmd.ExecuteReader();
            List<listUser> users = new List<listUser>();
            while(reader.Read())
            {
                users.Add(new listUser(
                        reader.GetInt32("id"),
                        reader.GetString("user"),
                        reader.GetString("email"),
                        reader.GetString("surname"),
                        reader.GetString("firstname"),
                        reader.GetString("lastname")
                    ));
            }
            return users;
        }
        public static List<listUser> adminGetUserByFilter(Connector c, int? limit, string? user, string? email, string? surname, string? firstname, string? lastname)
        {
            if (c == null) { return null; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            int count = 0;
            string Command = $"SELECT id, user, email, surname, firstname, lastname FROM users WHERE ";
            if (user != null) { Command += $" user LIKE '%{user}%'"; count++; }
            if (email != null) { if (count != 0) { Command += " AND "; } Command += $" email LIKE '%{email}%'"; count++; }
            if (surname != null) { if (count != 0) { Command += " AND "; } Command += $" surname LIKE '%{surname}%'"; count++; }
            if (firstname != null) { if (count != 0) { Command += " AND "; } Command += $" firstname LIKE '%{firstname}%'"; count++; }
            if (lastname != null) { if (count != 0) { Command += " AND "; } Command += $" lastname LIKE '%{lastname}%'"; }
            Command += $" ORDER BY ID ASC LIMIT {limit};";
            cmd.CommandText = Command;
            MySqlDataReader reader = cmd.ExecuteReader();
            List<listUser> users = new List<listUser>();
            while (reader.Read())
            {
                users.Add(new listUser(
                        reader.GetInt32("id"),
                        reader.GetString("user"),
                        reader.GetString("email"),
                        reader.GetString("surname"),
                        reader.GetString("firstname"),
                        reader.GetString("lastname")
                    ));
            }
            return users;
        }
        public static listUser adminGetUserById(Connector c, int? id)
        {
            if (c == null) { return null; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            string Command = $"SELECT id, user, email, surname, firstname, lastname FROM users WHERE id={id};";
            cmd.CommandText = Command;
            MySqlDataReader reader = cmd.ExecuteReader();
            listUser user = null;
            while (reader.Read())
            {
                user = new listUser(
                        reader.GetInt32("id"),
                        reader.GetString("user"),
                        reader.GetString("email"),
                        reader.GetString("surname"),
                        reader.GetString("firstname"),
                        reader.GetString("lastname")
                    );
            }
            return user;
        }
        public static bool adminEditUserById(Connector c, dbUser user, int? id)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            string Command = $"UPDATE users SET user='{user.user}', email='{user.email}', surname='{user.surname}', firstname='{user.firstname}', lastname='{user.lastname}' WHERE id={id};";
            cmd.CommandText = Command;
            int count = cmd.ExecuteNonQuery();
            conn.CloseAsync();
            if (count == 0) { return false;}
            return true;
        }
        public static bool checkUserAdmin(Connector c, string token)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}  ;charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT U.`status` FROM tokens AS T, users AS U WHERE T.token= '{token}' AND T.`user`=U.id;";
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string status = reader["status"].ToString();
                conn.CloseAsync();
                if (status == "admin") { return true; }
                else { return false; }
            }
            return false;
        }

    }
}