using MySqlConnector;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TokoMAUI.Pages
{
    public class pgServisVM : BindProperty
    {
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // fields
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        private ObservableCollection<Servis> _lstServis;
        private ObservableCollection<Servis> tmpServis;
        private int page = 2;
        private string _strTitle;

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // properties
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public ICommand comLoad { get; set; }
        public ObservableCollection<Servis> lstServis { get { return _lstServis; } set { _lstServis = value; OnPropertyChanged("lstServis"); } }
        public string strTitle { get { return _strTitle; } set { _strTitle = value; OnPropertyChanged("strTitle"); } }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public pgServisVM(string strMode)
        {
            initCommands();
            initList();

            //title
            if (strMode == "Servis Semua")
            {
                strTitle = "Semua servis";
            }
            else if (strMode == "Servis Proses")
            {
                strTitle = "Proses servis";
            }
            else if (strMode == "Tempo Servis")
            {
                strTitle = "Tempo servis";
            }
            else //cari
            {
                strTitle = "Cari servis";
            }

            getServis(strMode);
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
            lstServis = new ObservableCollection<Servis>();
            tmpServis = new ObservableCollection<Servis>();
        }

        public void getServis(string strMode)
        {
            //clear list
            lstServis.Clear();
            tmpServis.Clear();

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

                        if (strMode == "Servis Semua")
                        {
                            strSQL = "SELECT * FROM tbl_servis WHERE tanggal BETWEEN '" + dtStart + "' AND '" + dtEnd + "' ORDER BY noNota DESC";
                        }
                        else if (strMode == "Servis Proses")
                        {
                            strSQL = "SELECT * FROM tbl_servis WHERE proses = 'Proses' AND lokasi = 'Masuk' ORDER BY noNota DESC";
                        }
                        else if (strMode == "Tempo Servis")
                        {
                            strSQL = "SELECT * FROM tbl_servis WHERE proses = 'Selesai' AND lokasi = 'Diambil' AND status = 'Tempo' ORDER BY noNota DESC";
                        }
                        else //cari
                        {
                            strSQL = "SELECT * FROM tbl_servis WHERE pelanggan LIKE '%" + strMode + "%' OR noNota LIKE '%" + strMode + "%' ORDER BY noNota DESC";
                        }

                        cmd.CommandText = strSQL;

                        MySqlDataReader sqlReader = cmd.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            //set color
                            Color color;
                            if (sqlReader[14].ToString() == "Batal")
                            {
                                color = Colors.LightPink;
                            }
                            else if (sqlReader[12].ToString() == "Tempo" && sqlReader[14].ToString() == "Selesai" && sqlReader[15].ToString() == "Diambil")
                            {
                                color = Colors.SandyBrown;
                            }
                            else if (sqlReader[14].ToString() == "Selesai" && sqlReader[15].ToString() == "Diambil")
                            {
                                color = Colors.YellowGreen;
                            } 
                            else if (sqlReader[14].ToString() == "Selesai" && sqlReader[15].ToString() == "Masuk")
                            {
                                color = Colors.LightBlue;
                            }
                            else if (sqlReader[14].ToString() == "Proses" && sqlReader[15].ToString() == "Masuk")
                            {
                                color = Colors.Yellow;
                            }
                            else //default
                            {
                                color = Colors.Cornsilk;
                            }

                            tmpServis.Add(new Servis
                            {
                                nota = "Nota: " + sqlReader[0].ToString(),
                                tanggal = "Masuk: " + (sqlReader[1].ToString() != "" ? Convert.ToDateTime(sqlReader[1]).ToString("dd-MMM-yyyy") : "-"),
                                tanggalSelesai = "Selesai: " + (sqlReader[2].ToString() != "" ? Convert.ToDateTime(sqlReader[2]).ToString("dd-MMM-yyyy") : "-"),
                                pelanggan = "Nama: " + sqlReader[3].ToString(),
                                telepon = "Telepon: " + sqlReader[4].ToString(),
                                karyawan = "Teknisi: " + sqlReader[5].ToString(),
                                barang = "Barang: " + sqlReader[6].ToString(),
                                kelengkapan = "Kelengkapan: " + sqlReader[7].ToString(),
                                keluhan = "Keluhan: " + sqlReader[9].ToString(),
                                pengerjaan = "Pengerjaan: " + sqlReader[10].ToString(),
                                status = "Bayar: " + sqlReader[12].ToString(),
                                garansi = "Garansi: " + sqlReader[13].ToString() + " hari",
                                proses = "Status: " + sqlReader[14].ToString(),
                                lokasi = "Posisi: " + sqlReader[15].ToString(),
                                total = "Biaya: " + (sqlReader[16].ToString() != "" ? Convert.ToInt32(sqlReader[16]).ToString("N0") : "0"),
                                bgColor = color
                            }); ;
                        }

                        sqlReader.Close();
                        sqlConnection.Close();

                        //get servis
                        lstServis = getServis(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.showMessage(ex.Message);
            }
        }

        public ObservableCollection<Servis> getServis(int page)
        {
            return new ObservableCollection<Servis>(tmpServis.Skip(5 * (page - 1)).Take(5).ToList());
        }

        private void doLoad()
        {
            var newServis = getServis(page);
            page += 1;
            foreach (var item in newServis)
            {

                lstServis.Add(item);
                OnPropertyChanged("lstServis");
            }
        }

        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // class
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public class Servis
        {
            public string nota { get; set; }
            public string tanggal { get; set; }
            public string tanggalSelesai { get; set; }
            public string pelanggan { get; set; }
            public string telepon { get; set; }
            public string karyawan { get; set; }
            public string barang { get; set; }
            public string kelengkapan { get; set; }
            public string keluhan { get; set; }
            public string pengerjaan { get; set; }
            public string status { get; set; }
            public string garansi { get; set; }
            public string proses { get; set; }
            public string lokasi { get; set; }
            public string total { get; set; }
            public Color bgColor { get; set; }
        }
    }
}
