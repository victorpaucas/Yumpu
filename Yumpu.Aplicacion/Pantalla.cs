using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Yumpu.Aplicacion
{
    public partial class Pantalla : Form
    {
        int CantidadImagenes;
        string Codigo;
        string Directorio;

        public Pantalla()
        {
            InitializeComponent();
        }

        private void btnDescargar_Click(object sender, EventArgs e)
        {
            if (ValidarCampos())
            {
                try
                {
                    Codigo = txtCodigo.Text.Trim();

                    using (WebClient webClient = new WebClient())
                    {
                        var downloadString = webClient.DownloadString("https://www.yumpu.com/en/document/json/" + txtCodigo.Text.Trim());
                        JObject jObject = JObject.Parse(downloadString);
                        CantidadImagenes = jObject["document"]["pages"].Count();

                        if (CantidadImagenes > 0)
                        {
                            //Directorio = Directory.GetCurrentDirectory() + "/" + "Imagenes - " + txtCodigo.Text.Trim() + " - " + DateTime.Now.ToString("yyyyMMddhhmmss");
                            //Directory.CreateDirectory(Directorio);

                            #region "Descargar Imagenes"
                            FormatearProgreso("Descargando imagenes ...");
                            try
                            {
                                for (int Indice = 1; Indice <= CantidadImagenes; Indice++)
                                {
                                    webClient.DownloadFile(new Uri("https://img.yumpu.com/" + Codigo + "/" + Convert.ToString(Indice) + "/1238x1600"), Directorio + "/" + Indice + ".jpg");
                                    pgbProgreso.PerformStep();
                                }
                            }
                            catch (Exception)
                            {
                                lblProgreso.Text = "Error al descargar imagenes.";
                                MessageBox.Show("Error al descargar imagenes.", "Yumpu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            #endregion "Descargar Imagenes"

                            #region "Convertir Imagenes a PDF"
                            FormatearProgreso("Convertiendo imagenes a PDF ...");
                            Document document = new Document();

                            try
                            {
                                PdfWriter.GetInstance(document, new FileStream(Directorio + "/" + Codigo + " - " + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf", FileMode.Create));
                                document.Open();

                                for (int Indice = 1; Indice <= CantidadImagenes; Indice++)
                                {
                                    Image image = Image.GetInstance(Directorio + "/" + Indice + ".jpg");
                                    image.ScaleAbsolute(570, 750);
                                    document.Add(image);
                                    pgbProgreso.PerformStep();
                                }
                            }
                            catch (Exception)
                            {
                                lblProgreso.Text = "Error al convertir imagenes a pdf.";
                                MessageBox.Show("Error al convertir imagenes a pdf.", "Yumpu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            finally
                            {
                                document.Close();
                            }
                            #endregion "Convertir Imagenes a PDF"

                            #region "Eliminar Imagenes"
                            FormatearProgreso("Eliminando imagenes ...");
                            try
                            {
                                for (int Indice = 1; Indice <= CantidadImagenes; Indice++)
                                {
                                    if (File.Exists(Directorio + "/" + Indice + ".jpg"))
                                    {
                                        File.Delete(Directorio + "/" + Indice + ".jpg");
                                    }
                                    pgbProgreso.PerformStep();
                                }
                            }
                            catch (Exception)
                            {
                                lblProgreso.Text = "Error al eliminar imagenes.";
                                MessageBox.Show("Error al eliminar imagenes.", "Yumpu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            #endregion "Eliminar Imagenes"

                            FormatearProgreso("");
                            MessageBox.Show("Se descargó la revista correctamente.", "Yumpu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error al buscar código, verifique que el código exista.", "Yumpu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRuta_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtRuta.Text = folderBrowserDialog.SelectedPath;
                Directorio = folderBrowserDialog.SelectedPath;
            }
        }

        private void FormatearProgreso(string Texto)
        {
            pgbProgreso.Visible = true;
            pgbProgreso.Minimum = 1;
            pgbProgreso.Maximum = CantidadImagenes;
            pgbProgreso.Value = 1;
            pgbProgreso.Step = 1;

            lblProgreso.Text = Texto;
        }

        private bool ValidarCampos()
        {
            if (txtRuta.Text.Trim().Length <= 0)
            {
                MessageBox.Show("Falta seleccionar la ruta de descarga.", "Yumpu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (txtCodigo.Text.Trim().Length <= 0)
            {
                MessageBox.Show("Falta ingresar el código de revista.", "Yumpu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void tsmAyuda_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Autor: Paucas Navarro, Victor Hugo \nCorreo: vhpaucas@gmail.com \nRepositorio: www.github.com/victorpaucas", "Yumpu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
