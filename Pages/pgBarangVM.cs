using MySqlConnector;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TokoMAUI.Pages
{
    public class pgBarangVM : BindProperty
    {
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // fields
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        private ObservableCollection<Barang> _lstBarang;
        private ObservableCollection<Barang> tmpBarang;
        private int page = 2;
        private string _strTitle;

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // properties
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public ICommand comLoad { get; set; }
        public ObservableCollection<Barang> lstBarang { get { return _lstBarang; } set { _lstBarang = value; OnPropertyChanged("lstBarang"); } }
        public string strTitle { get { return _strTitle; } set { _strTitle = value; OnPropertyChanged("strTitle"); } }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public pgBarangVM(string strMode)
        {
            initCommands();
            initList();

            //title
            if (strMode == "Barang Semua") 
            {
                strTitle = "Semua barang";
            }
            else if (strMode == "Barang Habis")
            {
                strTitle = "Barang habis";
            }
            else //cari
            {
                strTitle = "Cari barang";
            }

            getBarang(strMode);
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // Methods
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        private void initCommands()
        {
            comLoad = new Command(() => { doLoad(); });
        }

        private void initList()
        {
            lstBarang = new ObservableCollection<Barang>();
            tmpBarang = new ObservableCollection<Barang>();
        }

        public void getBarang(string strMode)
        {
            //clear list
            lstBarang.Clear();
            tmpBarang.Clear();

            //get mysql connection string
            MySqlConnectionStringBuilder conString = Global.getConString();

            try
            {
                using (MySqlConnection sqlConnection = new MySqlConnection(conString.ConnectionString))
                {
                    

                    using (var cmd = sqlConnection.CreateCommand())
                    {
                        sqlConnection.Open();

                        string strSQL = "";

                        if (strMode == "Barang Semua")
                        {
                            strSQL = "SELECT * FROM tbl_barang WHERE jumlah > 0 ORDER BY kode";
                        }
                        else if (strMode == "Barang Habis")
                        {
                            strSQL = "SELECT * FROM tbl_barang WHERE jumlah < 1 ORDER BY kode";
                        }
                        else //cari
                        {
                            strSQL = "SELECT * FROM tbl_barang WHERE nama LIKE '%" + strMode + "%' OR kode LIKE '%" + strMode + "%' ORDER BY kode";
                        }

                        cmd.CommandText = strSQL;

                        MySqlDataReader sqlReader = cmd.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            tmpBarang.Add(new Barang
                            {
                                kode = "Kode: " + sqlReader[0].ToString(),
                                barang = "Nama: " + sqlReader[1].ToString(),
                                harga = "Harga: " + Convert.ToInt32(sqlReader[2]).ToString("N0"),
                                stok = "Stok: " + sqlReader[4].ToString()
                            }); ;
                        }

                        sqlReader.Close();
                        sqlConnection.Close();

                        //get barang
                        lstBarang = getBarang(1);
                    }
                }
            }
            catch(Exception ex)
            {
                Global.showMessage(ex.Message);
            }
        }

        public ObservableCollection<Barang> getBarang(int page)
        {
            return new ObservableCollection<Barang>(tmpBarang.Skip(5 * (page - 1)).Take(5).ToList());
        }

        private void doLoad()
        {
            var newBarang = getBarang(page);
            page += 1;
            foreach (var item in newBarang)
            {

                lstBarang.Add(item);
                OnPropertyChanged("lstBarang");
            }
        }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // class
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public class Barang
        {
            public string kode { get; set; }
            public string barang { get; set; }
            public string harga { get; set; }
            public string stok { get; set; }
        }
    }
}
