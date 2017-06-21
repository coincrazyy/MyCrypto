using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using HtmlAgilityPack;
using System.Reflection;
using System.Speech.Synthesis;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Collections;

namespace MyCrypto
{
    public partial class Form1 : Form
    {
        Hashtable _myStash = new Hashtable();
        bool _useDollar = true;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SayIt(true);
        }

        private void OpenStash()
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var stashFileName = appPath.Substring(0, appPath.LastIndexOf("\\") + 1) + "stash.txt";
            Process.Start("notepad.exe", stashFileName);
        }

        private void ReadStash()
        {
            _myStash.Clear();
            string line = "";
            try
            {
                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var stashFileName = appPath.Substring(0, appPath.LastIndexOf("\\") + 1) + "stash.txt";
                StreamReader sr = new StreamReader(stashFileName);
                while ((line = sr.ReadLine()) != null)
                {
                    string coinName = line.Substring(0, line.IndexOf(" "));
                    if (coinName == "UseDollar")
                    {
                        _useDollar = line.Substring(line.IndexOf(" "), line.Length - line.IndexOf(" ")).Trim() == "Y";

                    }
                    else
                    {
                        double amount = Double.Parse(line.Substring(line.IndexOf(" "), line.Length - line.IndexOf(" ")).Trim());
                        _myStash.Add(coinName, amount);
                    }
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {

                Console.WriteLine("Executing finally block.");
            }
        }

        double GetNorwegianCrownToUSDollar()
        {
            using (var client = new WebClient())
            {
                string data = client.DownloadString("https://themoneyconverter.com/USD/NOK.aspx");
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                string str = doc.DocumentNode.InnerHtml;
                str = str.Substring(str.IndexOf("NOK/USD = ") + 10);
                str = str.Substring(0, str.IndexOf("<"));
                str = str.Replace(".", ",");
                return double.Parse(str);
            }
        }
        private void SayIt(bool dontSayIt=false)
        {
            ReadStash();
            double norskTotal = 0.00;
            double dollarTotal = 0.00;
            double record = 0.00;
            string strTotal;

            foreach (DictionaryEntry entry in _myStash)
            {
                double price = GetPriceRaw(entry.Key.ToString());
                double amount = Double.Parse(entry.Value.ToString());
                dollarTotal += amount * price;
            }

            if (!_useDollar)
            {
                double conversionRate = GetNorwegianCrownToUSDollar();
                norskTotal = (dollarTotal) * conversionRate;
                norskTotal = Math.Round(norskTotal, 2);
            }

            dollarTotal = Math.Round(dollarTotal, 2);

            if (!_useDollar)
            {
                strTotal = "Your crypto net worth is " + norskTotal.ToString().Replace(",", ".") + " Norwegian crowns or $" + dollarTotal.ToString().Replace(",", ".") + ".";
            }
            else
            {
                strTotal = "Your crypto net worth is $" + dollarTotal.ToString().Replace(",", ".") + ".";
            }

            SetControlPropertyThreadSafe(this, "Text", strTotal);

            if(!dontSayIt)
                ShowMessage(strTotal);

            System.Threading.Thread.Sleep(15000);

            record = double.Parse(ReadDB());
            record = Math.Round(record, 2);
            if (dollarTotal > record)
            {
                ShowMessage("Congratulations! Your crypto assets have reached a new all time high!");
                System.Threading.Thread.Sleep(7000);
                ShowMessage("Your previous high was $" + record.ToString().Replace(",", ".") + ".  Your new high is now $" + dollarTotal.ToString().Replace(",", ".") + "!");
                System.Threading.Thread.Sleep(7000);
                WriteDB(dollarTotal.ToString());
            }

        }

        private delegate void SetControlPropertyThreadSafeDelegate(
    Control control,
    string propertyName,
    object propertyValue);

        public static void SetControlPropertyThreadSafe(
            Control control,
            string propertyName,
            object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate
                (SetControlPropertyThreadSafe),
                new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(
                    propertyName,
                    BindingFlags.SetProperty,
                    null,
                    control,
                    new object[] { propertyValue });
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                mynotifyicon.Visible = true;
                mynotifyicon.ShowBalloonTip(500);
                this.Hide();
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                mynotifyicon.Visible = false;
            }
        }

        private void ShowMessage(string text)
        {
            mynotifyicon.BalloonTipText = text;
            mynotifyicon.BalloonTipIcon = ToolTipIcon.Info;
            mynotifyicon.ShowBalloonTip(20000);
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Teen);
            synthesizer.Volume = 100;  // 0...100
            synthesizer.Rate = -2;     // -10...10
            synthesizer.SpeakAsync(text);
        }

        private void mynotifyicon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SayIt();
        }

        private void mynotifyicon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {

            }
        }
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private void Form1_Load(object sender, EventArgs e)
        {
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();

            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "Bitcoin";

            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "Ethereum";

            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem3.Index = 0;
            this.menuItem3.Text = "Litecoin";

            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem4.Index = 0;
            this.menuItem4.Text = "E&xit";

            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem5.Index = 0;
            this.menuItem5.Text = "Enter currency name..";

            this.contextMenu1.MenuItems.AddRange(
       new System.Windows.Forms.MenuItem[] { this.menuItem1, this.menuItem2, this.menuItem3, this.menuItem4, this.menuItem5 });
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            mynotifyicon.ContextMenu = this.contextMenu1;
            ReadStash();
            ReadDB();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1800000;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }
        private void menuItem4_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            this.Close();
        }

        private void WriteDB(string value)
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var app = appPath.Substring(0, appPath.LastIndexOf("\\") + 1) + "db.txt";
            File.WriteAllText(app, String.Empty);
            File.WriteAllText(app, value);
        }

        private string ReadDB()
        {
            string line = "";
            try
            {
                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var app = appPath.Substring(0, appPath.LastIndexOf("\\") + 1) + "db.txt";
                FileStream fs = new FileStream(app, FileMode.OpenOrCreate);
                fs.Close();
                StreamReader sr = new StreamReader(app);
                line = sr.ReadLine();
                sr.Close();

                if (line == null)
                {
                    WriteDB("0");
                    line = "0";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {

                Console.WriteLine("Executing finally block.");
            }
            return line;
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            GetPrice("Bitcoin");
        }

        private void menuItem5_Click(object Sender, EventArgs e)
        {

            var value = Microsoft.VisualBasic.Interaction.InputBox("Name of crypto currency?", "Enter whatever currency you want", "Bitcoin");
            GetPrice(value);
        }

        private void GetPrice(string coin)
        {
            double price = 0.00;

            using (var client = new WebClient())
            {
                string data = client.DownloadString("http://coinmarketcap.com/currencies/" + coin + "/");
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                var str = doc.DocumentNode.InnerHtml;
                str = str.Substring(str.IndexOf("</title>"), str.Length - str.IndexOf("</title>"));
                str = str.Substring(str.IndexOf("document.title = "), str.Length - str.IndexOf("document.title = "));
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                coin = textInfo.ToTitleCase(coin);
                str = str.Substring(str.IndexOf(coin + " ("), str.Length - str.IndexOf(coin + " ("));
                str = str.Substring(str.IndexOf("$"));
                str = str.Substring(1, str.IndexOf(" "));
                str = str.Replace(".", ",");
                price = double.Parse(str);

                var returnString = coin + " is currently priced at $" + price.ToString().Replace(",", ".") + " per " + coin;

                SetControlPropertyThreadSafe(this, "Text", returnString);
                ShowMessage(returnString);
            }
        }

        private double GetPriceRaw(string coin)
        {
            double price = 0.00;

            using (var client = new WebClient())
            {
                string data = client.DownloadString("http://coinmarketcap.com/currencies/" + coin + "/");
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                var str = doc.DocumentNode.InnerHtml;
                str = str.Substring(str.IndexOf("</title>"), str.Length - str.IndexOf("</title>"));
                str = str.Substring(str.IndexOf("document.title = "), str.Length - str.IndexOf("document.title = "));
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                coin = textInfo.ToTitleCase(coin);
                str = str.Substring(str.IndexOf(coin + " ("), str.Length - str.IndexOf(coin + " ("));
                str = str.Substring(str.IndexOf("$"));
                str = str.Substring(1, str.IndexOf(" "));
                str = str.Replace(".", ",");
                price = double.Parse(str);

                return price;
            }
        }


        private void menuItem2_Click(object Sender, EventArgs e)
        {
            GetPrice("Ethereum");
        }

        private void menuItem3_Click(object Sender, EventArgs e)
        {
            GetPrice("Litecoin");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenStash();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ReadStash();
        }
    }
}
