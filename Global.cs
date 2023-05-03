using MySqlConnector;

namespace TokoMAUI
{
    public class Global
    {
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // constanta
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public const string strTitle = "Toko";
        public const string strVersion = "1.0.0";
        public const string strWeb = "www.zaqstore.com";
        public const bool isDev = true;



        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // fields
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public static string strDBName = "";

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // properties
        // -------------------------------------------------------------
        // -------------------------------------------------------------



        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // function
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public async static void showMessage(string strMessage)
        {
            await App.Current.MainPage.DisplayAlert(strTitle, strMessage, "OK");
        }

        public async static void errorMessage(string strMessage)
        {
            if (isDev)
            {
                await App.Current.MainPage.DisplayAlert(strTitle, strMessage, "OK");
            }
        }

        public static MySqlConnectionStringBuilder getConString(string dbName = "")
        {
            if (dbName == "") dbName = strDBName;

            MySqlConnectionStringBuilder conString = new MySqlConnectionStringBuilder();

            conString.Server = "";
            conString.UserID = "";
            conString.Password = "";
            conString.Database = "";
            conString.Port = 3306;
            conString.CharacterSet = "utf8mb4";
            conString.ConnectionTimeout = 15;
            conString.SslMode = MySqlSslMode.None;

            return conString;
        }

        public static bool setDBName(string strToko)
        {
            //get mysql connection string
            MySqlConnectionStringBuilder conString = getConString("");

            try
            {
                // open a connection asynchronously
                var connection = new MySqlConnection(conString.ConnectionString);
                connection.Open();

                //get status
                var command = connection.CreateCommand();
                command.CommandText = "SELECT dbName FROM tbl_toko WHERE userName = ''";
                strDBName = command.ExecuteScalar().ToString();        

                connection.Close();

                return true;
            }
            catch 
            {
                return false;
            }
        }

        public static string getTokoName(string strToko)
        {
            //get mysql connection string
            MySqlConnectionStringBuilder conString = getConString("");

            try
            {
                // open a connection asynchronously
                var connection = new MySqlConnection(conString.ConnectionString);
                connection.Open();

                //get status
                var command = connection.CreateCommand();
                command.CommandText = "SELECT storeName FROM tbl_toko WHERE userName = ''";
                string tokoName = command.ExecuteScalar().ToString();

                connection.Close();

                return tokoName;
            }
            catch
            {
                return "";
            }
        }

        public static string getStatusAndDate()
        {
            //get mysql connection string
            MySqlConnectionStringBuilder conString = getConString();

            try
            {
                string status = "";

                // open a connection asynchronously
                var connection = new MySqlConnection(conString.ConnectionString);
                connection.Open();

                //get status
                var command = connection.CreateCommand();
                command.CommandText = "SELECT status FROM tbl_status";
                status = command.ExecuteScalar().ToString();

                if (status == "Success") status = "Last updated";

                //get date
                command.CommandText = "SELECT dtUpdate FROM tbl_status";
                status = status + ": " + command.ExecuteScalar().ToString();

                connection.Close();

                return status;
            }
            catch (Exception ex)
            {
                errorMessage(ex.Message);
                return "Error!";
            }
        }

        public static string getSaldo()
        {
            //get mysql connection string
            MySqlConnectionStringBuilder conString = getConString();

            try
            {
                // open a connection asynchronously
                var connection = new MySqlConnection(conString.ConnectionString);
                connection.Open();

                //get status
                var command = connection.CreateCommand();
                command.CommandText = "SELECT saldo FROM tbl_toko";
                string saldo = Convert.ToInt32(command.ExecuteScalar()).ToString("N0");

                connection.Close();

                return saldo;
            }
            catch (Exception ex)
            {
                errorMessage(ex.Message);
                return "0";
            }
        }

        public static bool recordExists(string tableName)
        {
            //get mysql connection string
            MySqlConnectionStringBuilder conString = getConString();

            try
            {
                // open a connection asynchronously
                var connection = new MySqlConnection(conString.ConnectionString);
                connection.Open();

                //count
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Count(*) AS total FROM " + tableName;
                var total = Convert.ToInt32(command.ExecuteScalar());

                connection.Close();

                return total > 0 ? true : false;
            }
            catch (Exception ex)
            {
                errorMessage(ex.Message);
                return false;
            }
        }
    }
}
