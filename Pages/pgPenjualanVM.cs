using MySqlConnector;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TokoMAUI.Pages
{
    public class pgPenjualanVM : BindProperty
    {
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // fields
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        private ObservableCollection<Penjualan> _lstPenjualan;
        private ObservableCollection<Penjualan> tmpPenjualan;
        private int page = 2;
        private string _strTitle;

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // properties
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public ICommand comLoad { get; set; }
        public ObservableCollection<Penjualan> lstPenjualan { get { return _lstPenjualan; } set { _lstPenjualan = value; OnPropertyChanged("lstPenjualan"); } }
        public string strTitle { get { return _strTitle; } set { _strTitle = value; OnPropertyChanged("strTitle"); } }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public pgPenjualanVM(string strMode)
        {
            initCommands();
            initList();

            //title
            if (strMode == "Penjualan Semua")
            {
                strTitle = "Semua penjualan";
            }
            else if (strMode == "Tempo Penjualan")
            {
                strTitle = "Tempo penjualan";
            }
            else //cari
            {
                strTitle = "Cari penjualan";
            }

            getPenjualan(strMode);
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
            lstPenjualan = new ObservableCollection<Penjualan>();
            tmpPenjualan = new ObservableCollection<Penjualan>();
        }

        public void getPenjualan(string strMode)
        {
            //clear list
            lstPenjualan.Clear();
            tmpPenjualan.Clear();

            //get mysql connection string
            MySqlConnectionStringBuilder conString = Global.getConString();

            try
            {
                using (MySqlConnection sqlConnection = new MySqlConnection(conString.ConnectionString))
                {
                    using (var cmd = sqlConnection.CreateCommand())
                    {
                        sqlConnection.Open();

                        string dtStart = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd");
                        string dtEnd = DateTime.Now.ToString("yyyy-MM-dd");

                        string strSQL = "";

                        if (strMode == "Penjualan Semua")
                        {
                            strSQL = "SELECT t1.*, t2.nama, t2.harga, t2.jumlah, (t2.harga * t2.jumlah) AS perTotal FROM (SELECT * FROM tbl_penjualan WHERE tanggal BETWEEN '" + dtStart + "' AND '" + dtEnd + "') AS t1 INNER JOIN (SELECT * FROM tbl_penjualan_sub) AS t2 ON t1.noNota = t2.noNota ORDER BY noNota DESC";
                        }
                        else if (strMode == "Tempo Penjualan")
                        {
                            strSQL = "SELECT t1.*, t2.nama, t2.harga, t2.jumlah, (t2.harga * t2.jumlah) AS perTotal FROM (SELECT * FROM tbl_penjualan WHERE status = 'Tempo') AS t1 INNER JOIN (SELECT * FROM tbl_penjualan_sub) AS t2 ON t1.noNota = t2.noNota ORDER BY noNota DESC";
                        }
                        else //cari
                        {
                            strSQL = "SELECT t1.*, t2.nama, t2.harga, t2.jumlah, (t2.harga * t2.jumlah) AS perTotal FROM (SELECT * FROM tbl_penjualan WHERE pelanggan LIKE '%" + strMode + "%' OR noNota LIKE '%" + strMode + "%') AS t1 INNER JOIN (SELECT * FROM tbl_penjualan_sub) AS t2 ON t1.noNota = t2.noNota ORDER BY noNota DESC";
                        }

                        cmd.CommandText = strSQL;

                        MySqlDataReader sqlReader = cmd.ExecuteReader();

                        int i = 0;
                        string strNota = "";

                        while (sqlReader.Read())
                        {
                            if (sqlReader[0].ToString() != strNota)
                            {
                                //save temp
                                strNota = sqlReader[0].ToString();

                                //set color
                                Color color;
                                if (sqlReader[6].ToString() == "DP")
                                {
                                    color = Colors.LightPink;
                                }
                                else if (sqlReader[6].ToString() == "Tempo")
                                {
                                    color = Colors.SandyBrown;
                                }
                                else if (sqlReader[6].ToString() == "ATM")
                                {
                                    color = Colors.LightBlue;
                                }
                                else //default lunas
                                {
                                    color = Colors.Cornsilk;
                                }

                                tmpPenjualan.Add(new Penjualan
                                {
                                    nota = "Nota: " + sqlReader[0].ToString(),
                                    tanggal = "Tanggal: " + (sqlReader[1].ToString() != "" ? Convert.ToDateTime(sqlReader[1]).ToString("dd-MMM-yyyy") : "-"),
                                    pelanggan = "Nama: " + sqlReader[3].ToString(),
                                    telepon = "Telepon: " + sqlReader[4].ToString(),
                                    karyawan = "Karyawan: " + sqlReader[5].ToString(),
                                    status = "Bayar: " + sqlReader[6].ToString(),
                                    total = "Total: " + (sqlReader[8].ToString() != "" ? Convert.ToInt32(sqlReader[8]).ToString("N0") : "0"),
                                    barang = "Barang: \n" + sqlReader[12].ToString() + " (@" + (sqlReader[13].ToString() != "" ? Convert.ToInt32(sqlReader[13]).ToString("N0") : "0") +
                                            " x " + sqlReader[14].ToString() + " = " + (sqlReader[15].ToString() != "" ? Convert.ToInt32(sqlReader[15]).ToString("N0") : "0") + ")",
                                    bgColor = color
                                }); ;

                                i++;
                            }
                            else
                            {
                                tmpPenjualan[i - 1].barang += "\n" + sqlReader[12].ToString() + " (@" + (sqlReader[13].ToString() != "" ? Convert.ToInt32(sqlReader[13]).ToString("N0") : "0") +
                                            " x " + sqlReader[14].ToString() + " = " + (sqlReader[15].ToString() != "" ? Convert.ToInt32(sqlReader[15]).ToString("N0") : "0") + ")";
                            }
                        }

                        sqlReader.Close();
                        sqlConnection.Close();

                        //get penjualan
                        lstPenjualan = getPenjualan(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.showMessage(ex.Message);
            }
        }

        public ObservableCollection<Penjualan> getPenjualan(int page)
        {
            return new ObservableCollection<Penjualan>(tmpPenjualan.Skip(5 * (page - 1)).Take(5).ToList());
        }

        private void doLoad()
        {
            var newpenjualan = getPenjualan(page);
            page += 1;
            foreach (var item in newpenjualan)
            {

                lstPenjualan.Add(item);
                OnPropertyChanged("lstPenjualan");
            }
        }

        private string getPenjualanDetail(string strNota)
        {
            string strBarang = "";

            //get mysql connection string
            MySqlConnectionStringBuilder conString = Global.getConString();

            try
            {
                using (MySqlConnection sqlConnection = new MySqlConnection(conString.ConnectionString))
                {
                    using (var cmd = sqlConnection.CreateCommand())
                    {
                        sqlConnection.Open();

                        string strSQL = "SELECT * FROM tbl_penjualan_sub WHERE noNota = '" + strNota + "'";

                        cmd.CommandText = strSQL;

                        MySqlDataReader sqlReader = cmd.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            strBarang = "\n" + sqlReader[2].ToString() + " (" + (sqlReader[3].ToString() != "" ? Convert.ToInt32(sqlReader[3]).ToString("N0") : "0") +
                                        ", " + sqlReader[4].ToString() + ", " + sqlReader[5].ToString() + " hari)";
                        }

                    }
                }
            }
            catch
            {
                strBarang = "";
            }


            return strBarang;
        }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // class
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public class Penjualan
        {
            public string nota { get; set; }
            public string tanggal { get; set; }
            public string pelanggan { get; set; }
            public string telepon { get; set; }
            public string karyawan { get; set; }
            public string status { get; set; }
            public string total { get; set; }
            public string barang { get; set; }
            public Color bgColor { get; set; }
        }
    }
}
