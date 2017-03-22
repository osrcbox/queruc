using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
    public partial class MainRuc : Form
    {
        public MainRuc()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var text = txtConRuc.Text.Trim();
            LimpiarCampos();
            if (text.Length > 10)
            {
                ConsultarRuc();
            }
            else
            {
                ConsultarCedula();
            }
        }

        public void LimpiarCampos()
        {            
            txtRaz.Text = "";
            txtNom.Text = "";
            txtEst.Text = "";
            txtCla.Text = "";
            txtTip.Text = "";
            txtObl.Text = "";
            txtAct.Text = "";
        }

        public void ConsultarRuc()
        {
            var serverurlRuc = "https://declaraciones.sri.gob.ec/facturacion-internet/consultas/publico/ruc-datos2.jspa";
            var serverurlEst = "https://declaraciones.sri.gob.ec/facturacion-internet/consultas/publico/ruc-establec.jspa";

            var contribuyente = new Contribuyente();

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 30);
                client.BaseAddress = new Uri(serverurlRuc);
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("accion", "siguiente"), 
                    new KeyValuePair<string, string>("ruc", txtConRuc.Text) 
                });

                //Lee ruc
                var responseRuc = client.PostAsync(serverurlRuc, formContent);
                var sRespRuc = responseRuc.Result.Content.ReadAsStringAsync().Result;
                var sFinalRuc = this.LimpiarTexto(sRespRuc);

                
                var srRuc = new StringReader(sFinalRuc);
                var lnRuc = "";

                while (lnRuc != null)
                {
                    lnRuc = srRuc.ReadLine();
                    if(lnRuc != null){
                        if (lnRuc.Contains("Raz&oacute;n Social:"))
                        {
                            var rs = srRuc.ReadLine().Replace("<td>","").Replace("</td>", "").Trim();
                            txtRaz.Text = rs;
                            contribuyente.RazonSocial = rs;
                        }

                        if (lnRuc.Contains("Nombre Comercial:"))
                        {
                            var rs = srRuc.ReadLine().Replace("<td>","").Replace("</td>", "").Trim();
                            txtNom.Text = rs;
                            contribuyente.NombreComercial = rs;
                        }

                        if (lnRuc.Contains("Estado del Contribuyente en el RUC"))
                        {
                            var rs = srRuc.ReadLine().Replace("<td>", "").Replace("</td>", "").Trim();
                            txtEst.Text = rs;
                            contribuyente.Estado = rs;
                        }

                        if (lnRuc.Contains("Clase de Contribuyente"))
                        {
                            var rs = srRuc.ReadLine().Replace("<td>", "").Replace("</td>", "").Trim();
                            txtCla.Text = rs;
                            contribuyente.Clase = rs;
                        }

                        if (lnRuc.Contains("Tipo de Contribuyente"))
                        {
                            var rs = srRuc.ReadLine().Replace("<td>", "").Replace("</td>", "").Trim();
                            txtTip.Text = rs.Contains("javascript:sociedad();")?"Sociedad": rs;
                            contribuyente.Tipo = txtTip.Text;
                        }

                        if (lnRuc.Contains("Obligado a llevar Contabilidad"))
                        {
                            var rs = srRuc.ReadLine().Replace("<td>", "").Replace("</td>", "").Trim();
                            txtObl.Text = rs;
                            contribuyente.Contabilidad = rs;
                        }

                        if (lnRuc.Contains("Actividad Econ&oacute;mica Principal"))
                        {
                            var rs = srRuc.ReadLine().Replace("<td>", "").Replace("</td>", "").Trim();
                            txtAct.Text = rs;
                            contribuyente.ActividadEcocimica = rs;
                        }
                    }

                }

                //Lee establecimientos
                var responseEst = client.GetAsync(serverurlEst);
                var sRespEst = responseEst.Result.Content.ReadAsStringAsync().Result;
                var sFinalEst = this.LimpiarTexto(sRespEst); ;

                var srEst = new StringReader(sFinalEst);
                var lnEst = "";
                var tipo = "";

                while (lnEst != null)
                {
                    lnEst = srEst.ReadLine();
                    if (lnEst != null)
                    {


                        if (lnEst.Contains("<b>Establecimiento Matriz</b>")) tipo = "Matriz";
                        if (lnEst.Contains("<b>Establecimientos Adicionales</b>")) tipo = "Adicional";

                        if (lnEst.Contains(@"<td class=""primeraCol"" style=""text-align: center;"">"))
                        {
                            var est = new Establecimiento() { Tipo = tipo};
                            est.Codigo = lnEst.Replace(@"<td class=""primeraCol"" style=""text-align: center;"">", "");
                            srEst.ReadLine();

                            est.Nombre = srEst.ReadLine().Replace("<td>", "").Replace("</td>", "");

                            var dir = srEst.ReadLine();
                            while (!dir.Contains("</td>"))
                            {
                                dir += srEst.ReadLine();
                            }

                            est.Direccion = dir.Replace(@"<td style=""text-align: center;"">", "").Replace("</td>","");

                            srEst.ReadLine();
                            est.Estado = srEst.ReadLine();

                            contribuyente.Establecimientos.Add(est);
                        }

                        
                    }

                }
             
            }//End client

            //Console.WriteLine(contribuyente.Establecimientos.Count);
            dataGridView1.DataSource = contribuyente.Establecimientos;
            var activo = contribuyente.Establecimientos.OrderByDescending(p=>p.Tipo).FirstOrDefault(p=>p.Estado == "Activo");
            txtDir.Text = activo != null ? activo.Direccion : "";

            if (string.IsNullOrEmpty(contribuyente.RazonSocial))
            {
                txtRaz.Text = "RUC INVÁLIDO";
            }
        }

        public void ConsultarCedula()
        {
            var serverurl = "http://www.mdi.gob.ec/minterior1/antecedentes/data.php";

            var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 30);
            client.BaseAddress = new Uri("http://www.mdi.gob.ec/minterior1/antecedentes/data.php");
            client.DefaultRequestHeaders.Accept.Clear();  
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
                Console.WriteLine("si - " + r.ToString() + ", dir: " + r.residence);
                txtRaz.Text = r.response;
                txtDir.Text = r.residence;
            }
            else
            {
                txtRaz.Text = "CEDULA INVÁLIDA";
            }

            dataGridView1.DataSource = new List<Establecimiento>();
        }

        private string LimpiarTexto(string sResp)
        {
            string sFinal = "";
            //Quita lineas
            var src = new StringReader(sResp);
            var lnc = "";
            while (lnc != null)
            {
                lnc = src.ReadLine();
                var l = (lnc ?? "").Trim();
                if (!string.IsNullOrEmpty(l))
                {
                    sFinal += l + "\n";
                }
            }

            return sFinal;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void txtConRuc_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
