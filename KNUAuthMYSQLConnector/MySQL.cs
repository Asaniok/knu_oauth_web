using MySql.Data.MySqlClient;

namespace KNUAuthMYSQLConnector
{
    public class MySQL
    {
        public static bool Initialize(Connector c) {
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
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
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = exec;
            cmd.ExecuteNonQuery();
            conn.CloseAsync();
            return true;
        }
        public static bool AddTokenB(Connector c, string token, int user, string refresh_token, int exptime, string type, string scope = "")
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
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
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
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
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
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
        public static bool addUser(Connector c, string username, string password)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO users (user,password) VALUES ('{username}','{password}');";
            int count = cmd.ExecuteNonQuery();
            conn.CloseAsync();
            if (count == 0) { return false; }
            return true;
        }
        public static int checkAuth(Connector c, string username, string password)
        {
            if (c == null) { return 0; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
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
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
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
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO codes (code,exptime,scope,client_id,user_id) VALUES ('{code}',{exptime},'{scope}',{client_id},{user_id});";
            int count = cmd.ExecuteNonQuery();
            conn.CloseAsync();
            if (count == 0) {return false; }
            return true;
        }
        public static bool checkCode(Connector c, string code, int client_id)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT count(code) as c FROM codes WHERE code='{code}';";
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int count = int.Parse(reader["c"].ToString());
                if (count == 0) {  conn.CloseAsync(); return false; }
            }
            conn.CloseAsync();
            conn.Open();
            cmd = conn.CreateCommand();
            cmd.CommandText = $"DELETE FROM codes WHERE (UNIX_TIMESTAMP(CURRENT_TIMESTAMP)-(UNIX_TIMESTAMP(created_at))>exptime);" +
                $" SELECT ((UNIX_TIMESTAMP(created_at)-UNIX_TIMESTAMP(CURRENT_TIMESTAMP))+exptime) as time FROM codes WHERE client_id={client_id} and code='{code}';";
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int time = int.Parse(reader["time"].ToString());
                Execute(c, $"DELETE FROM codes WHERE client_id={client_id} and code='{code}';");
                conn.CloseAsync();
                if (time<=0) { return false; }
                else if(time>0) { return true; }
            }
            else{  return false; }
            return true;
        }
        public static bool checkToken(Connector c, string token, int user_id)
        {
            if (c == null) { return false; }
            string connStr = $"server={c.server};user={c.user};port={c.port};password={c.password};database={c.database}";
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
    }
}