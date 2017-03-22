using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace QueRuc
{
    public partial class MainCed : Form
    {
        HttpClient client = null;
        /*string serverurl = "https://servicios.registrocivil.gob.ec/cdd/";
        Uri basedAddr = new Uri("https://servicios.registrocivil.gob.ec/cdd/");

        CookieContainer cookieContainer = new CookieContainer();*/

        


        string Logo = "";
        string Text = "";
        string sCed = "";

        public MainCed()
        {
            InitializeComponent();
            InitCed();
            
        }

        private void btnCon_Click(object sender, EventArgs e)
        {
            Clear();            
            PostCed();
        }

        public void Clear()
        {
            txtNombres.Text = "";
            txtDir.Text = "";
        }

        

        public void InitCed()
        {            
            client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 30);
            client.BaseAddress = new Uri("http://www.mdi.gob.ec/minterior1/antecedentes/data.php");
            client.DefaultRequestHeaders.Accept.Clear();            
               
        }        

        public void PostCed()
        {
            var serverurl = "http://www.mdi.gob.ec/minterior1/antecedentes/data.php";

            client.DefaultRequestHeaders.Accept.Clear();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("tipo", "getDataWsRc"), 
                new KeyValuePair<string, string>("ci", txtConRuc.Text) 
            });



            var response = client.PostAsync(serverurl, formContent);
            
            var sResp = response.Result.Content.ReadAsStringAsync().Result;
            Console.WriteLine(sResp);

            JavaScriptSerializer js = new JavaScriptSerializer();
            var lista = js.Deserialize<List<RespuestaCed>>(sResp);
            
            if (lista.Count > 0 && string.IsNullOrEmpty(lista[0].error))
            {
                var r = lista[0];
                Console.WriteLine("si - " + r.ToString() +", dir: "+ r.residence);
                txtNombres.Text = r.response;
                txtDir.Text = r.residence;
            }
            else
            {
                Console.WriteLine("NO");
            }

            
            
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }

    public class RespuestaCed
    {
        public string error { get; set; }
        public string identity { get; set; }
        public string name { get; set; }
        public string genre { get; set; }
        public string dob { get; set; }
        public string nationality { get; set; }
        public string residence { get; set; }
        public string streets { get; set; }
        public string homenumber { get; set; }
        public string fingerprint { get; set; }

        public string civilstate { get; set; }
        public string response { get; set; }

        public override string ToString()
        {
            return identity + " - " + response;
        }
    }
}