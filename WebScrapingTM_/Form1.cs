using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using RestSharp;
using ScrapySharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WooCommerceNET.WooCommerce.v3.Extension;

namespace WebScrapingTM_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        List<prodPrincipal> justoList = new List<prodPrincipal>();
        List<prodPrincipal> centralList = new List<prodPrincipal>();
        List<prodPrincipal> chedrauiList = new List<prodPrincipal>();
        List<prodPrincipal> tuMarchanteList = new List<prodPrincipal>();
        private void button1_Click(object sender, EventArgs e)
        {
            //List<prodPrincipal> justoList = new List<prodPrincipal>();
            //List<prodPrincipal> centralList = new List<prodPrincipal>();
            //List<prodPrincipal> chedrauiList = new List<prodPrincipal>();
            //List<prodPrincipal> tuMarchanteList = new List<prodPrincipal>();
            justoList.Clear();
            centralList.Clear();
            chedrauiList.Clear();
            tuMarchanteList.Clear();

            skuProductos.Clear();

            string productos = textBox1.Text;
            if (productos == "")
            {
                MessageBox.Show("Ingrese productos");
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine(productos);
                //MessageBox.Show(productos);
                string[] auxCadena = productos.Split(',');
                //MessageBox.Show("Scrap Justo");
                generateJusto(auxCadena);
                //MessageBox.Show("Scrap Central en Linea");
                generateCentralLinea(auxCadena);
                //MessageBox.Show("Scrap Chedraui");
                generateChedraui(auxCadena);
                //MessageBox.Show("Scrap TuMarchante");
                generateTuMarchante(auxCadena);
                dataAnalitycs();
            }
                        
        }

        void generateJusto(string[] auxCadena)
        {

            foreach (var producto in auxCadena)
            {
                List<dataProductos> product = new List<dataProductos>();

                //Console.WriteLine("***" + producto + "***");
                string[] auxProducto = producto.Split(' ');
                string auxCad = producto;
                string cadenaProducto = auxCad.Replace(' ', '+');

                var urlJusto = "https://justo.mx/search/?q=" + cadenaProducto;

                ChromeOptions options = new ChromeOptions();

                //var proxy = new Proxy();
                //proxy.Kind = ProxyKind.Manual;
                //proxy.IsAutoDetect = false;
                //proxy.HttpProxy =
                //proxy.SslProxy = "45.174.248.53";
                //options.Proxy = proxy;
                //options.AddArgument("ignore-certificate-errors");
                IWebDriver driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                
                using (driver)
                {
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    driver.Navigate().GoToUrl(urlJusto);
                    //driver.FindElement(By.Id("heading1")).Click();
                    Thread.Sleep(5000);
                    IWebElement firstResult = wait.Until(ExpectedConditions.ElementExists(By.Id("product-search-react ")));
                    //Console.WriteLine(firstResult.Text);

                    IList<IWebElement> listProductos = firstResult.FindElements(By.CssSelector("div.styles__StyledCard-justo__sc-1usw4yt-0 article a div.styles__StyledInfo-justo__sc-1usw4yt-5 p.styles__StyledName-justo__sc-1usw4yt-6"));
                    IList<IWebElement> listPrecios = firstResult.FindElements(By.CssSelector("div.styles__StyledCard-justo__sc-1usw4yt-0 article a div.styles__StyledInfo-justo__sc-1usw4yt-5 p.styles__StyledPrice-justo__sc-1usw4yt-7 span"));
                    //IList<IWebElement> list = firstResult.    FindElements(By.ClassName("p.styles__StyledName-justo__sc-1usw4yt-6"));

                    for (int i = 0; i < listProductos.Count; i++)
                    {
                        Console.WriteLine(listProductos[i].Text + "-------" + listPrecios[i].Text);
                        product.Add(createAddProd(listProductos[i].Text, listPrecios[i].Text));
                    }

                    if (listProductos.Count > 0)
                    {
                        justoList.Add(createAddProPrinc(producto, product));
                    }
                    else
                    {
                        MessageBox.Show("Sin coincidencias de busqueda : Justo");
                    }
                    
                }
            }
        }
        void generateCentralLinea(string[] auxCadena)
        {

            foreach (var producto in auxCadena)
            {
                List<dataProductos> product = new List<dataProductos>();

                string auxCad = producto;
                string cadenaProducto = auxCad.Replace(' ', '+');

                var urlCentralEnLinea = "https://www.centralenlinea.com/search?q=" + cadenaProducto;
                List<string> nameProductos = new List<string>();
                List<string> precio = new List<string>();
                HtmlWeb web = new HtmlWeb();

                HtmlAgilityPack.HtmlDocument html = web.Load(urlCentralEnLinea);

                //HtmlNode node = html.DocumentNode.CssSelect(".product-grid").First();
                //Console.WriteLine(node.InnerHtml);

                foreach (var nodo in html.DocumentNode.CssSelect(".product-grid div.col .card-title"))
                {
                    nameProductos.Add(nodo.InnerHtml);
                }

                foreach (var nodo in html.DocumentNode.CssSelect(".product-grid div.col .product-price"))
                {
                    precio.Add(nodo.InnerHtml);
                }

                for (int i = 0; i < nameProductos.Count; i++)
                {
                    string[] aux = precio[i].Split('<');
                    //string data = nameProductos[i] + " --- " + aux[0];
                    //Console.WriteLine(data);
                    product.Add(createAddProd(nameProductos[i], aux[0]));
                }

                if(nameProductos.Count > 0)
                {
                    centralList.Add(createAddProPrinc(producto, product));
                }
                else
                {
                    MessageBox.Show("Sin coincidencias de busqueda : Central en Linea");
                }

                

            }

        }
        void generateChedraui(string[] auxCadena)
        {

            foreach (var producto in auxCadena)
            {
                List<dataProductos> product = new List<dataProductos>();
                //Console.WriteLine("***" + producto + "***");
                string auxCad = producto;
                string space = " ";
                string newCaracter = "%20";
                string cadenaProducto = auxCad.Replace(space, newCaracter);


                //var urlCentralEnLinea = "https://justo.mx/search/?q=" + cadenaProducto;
                var urlChedraui = "https://www.chedraui.com.mx/search?text=" + cadenaProducto;
                List<string> nameProductos = new List<string>();
                List<string> precio = new List<string>();
                HtmlWeb web = new HtmlWeb();

                HtmlAgilityPack.HtmlDocument html = web.Load(urlChedraui);

                //HtmlNode node = html.DocumentNode.CssSelect("body").First();
                //Console.WriteLine(node.InnerHtml);

                //div.wrap-text-hook a.product__list--name
                //div.product__listing--price
                foreach (var nodo in html.DocumentNode.CssSelect("li.product__list--item div.wrap-text-hook a.product__list--name"))
                {
                    //Console.WriteLine(nodo.InnerHtml);
                    nameProductos.Add(nodo.InnerHtml);
                }


                foreach (var nodo in html.DocumentNode.CssSelect("li.product__list--item div.product__list--price-panel div.product__listing--price"))
                {
                    //Console.WriteLine(nodo.InnerHtml);
                    precio.Add(nodo.InnerHtml);
                }

                var limitChedraui = 0;

                if (nameProductos.Count > 3)
                {
                    limitChedraui = 3;
                }
                else
                {
                    limitChedraui = nameProductos.Count;
                }

                for (int i = 0; i < limitChedraui; i++)
                {
                    //string[] aux = precio[i].Split('<');
                    string data = nameProductos[i] + " --- " + precio[i];
                    //Console.WriteLine(data);
                    product.Add(createAddProd(nameProductos[i], precio[i]));
                }

                if (nameProductos.Count > 0)
                {
                    chedrauiList.Add(createAddProPrinc(producto, product));
                }
                else
                {
                    MessageBox.Show("Sin coincidencias de busqueda : Chedraui");
                }
                
            }
        }


        List<string> skuProductos = new List<string>();
        void generateTuMarchante(string[] auxCadena)
        {
            foreach (var producto in auxCadena)
            {
                List<dataProductos> product = new List<dataProductos>();

                //Console.WriteLine("***" + producto + "***");
                string auxCad = producto;
                string space = " ";
                string newCaracter = "-";
                string cadenaProducto = auxCad.Replace(space, newCaracter);
                cadenaProducto = cadenaProducto.Replace("ñ", "n");
                cadenaProducto = cadenaProducto.Replace("á", "a");
                cadenaProducto = cadenaProducto.Replace("é", "e");
                cadenaProducto = cadenaProducto.Replace("í", "i");
                cadenaProducto = cadenaProducto.Replace("ó", "o");
                cadenaProducto = cadenaProducto.Replace("ú", "u");

                var urlTuMarchante = "https://tumarchante.mx/producto/" + cadenaProducto;
                List<string> nameProductos = new List<string>();
                //List<string> skuProductos = new List<string>();
                List<string> precio = new List<string>();
                HtmlWeb web = new HtmlWeb();

                HtmlAgilityPack.HtmlDocument html = web.Load(urlTuMarchante);
                //HtmlNode node = html.DocumentNode.CssSelect("body").First();
                //Console.WriteLine(node.InnerHtml);

                foreach (var nodo in html.DocumentNode.CssSelect("div.et_pb_module_inner h1"))
                {
                    //Console.WriteLine(nodo.InnerHtml);
                    nameProductos.Add(nodo.InnerHtml);
                }

                foreach (var nodo in html.DocumentNode.CssSelect("div.product_meta span.sku_wrapper span.sku"))
                {                    
                    skuProductos.Add(nodo.InnerHtml);
                    //Console.WriteLine(nodo.InnerHtml);
                    //MessageBox.Show(nodo.InnerHtml);
                }


                foreach (var nodo in html.DocumentNode.CssSelect("div.et_pb_wc_price_1_tb_body div.et_pb_module_inner p.price span.woocommerce-Price-amount bdi"))
                {

                    //var auxprecio = nodo.InnerHtml.Split("</span>");                    
                    var auxprecio = nodo.InnerHtml.Split(new[] { "</span>" }, StringSplitOptions.None);
                    //Console.WriteLine(auxprecio[1]);
                    precio.Add(auxprecio[1]);
                }
                //Console.WriteLine("---"+nameProductos.Count+"---");
                if (nameProductos.Count != 1)
                {
                    nameProductos.RemoveAt(nameProductos.Count - 1);
                }

                if (precio.Count != 1)
                {
                    precio.RemoveAt(0);
                }

                for (int i = 0; i < nameProductos.Count; i++)
                {
                    //string[] aux = precio[i].Split('<');
                    //string data = nameProductos[i] + " --- " + precio[i];
                    //Console.WriteLine(data);
                    product.Add(createAddProd(nameProductos[i], precio[i]));
                }

                tuMarchanteList.Add(createAddProPrinc(producto, product));

            }
        }

        void dataAnalitycs()
        {
            //MessageBox.Show("Estoy en dataAnalitycs" + ": Justo - " + justoList.Count);
            //MessageBox.Show("Estoy en dataAnalitycs" + ": Central - " + centralList.Count);
            //MessageBox.Show("Estoy en dataAnalitycs" + ": Chedraui - " + chedrauiList.Count);
            //MessageBox.Show("Estoy en dataAnalitycs" + ": TuMarchante - " + tuMarchanteList.Count);

            //sardina calmex 425 gr
            //sardina calmex 425 gr,lata de atun tuny en agua de 140 g
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            dataGridView2.DataSource = null;
            dataGridView2.Rows.Clear();
            dataGridView2.Columns.Clear();

            dataGridView3.DataSource = null;
            dataGridView3.Rows.Clear();
            dataGridView3.Columns.Clear();

            dataGridView4.DataSource = null;
            dataGridView4.Rows.Clear();
            dataGridView4.Columns.Clear();

            dataGridView5.DataSource = null;
            dataGridView5.Rows.Clear();
            dataGridView5.Columns.Clear();

            /*
            dataGridView5.Columns.Add("nombreProd", "Producto TM");
            dataGridView5.Columns.Add("precioProd", "Justo");
            dataGridView5.Columns.Add("precioProd", "Central en Linea");
            dataGridView5.Columns.Add("precioProd", "Chedraui");
            */
            dataGridView5.Columns.Add("nombreProd", "Producto TM");
            dataGridView5.Columns.Add("precioProd", "Justo");
            dataGridView5.Columns.Add("precioProd", "Central en Linea");
            dataGridView5.Columns.Add("precioProd", "Chedraui");
            dataGridView5.Columns.Add("nombreProd", "Precio S/I");
            dataGridView5.Columns.Add("nombreProd", "Costo");
            dataGridView5.Columns.Add("nombreProd", "Utilidad");
            dataGridView5.Columns.Add("nombreProd", "% Utilidad");
            dataGridView5.Columns.Add("precioProd", "Precio Sugerido");
            dataGridView5.Columns.Add("precioProd", "Acciones");   

            //JUSTO
            dataGridView1.Columns.Add("nombreProd", "Producto");
            dataGridView1.Columns.Add("precioProd", "Precio");
            for (int i = 0; i < justoList.Count; i++)
            {
                int contProductos = justoList[i].array.Count;
                dataGridView1.Rows.Add(justoList[i].producto);
                for (int j = 0; j < contProductos; j++)
                {
                    dataGridView1.Rows.Add(justoList[i].array[j].producto, justoList[i].array[j].precio);
                }

            }

            //CENTRAL EN LINEA
            dataGridView2.Columns.Add("nombreProd", "Producto");
            dataGridView2.Columns.Add("precioProd", "Precio");
            for (int i = 0; i < centralList.Count; i++)
            {
                int contProductos = centralList[i].array.Count;
                dataGridView2.Rows.Add(centralList[i].producto);
                for (int j = 0; j < contProductos; j++)
                {
                    dataGridView2.Rows.Add(centralList[i].array[j].producto, centralList[i].array[j].precio);
                }

            }

            //CHEDRAUI
            dataGridView3.Columns.Add("nombreProd", "Producto");
            dataGridView3.Columns.Add("precioProd", "Precio");
            for (int i = 0; i < chedrauiList.Count; i++)
            {
                int contProductos = chedrauiList[i].array.Count;
                dataGridView3.Rows.Add(chedrauiList[i].producto);
                for (int j = 0; j < contProductos; j++)
                {
                    string precioChedraui = chedrauiList[i].array[j].precio;
                    dataGridView3.Rows.Add(chedrauiList[i].array[j].producto, precioChedraui.Trim());
                }

            }

            //TUMARCHANTE
            dataGridView4.Columns.Add("nombreProd", "Producto");
            dataGridView4.Columns.Add("precioProd", "Precio");
            for (int i = 0; i < tuMarchanteList.Count; i++)
            {
                int contProductos = tuMarchanteList[i].array.Count;
                //dataGridView4.Rows.Add(tuMarchanteList[i].producto);
                for (int j = 0; j < contProductos; j++)
                {
                    dataGridView4.Rows.Add(tuMarchanteList[i].array[j].producto,"$"+ tuMarchanteList[i].array[j].precio);
                }

            }
            
            
            evaluateDataAsync();
        }

        async Task evaluateDataAsync()
        {
            
            string productos = textBox1.Text;
            string[] auxCadena = productos.Split(','); //los productos del marchante


            for (int i = 0; i < auxCadena.Length; i++)
            {
                //MessageBox.Show("evaluando producto " + i + ": " + auxCadena[i]);
                string[] productoN = auxCadena[i].Split(' ');//producto del marchante separado por " " split
                List<string> prodductoNAux = new List<string>();

                for (int p = 0; p < productoN.Length; p++)
                {
                    string word = productoN[p].ToUpper();
                    string palabaSinTildes = Regex.Replace(word.Normalize(System.Text.NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
                    //MessageBox.Show(palabaSinTildes);
                    prodductoNAux.Add(word);
                }

                //EVALUANDO JUSTO
                float precioPromedioJusto = 0;
                string status = "";

                float precioAucumulado = 0;

                int contadorCoincidencias = 0;
                float precioProd = 0;
                int contadorCoincidenciasProds = 0;
                if(justoList.Count > 0)
                {
                    for (int j = 0; j < 1; j++)
                    {
                        string productoJusto = justoList[i].array[j].producto;
                        //MessageBox.Show(productoJusto);
                        //string[] prodJustoAux = productoJusto.Split(" ");

                        string[] prodJustoAux = productoJusto.Split(new[] { " " }, StringSplitOptions.None);

                        for (int k = 0; k < prodJustoAux.Length; k++)
                        {
                            string word = prodJustoAux[k].ToUpper();
                            string palabaSinTildes = Regex.Replace(word.Normalize(System.Text.NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
                            //MessageBox.Show("palabras justo:"+ palabaSinTildes);
                            if (prodductoNAux.Contains(palabaSinTildes))
                            {
                                contadorCoincidencias++;
                            }
                        }

                        if (contadorCoincidencias >= ((prodductoNAux.Count * 70) / 100))
                        {
                            precioProd = float.Parse(justoList[i].array[j].precio.Substring(1));
                            contadorCoincidenciasProds++;
                        }
                        else
                        {
                            //MessageBox.Show("No hubo coincidencias de palabras de mas del 70%");                            

                            string patron = @"(?:- *)?\d+(?:\.\d+)?";
                            Regex regex = new Regex(patron);

                            string[] numProdJusto = regex.Matches(productoJusto)
                                                       .OfType<Match>()
                                                       .Select(m => m.Value)
                                                       .ToArray();

                            string[] numProducto = regex.Matches(auxCadena[i])
                                                       .OfType<Match>()
                                                       .Select(m => m.Value)
                                                       .ToArray();
                            /*
                            MessageBox.Show("evaluando los numero de justo");
                            for (int l = 0; l < numProdJusto.Length; l++)
                            {
                                MessageBox.Show("numero justo: " + numProdJusto[l]);
                            }

                            MessageBox.Show("evaluando los numero de tm");
                            for (int l = 0; l < numProducto.Length; l++)
                            {
                                MessageBox.Show("numero prod tm: " + numProducto[l]);
                            }
                            */
                            int coincidenciasNum = 0;
                            if (numProdJusto.Length > 0)
                            {
                                for (int v = 0; v < numProdJusto.Length; v++)
                                {
                                    if (numProducto.Contains(numProdJusto[v]))
                                    {
                                        coincidenciasNum++;
                                    }
                                    else
                                    {
                                        Console.WriteLine("no contiene: " + numProdJusto[v]);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("no contiene números ");
                            }
                            contadorCoincidencias = coincidenciasNum;
                            //MessageBox.Show("otro contador: " + contadorCoincidencias);
                            if (contadorCoincidencias >= 1)
                            {
                                precioProd = float.Parse(justoList[i].array[j].precio.Substring(1));
                                contadorCoincidenciasProds++;
                            }

                        }

                        precioAucumulado = precioAucumulado + precioProd;
                    }
                }

                float precioTM = float.Parse(tuMarchanteList[i].array[0].precio);
                status = "";
                if (contadorCoincidenciasProds == 0)
                {
                    status = "No vende";
                    precioPromedioJusto = 0;
                }
                else
                {
                    precioPromedioJusto = precioAucumulado / contadorCoincidenciasProds;
                    if (precioTM > precioPromedioJusto)
                    {
                        status = "+Barato";
                    }
                    else
                    {
                        status = "+Caro";
                    }
                }


                //EVALUANDO CENTRAL EN LINEA
                
                float precioPromedioCentral = 0;
                string statusCentral = "";

                float precioAucumuladoCentral = 0;

                int contadorCoincidenciasCentral = 0;
                float precioProdCentral = 0;
                int contadorCoincidenciasProdsCentral = 0;
                if(centralList.Count > 0)
                {
                    for (int j = 0; j < 1; j++)
                    {
                        string productoCentral = centralList[i].array[j].producto;
                        //MessageBox.Show(productoJusto);
                        //string[] prodCentralAux = productoCentral.Split(" ");
                        string[] prodCentralAux = productoCentral.Split(new[] { " " }, StringSplitOptions.None);

                        for (int k = 0; k < prodCentralAux.Length; k++)
                        {
                            string word = prodCentralAux[k].ToUpper();
                            string palabaSinTildes = Regex.Replace(word.Normalize(System.Text.NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
                            //MessageBox.Show("palabras central:" + palabaSinTildes);
                            if (prodductoNAux.Contains(palabaSinTildes))
                            {
                                contadorCoincidenciasCentral++;
                            }
                        }

                        if (contadorCoincidenciasCentral >= ((prodductoNAux.Count * 70) / 100))
                        {
                            precioProdCentral = float.Parse(centralList[i].array[j].precio.Substring(1));
                            contadorCoincidenciasProdsCentral++;
                        }
                        else
                        {
                            //MessageBox.Show("No hubo coincidencias de palabras de mas del 70%");

                            string patron = @"(?:- *)?\d+(?:\.\d+)?";
                            Regex regex = new Regex(patron);

                            string[] numProdCentral = regex.Matches(productoCentral)
                                                       .OfType<Match>()
                                                       .Select(m => m.Value)
                                                       .ToArray();

                            string[] numProducto = regex.Matches(auxCadena[i])
                                                       .OfType<Match>()
                                                       .Select(m => m.Value)
                                                       .ToArray();
                            
                            int coincidenciasNum = 0;
                            if(numProdCentral.Length > 0)
                            {
                                for (int v = 0; v < numProdCentral.Length; v++)
                                {
                                    //MessageBox.Show(numProdCentral[v].ToString());
                                    if (numProducto.Contains(numProdCentral[v]))//error
                                    {
                                        coincidenciasNum++;
                                    }
                                    else
                                    {
                                        Console.WriteLine("no contiene: " + numProdCentral[v]);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("no contiene números");
                            }
                            
                            contadorCoincidenciasCentral = coincidenciasNum;
                            //MessageBox.Show("otro contador: " + contadorCoincidencias);
                            if (contadorCoincidenciasCentral >= 1)
                            {
                                precioProdCentral = float.Parse(centralList[i].array[j].precio.Substring(1));
                                contadorCoincidenciasProdsCentral++;
                            }

                        }

                        precioAucumuladoCentral = precioAucumuladoCentral + precioProdCentral;
                    }
                }
                                
                statusCentral = "";
                if (contadorCoincidenciasProdsCentral == 0)
                {
                    statusCentral = "No vende";
                    precioPromedioCentral = 0;
                }
                else
                {
                    precioPromedioCentral = precioAucumuladoCentral / contadorCoincidenciasProdsCentral;
                    if (precioTM > precioPromedioCentral)
                    {
                        statusCentral = "+Barato";
                    }
                    else
                    {
                        statusCentral = "+Caro";
                    }
                }


                //EVALUANDO CHEDRAUI

                float precioPromedioChedraui = 0;
                string statusChedraui = "";

                float precioAucumuladoChedraui = 0;

                int contadorCoincidenciasChedraui = 0;
                float precioProdChedraui = 0;
                int contadorCoincidenciasProdsChedraui = 0;
                if (chedrauiList.Count > 0)
                {
                    for (int j = 0; j < 1; j++)
                    {
                        string productoChedraui = chedrauiList[i].array[j].producto;
                        //MessageBox.Show(productoJusto);
                        //string[] prodChedrauiAux = productoChedraui.Split(" ");
                        string[] prodChedrauiAux = productoChedraui.Split(new[] { " " }, StringSplitOptions.None);

                        for (int k = 0; k < prodChedrauiAux.Length; k++)
                        {
                            string word = prodChedrauiAux[k].ToUpper();
                            string palabaSinTildes = Regex.Replace(word.Normalize(System.Text.NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
                            //MessageBox.Show("palabras central:" + palabaSinTildes);
                            if (prodductoNAux.Contains(palabaSinTildes))
                            {
                                contadorCoincidenciasChedraui++;
                            }
                        }

                        if (contadorCoincidenciasChedraui >= ((prodductoNAux.Count * 70) / 100))
                        {
                            string precioChedraui = (chedrauiList[i].array[j].precio).Trim();

                            precioProdChedraui = float.Parse(precioChedraui.Substring(1));
                            contadorCoincidenciasProdsChedraui++;
                        }
                        else
                        {
                            //MessageBox.Show("No hubo coincidencias de palabras de mas del 70%");

                            string patron = @"(?:- *)?\d+(?:\.\d+)?";
                            Regex regex = new Regex(patron);

                            string[] numProdChedraui = regex.Matches(productoChedraui)
                                                       .OfType<Match>()
                                                       .Select(m => m.Value)
                                                       .ToArray();

                            string[] numProducto = regex.Matches(auxCadena[i])
                                                       .OfType<Match>()
                                                       .Select(m => m.Value)
                                                       .ToArray();

                            int coincidenciasNum = 0;
                            if (numProdChedraui.Length > 0)
                            {
                                for (int v = 0; v < numProdChedraui.Length; v++)
                                {
                                    if (numProducto.Contains(numProdChedraui[v]))
                                    {
                                        coincidenciasNum++;
                                    }
                                    else
                                    {
                                        Console.WriteLine("no contiene: " + numProdChedraui[v]);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("no contiene números ");
                            }
                            contadorCoincidenciasChedraui = coincidenciasNum;
                            //MessageBox.Show("otro contador: " + contadorCoincidencias);
                            if (contadorCoincidenciasChedraui >= 1)
                            {
                                string precioChedraui = (chedrauiList[i].array[j].precio).Trim();
                                precioProdChedraui = float.Parse(precioChedraui.Substring(1));
                                contadorCoincidenciasProdsChedraui++;
                            }

                        }

                        precioAucumuladoChedraui = precioAucumuladoChedraui + precioProdChedraui;
                    }
                }
                    
                statusChedraui = "";
                if (contadorCoincidenciasProdsChedraui == 0)
                {
                    statusChedraui = "No vende";
                    precioPromedioChedraui = 0;
                }
                else
                {
                    precioPromedioChedraui = precioAucumuladoChedraui / contadorCoincidenciasProdsChedraui;
                    if (precioTM > precioPromedioChedraui)
                    {
                        statusChedraui = "+Barato";
                    }
                    else
                    {
                        statusChedraui = "+Caro";
                    }
                }
                
                //Sardina Calmex 425 gr, Lata de atún Tuny en agua de 140 g
                //COMPARATIVO PRECIOS COSTOS
                //COMPARATIVO PRECIOS COSTOS
                string sku = skuProductos[i].Trim(' ');
                MessageBox.Show("SKU ["+i.ToString() + "] - "+sku);
                float precioTMSinImp = 0;
                for (int n = 0; n < productoCostos.Count; n++)
                {
                    string skuAux = productoCostos[n].sku.Trim(' ');
                    string tipoImpuesto = productoCostos[n].tipoImpuesto;
                    if (sku == skuAux)
                    {
                        if(tipoImpuesto == "1")//16%
                        {
                            precioTMSinImp = (84 * precioTM) / 100;
                        }
                        else
                        {
                            if (tipoImpuesto == "11")//8%
                            {
                                precioTMSinImp = (8 * precioTM) / 100;
                            }
                            else//tasa 0
                            {
                                precioTMSinImp = precioTM;
                            }
                        }
                    }
                }

                float minimo = 0;
                
                List<float> listPromedios = new List<float>();
                listPromedios.Add(precioPromedioJusto);
                listPromedios.Add(precioPromedioCentral);
                listPromedios.Add(precioPromedioChedraui);
                
                for (int g = 0; g < listPromedios.Count; g++)
                {
                    float dato = listPromedios[g];
                    if (dato == 0)
                    {
                        listPromedios.RemoveAt(g);
                    }
                }
                if (listPromedios.Count == 2)
                {
                    minimo = Math.Min(listPromedios[0], listPromedios[1]);                    
                }
                else
                {
                    float minimoAux = Math.Min(listPromedios[0], listPromedios[1]);                    
                    minimo = Math.Min(minimoAux, listPromedios[2]);                  
                }
                

                float precioSugerido = 0;
                string comentarios = "No se puede evaluar";
                float costoProducto = 0;
                float utilidad = 0;
                float porcUtilidad = 0;
                float margen = 0;

                for (int j = 0; j < productoCostos.Count; j++)
                {
                    string skuAux = productoCostos[j].sku.Trim(' ');
                    //MessageBox.Show("sku de la lista cargada: "+skuAux);

                    string costoAux = productoCostos[j].costoProm;
                    if (sku == skuAux)
                    {
                        //MessageBox.Show("Es igual");
                        costoProducto = float.Parse(costoAux);
                    }
                }

                MessageBox.Show("Costo ["+i+"] - " + costoProducto);
                if (precioTM < minimo)
                {
                    precioSugerido = minimo - 1;
                    comentarios = "Se sugiere subir el precio, manteniendose por debajo del precio minimo de la competencia";
                    margen = precioTMSinImp - costoProducto;
                }
                else
                {                                       

                    Thread.Sleep(4000);
                    if(costoProducto != 0)
                    {
                        if (precioTMSinImp > costoProducto)
                        {
                            margen = precioTMSinImp - costoProducto;
                            if (margen > ((costoProducto * 1.15) - costoProducto))
                            {
                                //precioSugerido = precioTMSinImp;
                                //float precioSugeridoAux = minimo - 1;
                                precioSugerido = minimo - 1;
                                comentarios = "Se sugiere bajar el precio para poder competir y mantener un margen del 15%.";
                            }
                            else
                            {
                                precioSugerido = costoProducto * 1.15F;
                                comentarios = "Se sugiere subir el precio para mantener un margen del 15%.";
                            }
                        }
                        else
                        {
                            if (precioTMSinImp <= costoProducto)
                            {
                                precioSugerido = costoProducto * 1.15F;
                                comentarios = "El costo es mayor al precio de venta, se sube el precio.";
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("SKU no disponible, no se puede evaluar");
                        precioSugerido = 00000;  
                    }
                    
                }


                utilidad = precioTMSinImp - costoProducto;
                porcUtilidad = (margen * 100) / precioTMSinImp;

                //*************************************************************************
                


                //*************************************************************************
                dataGridView5.Rows.Add(auxCadena[i] + "=> $" + precioTM,status + ": $" + precioPromedioJusto.ToString(), statusCentral + ": $" + precioPromedioCentral.ToString(), statusChedraui + ": $" + precioPromedioChedraui.ToString(), "$"+precioTMSinImp.ToString(), "$" + costoProducto.ToString(), "$" +utilidad.ToString(),porcUtilidad.ToString()+"%", '$' + precioSugerido.ToString());
                //dataGridView5.Rows.Add(auxCadena[i] + "=> $" + precioTM, status + ": $" + precioPromedioJusto.ToString(), statusCentral + ": $" + precioPromedioCentral.ToString(), statusChedraui + ": $" + precioPromedioChedraui.ToString());
                
            }


        }

        /*
         Sardina Calmex 425 gr, Lata de atún Tuny en agua de 140 g, Crema de cacahuate Aladino cremosa 425g,
        Gerber ciruela pasa 113 g, Croquetas Perrón Adulto de 25 kg, Aceite de cártamo Oléico 946 ml, 
        Alimento para bebé de sabor durazno Gerber 113 g, Alimento para bebé de sabor frutas Mixtas Gerber 113 g, 
        Alimento para bebé de sabor frutas tropicales Gerber 113 g, Alimento para bebé de sabor guayaba Gerber 113 g,
        Alimento para bebé de sabor mango Gerber 113 g, Alimento para bebé de sabor manzana Gerber 113 g
         */

        static dataProductos createAddProd(string name, string count) //A simple animal element setup method
        {
            dataProductos newProduct = new dataProductos();
            newProduct.producto = name;   //Set animal name
            newProduct.precio = count; //Set animals count

            return newProduct;
        }
        static dataProductosCostos createAddProdCostos(string sku, string costo, string tipoImp) //A simple animal element setup method
        {
            dataProductosCostos newProduct = new dataProductosCostos();
            newProduct.sku = sku;   //Set animal name
            newProduct.costoProm = costo; //Set animals count
            newProduct.tipoImpuesto = tipoImp;

            return newProduct;
        }

        static prodPrincipal createAddProPrinc(string name, List<dataProductos> array) //A simple animal element setup method
        {
            prodPrincipal newProdPrincipal = new prodPrincipal();
            newProdPrincipal.producto = name;   //Set animal name
            newProdPrincipal.array = array; //Set animals count

            return newProdPrincipal;
        }
        class dataProductos //You can use "class" or "struct"
        {
            public string producto;
            public string precio;
        }
        class dataProductosCostos
        {
            public string sku;
            public string costoProm;
            public string tipoImpuesto;
        }
        class prodPrincipal
        {
            public string producto;
            public List<dataProductos> array;
        }

        //Botón elegir documento
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlFicheroCSV = new OpenFileDialog();
            dlFicheroCSV.Title = "Abrir fichero CSV...";
            //dlFicheroCSV.InitialDirectory = @"C:\";
            dlFicheroCSV.Filter = "Ficheros de texto y CSV (*.txt, *.csv)|*.txt;*.csv|" +
                "Ficheros de texto (*.txt)|*.txt|" +
                "Ficheros CSV (*.csv)|*.csv|" +
                "Todos los ficheros (*.*)|*.*";
            dlFicheroCSV.FilterIndex = 1;
            dlFicheroCSV.RestoreDirectory = true;
            if (dlFicheroCSV.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dlFicheroCSV.FileName;
            }
        }

   
        //BOTÓN LEER CSV
        private async void button3_Click_1(object sender, EventArgs e)
        {
            /*
            if (File.Exists(textBox2.Text))
            {
                try
                {
                    string txtSeparador = ",";
                    CargarDatosCSV(textBox2.Text,Convert.ToChar(txtSeparador),"'");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Error al leer CSV...",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No existe el fichero CSV seleccionado.",
                    "Fichero no encontrado...",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            */

            RestAPI rest = new RestAPI("https://www.tumarchante.mx/wp-json/wc/v3/", "ck_26296d3d60aaa31c32ff9134564491a7b3608e19", "cs_734ae1d4c08e0208f4c29e146b039f1fdc5b8203");
            WCObject wc = new WCObject(rest);

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("per_page", "100");
            long pageNumber = 1;
            dic.Add("page", pageNumber.ToString());
            List<Product> products = new List<Product>();
            bool endWhile = false;
            while (!endWhile)
            {
                //var productsTemp = wc.GetProducts(dic);                
                try
                {
                    var productsTemp = await wc.Product.GetAll(dic);                    

                    //MessageBox.Show("Número de pagina: "+pageNumber.ToString());
                    //MessageBox.Show("ProductsTemp Count: " + productsTemp.Count);

                    if (pageNumber > 99)
                    {
                        MessageBox.Show("Count: " + productsTemp.Count.ToString() + "------ PageNumber?: " + pageNumber.ToString());
                    }


                    if (productsTemp.Count > 0)
                    {
                        products.AddRange(productsTemp);
                        pageNumber++;
                        dic["page"] = pageNumber.ToString();
                    }
                    else
                    {
                        endWhile = true;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error en: " + pageNumber.ToString());
                    //throw;
                    //endWhile = true;
                }
            }

            MessageBox.Show("List Products Count: " + products.Count);
            foreach (var p in products)
            {
                MessageBox.Show("ID: " + p.id + "- Sku :" + p.sku + "- Nombre: " + p.name + "- Precio: " + p.sale_price);
            };


        }



        //Carga un fichero CSV en un DataGridView
        List<dataProductosCostos> productoCostos = new List<dataProductosCostos>();
        private void CargarDatosCSV(string ficheroCSV, char separador,string separadorCampos)
        {
            bool primeraLineaTitulo = true;
            dataGridView6.DataSource = null;
            dataGridView6.Rows.Clear();

            DataTable tablaDatos = new DataTable();
            string[] lineas = File.ReadAllLines(ficheroCSV, Encoding.UTF8);

            if (lineas.Length > 0)
            {
                //Si la primea línea contiene el título  
                string[] etiquetaTitulosFinal;
                if (primeraLineaTitulo)
                {
                    string primelaLinea = lineas[0];
                    string[] etiquetaTitulos = primelaLinea.Split(separador);
                    List<string> lista = new List<string>();
                    foreach (string campoActual in etiquetaTitulos)
                    {
                        string valor = campoActual;
                        // Quitamos el posible carácter de inicio y fin de valor
                        /*
                        if (separadorCampos != "")
                        {
                            valor = valor.TrimEnd(Convert.ToChar(separadorCampos));
                            valor = valor.TrimStart(Convert.ToChar(separadorCampos));                         
                        }
                        */
                        tablaDatos.Columns.Add(new DataColumn(valor));
                        lista.Add(valor);
                    }
                    etiquetaTitulosFinal = lista.ToArray();
                }
                else
                {
                    string primelaLinea = lineas[0];
                    string[] etiquetaTitulos = primelaLinea.Split(separador);
                    int numero = 0;
                    List<string> lista = new List<string>();
                    foreach (string campoActual in etiquetaTitulos)
                    {
                        string valor = "C" + Convert.ToString(numero);  
                        tablaDatos.Columns.Add(new DataColumn(valor));
                        lista.Add(valor);
                        numero++;
                    }
                    etiquetaTitulosFinal = lista.ToArray();
                }

                //Resto de filas de datos
                int inicioFila = 0;
                if (primeraLineaTitulo)
                    inicioFila = 1;

                for (int i = inicioFila; i < lineas.Length; i++)
                {
                    string[] filasDatos = lineas[i].Split(separador);
                    productoCostos.Add(createAddProdCostos(filasDatos[0], filasDatos[2], filasDatos[3]));
                    DataRow dataGridS = tablaDatos.NewRow();
                    int columnIndex = 0;
                    foreach (string campoActual in etiquetaTitulosFinal)
                    {
                        string valor = filasDatos[columnIndex++];                                             
                        dataGridS[campoActual] = valor;                        
                    }
                    tablaDatos.Rows.Add(dataGridS);
                }
            }

            if (tablaDatos.Rows.Count > 0)
            {
                dataGridView6.DataSource = tablaDatos;
            }
            
        }





























        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        
    }
}
