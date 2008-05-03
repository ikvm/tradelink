using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using TradeLib;
using System.IO;

    namespace WinGauntlet
{
    public partial class Gauntlet : Form
    {
        Box mybox;
        BackTest bt;
        NewsService ns = new NewsService();
        public Gauntlet()
        {
            InitializeComponent();
            ns.NewsEventSubscribers += new NewsDelegate(ns_NewsEventSubscribers);
            FindStocks((WinGauntlet.Properties.Settings.Default.tickfolder == null) ? @"c:\program files\tradelink\tickdata\" : WinGauntlet.Properties.Settings.Default.tickfolder);
            UpdateBoxes(Util.GetBoxList((WinGauntlet.Properties.Settings.Default.boxdll== null) ? "box.dll" : WinGauntlet.Properties.Settings.Default.boxdll));
            exchlist.SelectedItem = "NYS";
            ProgressBar1.Enabled = false;
            FormClosing+=new FormClosingEventHandler(Gauntlet_FormClosing);
            Grids();
        }

        void Grids()
        {
            trades.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            trades.Columns.Add("date", "Date");
            trades.Columns.Add("time", "Time");
            trades.Columns.Add("sym", "Symbol");
            trades.Columns.Add("size", "XSize");
            trades.Columns.Add("side", "Side");
            trades.Columns.Add("price", "XPrice");
            trades.Columns.Add("user", "Comment");
            

            orders.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            orders.Columns.Add("date", "Date");
            orders.Columns.Add("time", "Time");
            orders.Columns.Add("sym", "Symbol");
            orders.Columns.Add("type", "Type");
            orders.Columns.Add("size", "Size");
            orders.Columns.Add("side", "Side");
            orders.Columns.Add("price", "Price");
            orders.Columns.Add("user", "Comment");
        }


        void ns_NewsEventSubscribers(News news)
        {
            if (news.Msg.Contains(mybox.Name))
                show(Environment.NewLine + news.Msg);
            else
                show(news.Msg);
        }

        void FindStocks(string path)
        {
            // build list of available stocks and dates available
            stocklist.Items.Clear();
            yearlist.Items.Clear();
            daylist.Items.Clear();
            monthlist.Items.Clear();
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo [] fi = di.GetFiles("*.epf");
            for (int i = 0; i < fi.Length; i++)
            {
                Stock s = StockFromFileName(fi[i].Name);
                DateTime d = Util.ToDateTime(s.Date);
                if (!stocklist.Items.Contains(s.Symbol))
                    stocklist.Items.Add(s.Symbol);
                if (!yearlist.Items.Contains(d.Year))
                    yearlist.Items.Add(d.Year);
                if (!monthlist.Items.Contains(d.Month))
                    monthlist.Items.Add(d.Month);
                if (!daylist.Items.Contains(d.Day))
                    daylist.Items.Add(d.Day);
            }
        }

        

        Stock StockFromFileName(string filename)
        {
            string symbol = System.Text.RegularExpressions.Regex.Match(filename, "([a-z]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Result("$1");
            Stock s = new Stock(symbol);
            s.Date = Convert.ToInt32(System.Text.RegularExpressions.Regex.Match(filename, "([0-9]+)").Result("$1"));
            return s;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // tick folder
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.Description = "Select the folder containing tick files";
            fd.SelectedPath = (WinGauntlet.Properties.Settings.Default.tickfolder == null) ? @"c:\program files\tradelink\tickdata\" : WinGauntlet.Properties.Settings.Default.tickfolder;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                WinGauntlet.Properties.Settings.Default.tickfolder = fd.SelectedPath;
                FindStocks(fd.SelectedPath);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // box DLL selection
            OpenFileDialog of = new OpenFileDialog();
            of.CheckPathExists = true;
            of.CheckFileExists = true;
            of.DefaultExt = ".dll";
            of.Filter = "Box DLL|*.dll";
            of.Multiselect = false;
            if (of.ShowDialog() == DialogResult.OK)
            {
                WinGauntlet.Properties.Settings.Default.boxdll = of.FileName;
                UpdateBoxes(Util.GetBoxList(of.FileName));
            }
        }

        void UpdateBoxes(List<string> boxes)
        {
            boxlist.Items.Clear();
            for (int i = 0; i < boxes.Count; i++)
                boxlist.Items.Add(boxes[i]);
        }

        private void messages_DoubleClick(object sender, EventArgs e)
        {
            messages.Clear();
        }

        delegate void ShowCallBack(string msg);
        void show(string message)
        {
            if (messages.InvokeRequired)
                Invoke(new ShowCallBack(show), new object[] { message });
            else
            {
                messages.AppendText(message);
                if (message.Contains(Environment.NewLine)) lastmessage.Text = message.Replace(Environment.NewLine,"");
                else lastmessage.Text = lastmessage.Text + message;
            }
        }

        

        private void queuebut_Click(object sender, EventArgs e)
        {
            bt = new BackTest(ns);
            if (cleartrades.Checked) trades.Rows.Clear();
            if (clearorders.Checked) orders.Rows.Clear();
            if (clearmessages.Checked) messages.Clear();
            //bt.mybroker.GotFill += new FillDelegate(mybroker_GotFill);
            //bt.mybroker.GotOrder += new OrderDelegate(mybroker_GotOrder);
            List<FileInfo> tf = new List<FileInfo>();
            DirectoryInfo di = new DirectoryInfo(WinGauntlet.Properties.Settings.Default.tickfolder);
            FileInfo [] fi = di.GetFiles("*.EPF");
            for (int i = 0; i < fi.Length; i++)
            {
                bool datematch = true;
                bool symmatch = true;
                bool ud = usedates.Checked;
                bool us = usestocks.Checked;
                DateTime d = Util.ToDateTime(StockFromFileName(fi[i].Name).Date);
                if (ud)
                {
                    for (int j = 0; j < yearlist.SelectedItems.Count; j++)
                        if ((int)yearlist.SelectedItems[j] == d.Year) { datematch &= true; break; }
                        else datematch &= false;
                    for (int j = 0; j < monthlist.SelectedItems.Count; j++)
                        if ((int)monthlist.SelectedItems[j] == d.Month) { datematch &= true; break; }
                        else datematch &= false;
                    for (int j = 0; j < daylist.SelectedItems.Count; j++)
                        if ((int)daylist.SelectedItems[j] == d.Day) { datematch &= true; break; }
                        else datematch &= false;
                }
                if (us)
                {
                    for (int j = 0; j < stocklist.SelectedItems.Count; j++)
                        if (fi[i].Name.Contains((string)stocklist.SelectedItems[j])) { symmatch = true; break; }
                        else symmatch = false;
                }
           
           
                if ((ud && us && datematch && symmatch) ||
                    (ud && datematch) ||
                    (us && symmatch))
                    tf.Add(fi[i]);
            }
            bt.name = DateTime.Now.ToString("yyyMMdd.HHmm");
            if (mybox != null) { bt.mybox = mybox; bt.mybox.Debug = showdebug.Checked; }
            else { show("You must select a box to run the gauntlet."); return; } 
            string exfilt = "";
            if ((exchlist.SelectedIndices.Count > 0) && 
                !exchlist.SelectedItem.ToString().Contains("NoFilter")) 
                exfilt = (string)exchlist.SelectedItem;
            bt.ExchFilter(exfilt);
            bt.Path = WinGauntlet.Properties.Settings.Default.tickfolder+"\\";
            bt.ProgressChanged += new ProgressChangedEventHandler(bt_ProgressChanged);
            bt.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bt_RunWorkerCompleted);
            ProgressBar1.Enabled = true;
            queuebut.Enabled = false;
            bt.RunWorkerAsync(tf);
        }

        void mybroker_GotOrder(Order o)
        {
            if (orders.InvokeRequired)
                Invoke(new OrderDelegate(mybroker_GotOrder), new object[] { o });
            else
            {
                orders.Rows.Add(o.date, o.time, o.symbol, o.isMarket ? "Mkt" : (o.isStop ? "Stp" : "Lmt"), o.size, o.side, o.price, o.stopp, o.comment);
                orders.AutoResizeColumns();
            }
        }

        void mybroker_GotFill(Trade t)
        {
            if (trades.InvokeRequired)
                Invoke(new FillDelegate(mybroker_GotFill), new object[] { t });
            else
            {
                trades.Rows.Add(t.xdate, t.xtime, t.symbol, t.xsize, t.side, t.xprice, t.comment);
                trades.AutoResizeColumns();
            }
        }


        void bt_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string unique = "";
            if (csvnamesunique.Checked)
                unique = "."+bt.name;
            if (ordersinwind.Checked)
            {
                for (int i = 0; i < bt.mybroker.GetOrderList().Count; i++)
                {
                    Order o = bt.mybroker.GetOrderList()[i];
                    orders.Rows.Add(Util.ToDateTime(o.date).ToString("yyyy/MM/dd"), Util.ToDateTime(o.time, 0).ToString("HH:mm:ss"), o.symbol, o.isMarket ? "Mkt" : (o.isStop ? "Stp" : "Lmt"), o.size, o.side ? "BUY" : "SELL", o.isStop ? o.stopp.ToString("N2") : o.price.ToString("N2"), o.comment);
                }
                orders.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);

            }
            if (tradesinwind.Checked)
            {
                for (int i = 0; i < bt.mybroker.GetTradeList().Count; i++)
                {
                    Trade t = bt.mybroker.GetTradeList()[i];
                    trades.Rows.Add(Util.ToDateTime(t.xdate).ToString("yyyy/MM/dd"), Util.ToDateTime(t.xtime,t.xsec).ToString("HH:mm:ss"), t.symbol, t.xsize, t.side ? "BUY" : "SELL", t.xprice.ToString("N2"), t.comment);
                }
                trades.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            }
            if (tradesincsv.Checked)
                Util.ClosedPLToText(bt.mybroker.GetTradeList(),',',Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+"\\Gauntlet.Trades"+unique+".csv");
            if (ordersincsv.Checked)
            {
                StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Gauntlet.Orders" + unique + ".csv", false);
                for (int i = 0; i < bt.mybroker.GetOrderList().Count; i++)
                    sw.WriteLine(bt.mybroker.GetOrderList()[i].Serialize());
                sw.Close();
            }
            if (indicatorscsv.Checked)
            {
                StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Gauntlet.Indicators"+unique+".csv",false);
                sw.WriteLine(string.Join(",",mybox.IndicatorNames));
                
                for (int i = 0; i< Indicators.Count; i++)
                {
                    List<string> ivals = new List<string>();
                    for (int j = 0; j<Indicators[i].Length; j++)
                        ivals.Add(Indicators[i][j].ToString());
                    sw.WriteLine(string.Join(",",ivals.ToArray()));
                }
                sw.Close();
            }


            
            ProgressBar1.Enabled = false;
            ProgressBar1.Value = 0;
            queuebut.Enabled = true;
        }

        void bt_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar1.Value = e.ProgressPercentage > 100 ? 100 : e.ProgressPercentage;
        }


        private void savesettings_Click(object sender, EventArgs e)
        {
            WinGauntlet.Properties.Settings.Default.Save();
        }

        private void Gauntlet_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (saveonexit.Checked) WinGauntlet.Properties.Settings.Default.Save();
        }

        private void usestocks_CheckedChanged(object sender, EventArgs e)
        {
            stocklist.Enabled = !stocklist.Enabled;
        }

        private void usedates_CheckedChanged(object sender, EventArgs e)
        {
            yearlist.Enabled = !yearlist.Enabled;
            monthlist.Enabled = !monthlist.Enabled;
            daylist.Enabled = !daylist.Enabled;
        }

        private void orders_DoubleClick(object sender, EventArgs e)
        {
            orders.Rows.Clear();
        }

        private void trades_DoubleClick(object sender, EventArgs e)
        {
            trades.Rows.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            WinGauntlet.Properties.Settings.Default.Reset();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WinGauntlet.Properties.Settings.Default.Reload();
        }

        private void boxlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                mybox = Box.FromDLL((string)boxlist.SelectedItem, WinGauntlet.Properties.Settings.Default.boxdll, ns);
            }
            catch (Exception ex) { show("Box failed to load, quitting... (" + ex.Message + (ex.InnerException != null ? ",ex.InnerException.Message" : "") + ")"); }
            mybox.IndicatorUpdate += new ObjectArrayDelegate(mybox_IndicatorUpdate);
        }

        void mybox_IndicatorUpdate(object[] parameters)
        {
            Indicators.Add(parameters);
        }

        public List<object[]> Indicators = new List<object[]>();





    }
}