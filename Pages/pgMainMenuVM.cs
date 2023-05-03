using System.Collections.ObjectModel;
using System.Windows.Input;
using static TokoMAUI.Pages.pgBarangVM;

namespace TokoMAUI.Pages
{
    public class pgMainMenuVM : BindProperty
    {
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // constanta
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        private const string strMenuOld = "Semua barang;Barang habis;Cari barang;Semua servis;Proses servis;Tempo servis;Cari servis;Semua penjualan;Tempo penjualan;Cari penjualan;Lainnya";
        private const string strMenu = "Barang,Semua,barang_semua.png,Habis,barang_habis.png,Cari,cari.png;Servis,Semua,servis_semua.png,Proses,servis_proses.png,Cari,cari.png;" +
                                        "Penjualan,Semua,penjualan.png,Cari,cari.png,,;Tempo,Penjualan,penjualan.png,Servis,servis_proses.png,,;Tempo total,Penjualan,penjualan.png,Servis,servis_proses.png,Semua,tempo.png;Lainnya,Data,lainnya.png,,,,";


        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // fields
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        private bool _bolShow = true;
        private bool _bolActivity = false;
        private string _strTitle;
        private string _strStatus;
        private string _strTokoName;
        private string _strSaldo;
        private Color _bgColor;
        private ObservableCollection<Menu> _lstMenu;
        private ObservableCollection<string> _lstMenu1;
        private ObservableCollection<string> _lstMenu2;



        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // properties
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public ICommand comMenu { get; set; }
        public ICommand comChangeDB { get; set; }

        public bool bolShow { get { return _bolShow; } set { _bolShow = value; OnPropertyChanged("bolShow"); } }
        public bool bolActivity { get { return _bolActivity; } set { _bolActivity = value; OnPropertyChanged("bolActivity"); } }
        public string strTitle { get { return _strTitle; } set { _strTitle = value; OnPropertyChanged("strTitle"); } }
        public string strStatus { get { return _strStatus; } set { _strStatus = value; OnPropertyChanged("strStatus"); } }
        public string strTokoName { get { return _strTokoName; } set { _strTokoName = value; OnPropertyChanged("strTokoName"); } }
        public string strSaldo { get { return _strSaldo; } set { _strSaldo = value; OnPropertyChanged("strSaldo"); } }
        public Color bgColor { get { return _bgColor; } set { _bgColor = value; OnPropertyChanged("bgColor"); } }
        public ObservableCollection<Menu> lstMenu { get { return _lstMenu; } set { _lstMenu = value; OnPropertyChanged("lstMenu"); } }
        public ObservableCollection<string> lstMenu1 { get { return _lstMenu1; } set { _lstMenu1 = value; OnPropertyChanged("lstMenu1"); } }
        public ObservableCollection<string> lstMenu2 { get { return _lstMenu2; } set { _lstMenu2 = value; OnPropertyChanged("lstMenu2"); } }


        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        public pgMainMenuVM()
        {
            initCommands();

            Global.setDBName("");

            initList();
            initMenu();

            setData("");
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------
        // Methods
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        private void initCommands()
        {
            comMenu = new Command(doMenu);
            comChangeDB = new Command(doChangeDB);
        }

        private void initList()
        {
            lstMenu = new ObservableCollection<Menu>();
            lstMenu1 = new ObservableCollection<string>();
            lstMenu2 = new ObservableCollection<string>();
        }

        private void initMenu()
        {
            //split menu
            string[] arrMenu = strMenu.Split(new char[] { ';' }, StringSplitOptions.None);

            //clear data
            lstMenu.Clear();

            //generate menu
            foreach (string menu in arrMenu)
            {
                //split sub menu
                string[] arrSubMenu = menu.Split(new char[] { ',' }, StringSplitOptions.None);

                if (arrSubMenu.Count() > 0)
                {
                    lstMenu.Add(new Menu
                    {
                        strMenu = arrSubMenu[0],
                        strSubMenu1 = arrSubMenu[1],
                        strImage1 = arrSubMenu[2],
                        strSubMenu2 = arrSubMenu[3],
                        strImage2 = arrSubMenu[4],
                        strSubMenu3 = arrSubMenu[5],
                        strImage3 = arrSubMenu[6],
                    });
                }
            }
            
        }

        private void setData(string strToko)
        {
            //get status
            strStatus = Global.getStatusAndDate();

            //set status color
            if (strStatus.Contains("Last updated"))
            {
                bgColor = Colors.CornflowerBlue;
            }
            else
            {
                bgColor = Colors.Red;
            }

            //get toko and saldo
            strTokoName = Global.getTokoName(strToko);
            strSaldo = "Saldo: Rp. " + Global.getSaldo().Replace(",", ".") + ",-";
        }

        private async Task<string> cariItem(string strMode)
        {
            string result = await App.Current.MainPage.DisplayPromptAsync("", "Cari " + strMode);

            return result;
        }
        private async void doMenu(object sender)
        {
            switch (sender.ToString())
            {
                case "Barang Semua":
                case "Barang Habis":
                case "Barang Cari":
                    if (Global.recordExists("tbl_barang"))
                    {
                        string strMode = sender.ToString();

                        if (strMode == "Barang Cari")
                        {
                            //ask question
                            string result = await cariItem("barang");

                            if (string.IsNullOrEmpty(result))
                            {
                                return;
                            }
                            else
                            {
                                strMode = result;
                            }
                        }

                        var page = new pgBarang();
                        page.BindingContext = new pgBarangVM(strMode);

                        await App.Current.MainPage.Navigation.PushAsync(page, false);
                    }
                    else
                    {
                        Global.showMessage("Tidak ada barang!");
                    }
                    break;


                case "Servis Semua":
                case "Servis Proses":
                case "Tempo Servis":
                case "Servis Cari":
                    if (Global.recordExists("tbl_servis"))
                    {
                        string strMode = sender.ToString();

                        if (strMode == "Servis Cari")
                        {
                            //ask question
                            string result = await cariItem("servis");

                            if (string.IsNullOrEmpty(result))
                            {
                                return;
                            }
                            else
                            {
                                strMode = result;
                            }
                        }

                        var page = new pgServis();
                        page.BindingContext = new pgServisVM(strMode);

                        await App.Current.MainPage.Navigation.PushAsync(page, false);
                    }
                    else
                    {
                        Global.showMessage("Tidak ada servis!");
                    }
                    break;

                case "Penjualan Semua":
                case "Penjualan Cari":
                case "Tempo Penjualan":
                    if (Global.recordExists("tbl_penjualan"))
                    {
                        string strMode = sender.ToString();

                        if (strMode == "Penjualan Cari")
                        {
                            //ask question
                            string result = await cariItem("penjualan");

                            if (string.IsNullOrEmpty(result))
                            {
                                return;
                            }
                            else
                            {
                                strMode = result;
                            }
                        }

                        var page = new pgPenjualan();
                        page.BindingContext = new pgPenjualanVM(strMode);

                        await App.Current.MainPage.Navigation.PushAsync(page, false);
                    }
                    else
                    {
                        Global.showMessage("Tidak ada penjualan!");
                    }
                    break;

                case "Lainnya Data":
                    if (Global.recordExists("tbl_lainnya"))
                    {
                        var page = new pgLain();
                        page.BindingContext = new pgLainVM();

                        await App.Current.MainPage.Navigation.PushAsync(page, false);
                    }
                    else
                    {
                        Global.showMessage("Tidak ada data lainnya!");
                    }
                    break;

                case "Tempo total Penjualan":
                case "Tempo total Servis":
                case "Tempo total Semua":
                    if (Global.recordExists("tbl_penjualan") || Global.recordExists("tbl_servis"))
                    {
                        string strMode = sender.ToString();

                        var page = new pgTempo();
                        page.BindingContext = new pgTempoVM(strMode);

                        await App.Current.MainPage.Navigation.PushAsync(page, false);
                    }
                    else
                    {
                        Global.showMessage("Tidak ada data tempo!");
                    }

                    break;

                default:
                    Global.showMessage(sender.ToString() + " not implemented!");
                    break;
            }
        }

        private async void doChangeDB(object sender)
        {
            await Task.Run(() => showActivity(false));

            Thread.Sleep(1000);
            string strToko = "";

            Global.setDBName(strToko);
            setData(strToko);

            await Task.Run(() => showActivity(true));
        }

        private void showActivity(bool bolSet)
        {
            bolShow = bolSet;
            bolActivity = !bolSet;
        }
    }

    public class Menu
    { 
        public string strMenu { get; set; }
        public string strSubMenu1 { get; set; }
        public string strSubMenu2 { get; set; }
        public string strSubMenu3 { get; set; }
        public bool bolSubMenu1 { get { return (strSubMenu1.Length > 0 ? true : false); } }
        public bool bolSubMenu2 { get { return (strSubMenu2.Length > 0 ? true : false); } }
        public bool bolSubMenu3 { get { return (strSubMenu3.Length > 0 ? true : false); } }
        public string strImage1 { get; set; }
        public string strImage2 { get; set; }
        public string strImage3 { get; set; }
    }

}
