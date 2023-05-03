using MySqlConnector;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TokoMAUI.Pages
{
    public class pgTempoVM : BindProperty
    {
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // fields
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        private ObservableCollection<Tempo> _lstTempo;
        private ObservableCollection<Tempo> tmpTempo;
        private int page = 2;
        private string _strTitle;
        private string _strTotal;

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // properties
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public ICommand comLoad { get; set; }
        public ObservableCollection<Tempo> lstTempo { get { return _lstTempo; } set { _lstTempo = value; OnPropertyChanged("lstTempo"); } }
        public string strTitle { get { return _strTitle; } set { _strTitle = value; OnPropertyChanged("strTitle"); } }
        public string strTotal { get { return _strTotal; } set { _strTotal = value; OnPropertyChanged("strTotal"); } }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public pgTempoVM(string strMode)
        {
            initCommands();
            initList();

            //title
            if (strMode == "Tempo total Penjualan")
            {
                strTitle = "Tempo penjualan";
            }
            else if (strMode == "Tempo total Servis")
            {
                strTitle = "Tempo servis";
            }
            else //semua
            {
                strTitle = "Tempo penjualan & servis";
            }

            getTempo(strMode);
            strTotal = "Total: " + getTotal(strMode);
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
            lstTempo = new ObservableCollection<Tempo>();
            tmpTempo = new ObservableCollection<Tempo>();
        }

        public string getTotal(string strMode)
        {
            //get mysql connection string
            MySqlConnectionStringBuilder conString = Global.getConString();

            try
            {
                // open a connection asynchronously
                var connection = new MySqlConnection(conString.ConnectionString);
                connection.Open();

                string strSQL = "";
                

                if (strMode == "Tempo total Penjualan")
                {
                    strSQL = "SELECT SUM(total) AS tagihan FROM tbl_penjualan WHERE status = 'Tempo'";
                }
                else if (strMode == "Tempo total Servis")
                {
                    strSQL = "SELECT SUM(total) AS tagihan FROM tbl_servis WHERE status = 'Tempo' AND proses = 'Selesai' AND lokasi = 'Diambil'";
                }
                else //semua
                {
                    strSQL = "SELECT SUM(tagihan) AS tagihan FROM (SELECT SUM(total) AS tagihan FROM tbl_penjualan WHERE status = 'Tempo' UNION ALL " +
                                 "SELECT SUM(total) AS tagihan FROM tbl_servis WHERE status = 'Tempo' AND proses = 'Selesai' AND lokasi = 'Diambil') AS t1";
                }

                //get total
                var command = connection.CreateCommand();
                command.CommandText = strSQL;
                string total = Convert.ToInt32(command.ExecuteScalar().ToString()).ToString("N0");

                connection.Close();

                return total;
            }
            catch
            {
                return "0";
            }
        }

        public void getTempo(string strMode)
        {
            //clear list
            lstTempo.Clear();

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
                        string strSQLDetail = "";

                        if (strMode == "Tempo total Penjualan")
                        {
                            strSQL = "SELECT pelanggan, SUM(total) AS tagihan FROM tbl_penjualan WHERE status = 'Tempo' GROUP BY pelanggan ORDER BY pelanggan";
                            strSQLDetail = "SELECT noNota, tanggal, pelanggan, total FROM tbl_penjualan WHERE status = 'Tempo'";
                        }
                        else if (strMode == "Tempo total Servis")
                        {
                            strSQL = "SELECT pelanggan, SUM(total) AS tagihan FROM tbl_servis WHERE status = 'Tempo' AND proses = 'Selesai' AND lokasi = 'Diambil' GROUP BY pelanggan ORDER BY pelanggan";
                            strSQLDetail = "SELECT noNota, tanggal, pelanggan, total FROM tbl_servis WHERE status = 'Tempo' AND proses = 'Selesai' AND lokasi = 'Diambil'";
                        }
                        else //semua
                        {
                            strSQL = "SELECT pelanggan, SUM(tagihan) AS tagihan FROM (SELECT pelanggan, SUM(total) AS tagihan FROM tbl_penjualan WHERE status = 'Tempo' GROUP BY pelanggan UNION ALL " +
                                         "SELECT pelanggan, SUM(total) AS tagihan FROM tbl_servis WHERE status = 'Tempo' AND proses = 'Selesai' AND lokasi = 'Diambil' GROUP BY pelanggan) AS t1 " +
                                         "GROUP BY pelanggan ORDER BY pelanggan";
                            strSQLDetail = "SELECT noNota, tanggal, pelanggan, total FROM tbl_penjualan WHERE status = 'Tempo' UNION ALL " +
                                            "SELECT noNota, tanggal, pelanggan, total FROM tbl_servis WHERE status = 'Tempo' AND proses = 'Selesai' AND lokasi = 'Diambil'";
                        }

                        ObservableCollection<TempoDetail> lstDetail = new ObservableCollection<TempoDetail>();
                        lstDetail.Clear();

                        cmd.CommandText = strSQLDetail;

                        MySqlDataReader sqlReader = cmd.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            lstDetail.Add(new TempoDetail
                            {
                                noNota = sqlReader[0].ToString(),
                                tanggal = (sqlReader[1].ToString() != "" ? Convert.ToDateTime(sqlReader[1]).ToString("dd-MMM-yyyy") : "-"),
                                pelanggan = sqlReader[2].ToString(),
                                total = Convert.ToInt32(sqlReader[3]).ToString("N0")
                            }); ;
                        }

                        sqlReader.Close();

                        cmd.CommandText = strSQL;

                        string strDetail = "";

                        sqlReader = cmd.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            strDetail = "";

                            var tmp = lstDetail.Where(x => x.pelanggan == sqlReader[0].ToString()).ToList().OrderBy(x => x.noNota);

                            foreach (var detail in tmp)
                            {
                                strDetail += detail.noNota + " - " + detail.tanggal + " = " +  detail.total + "\n";
                            }

                            tmpTempo.Add(new Tempo
                            {
                                pelanggan = sqlReader[0].ToString(),
                                tagihan = Convert.ToInt32(sqlReader[1]).ToString("N0"),
                                detail = strDetail
                            }); ;
                        }

                        sqlReader.Close();
                        sqlConnection.Close();

                        //get tempo
                        lstTempo = getTempo(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.showMessage(ex.Message);
            }
        }

        public ObservableCollection<Tempo> getTempo(int page)
        {
            return new ObservableCollection<Tempo>(tmpTempo.Skip(5 * (page - 1)).Take(5).ToList());
        }

        private void doLoad()
        {
            var newTempo = getTempo(page);
            page += 1;
            foreach (var item in newTempo)
            {

                lstTempo.Add(item);
                OnPropertyChanged("lstTempo");
            }
        }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // class
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public class Tempo
        {
            public string pelanggan { get; set; }
            public string tagihan { get; set; }
            public string detail { get; set; }
        }

        public class TempoDetail
        {
            public string noNota { get; set; }
            public string tanggal { get; set; }
            public string pelanggan { get; set; }
            public string total { get; set; }
        }
    }
}
