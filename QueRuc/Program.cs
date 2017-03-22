using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QueRuc
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainRuc());
        }
    }

    public class Contribuyente
    {
        
        public string Ruc { get; set; }
        public string RazonSocial { get; set; }
        public string NombreComercial { get; set; }
        public string Estado { get; set; }
        public string Clase { get; set; }
        public string Tipo { get; set; }
        public string Contabilidad { get; set; }
        public string ActividadEcocimica { get; set; }

        private List<Establecimiento> _establecimientos;
        public List<Establecimiento> Establecimientos { 
            get{
                return _establecimientos;
            }
            set {
                _establecimientos = value; 
            }
        }

        public Contribuyente()
        {
            _establecimientos = new List<Establecimiento>();
        }
    }

    public class Establecimiento
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Estado { get; set; }
        public string Tipo { get; set; }
    }
}
