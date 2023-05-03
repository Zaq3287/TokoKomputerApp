using MySqlConnector;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TokoMAUI.Pages
{
    public class pgLainVM : BindProperty
    {
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // fields
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        private ObservableCollection<Lain> _lstLain;
        private ObservableCollection<Lain> tmpLain;
        private int page = 2;

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // properties
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public ICommand comLoad { get; set; }
        public ObservableCollection<Lain> lstLain { get { return _lstLain; } set { _lstLain = value; OnPropertyChanged("lstLain"); } }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public pgLainVM()
        {
            initCommands();
            initList();

            getLain();
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
            lstLain = new ObservableCollection<Lain>();
            tmpLain = new ObservableCollection<Lain>();
        }

        public void getLain()
        {
            //clear list
            lstLain.Clear();
            tmpLain.Clear();

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

                        string strSQL = "SELECT * FROM tbl_lainnya WHERE tanggal BETWEEN '" + dtStart + "' AND '" + dtEnd + "' ORDER BY noNota DESC";
                        cmd.CommandText = strSQL;

                        MySqlDataReader sqlReader = cmd.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            tmpLain.Add(new Lain
                            {
                                tanggal = "Tanggal: " + (sqlReader[1].ToString() != "" ? Convert.ToDateTime(sqlReader[1]).ToString("dd-MMM-yyyy") : "-"),
                                karyawan = "Nama: " + sqlReader[2].ToString(),
                                transaksi = "Transaksi: " + sqlReader[3].ToString(),
                                jenis = "Jenis: " + sqlReader[4].ToString(),
                                total = "Total: " + (sqlReader[5].ToString() != "" ? Convert.ToInt32(sqlReader[5]).ToString("N0") : "0"),
                                bgColor = sqlReader[4].ToString() == "Keluar" ? Colors.Yellow : Colors.LightBlue
                            }); ;
                        }

                        sqlReader.Close();
                        sqlConnection.Close();

                        //get lain
                        lstLain = getLain(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.showMessage(ex.Message);
            }
        }

        public ObservableCollection<Lain> getLain(int page)
        {
            return new ObservableCollection<Lain>(tmpLain.Skip(5 * (page - 1)).Take(5).ToList());
        }

        private void doLoad()
        {
            var newLain = getLain(page);
            page += 1;
            foreach (var item in newLain)
            {

                lstLain.Add(item);
                OnPropertyChanged("lstLain");
            }
        }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // class
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public class Lain
        {
            public string tanggal { get; set; }
            public string karyawan { get; set; }
            public string transaksi { get; set; }
            public string jenis { get; set; }
            public string total { get; set; }
            public Color bgColor { get; set; }
        }
    }
}
