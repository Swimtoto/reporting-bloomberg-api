using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BlgConnector;
using NeutralObjects;
using YourProject.Models;


namespace YourProject
{
    /// <summary>
    /// Logique d'interaction pour ReportingSimple.xaml
    /// </summary>
    public partial class ReportingSimple : Window
    {
        public ReportingSimple()
        {
            InitializeComponent();

            // Initialize DatePicker control with the date of the yesterday & today
            dateFrom.Text = DateTime.Now.AddDays(-1).ToString();
            dateTo.Text = DateTime.Now.ToString();

            // At the begining, the report is empty, there is nothing to print
            PrintButton.IsEnabled = false;
        }

        private void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            bool securityFilled = (ticker.Text != "") ? true : false; // TRUE if ticker field has been indicated

            if (DateTime.Compare(Convert.ToDateTime(dateFrom.Text), Convert.ToDateTime(dateTo.Text)) < 0)
            {
                if (securityFilled)
                {
                    try
                    {
                        BlgWrapper wrap = new BlgWrapper();
                        List<string> tickers = new List<string>();
                        List<string> fields = new List<string>();
                        List<string> fields2 = new List<string>() { "PX_LAST" };
                        List<double> histoValues = new List<double>();

                        Dictionary<string, Dictionary<string, string>> res_ref = new Dictionary<string, Dictionary<string, string>>();
                        Dictionary<string, Dictionary<string, List<HistoData>>> res_histo = new Dictionary<string, Dictionary<string, List<HistoData>>>();
                        Dictionary<string, Dictionary<string, List<HistoData>>> res_histo2 = new Dictionary<string, Dictionary<string, List<HistoData>>>();

                        MainModel mm = GridMain.DataContext as MainModel;
                        ResultModel rm = GridResult.DataContext as ResultModel;

                        tickers.Add(mm.Ticker);
                        fields.Add("PX_LAST");  // By default, we resquest the security's last price

                        // We need to convert dateTo & dateFrom value into a DateTime object
                        // to be able to use the method GetHistoricalData
                        rm.DateTo = Convert.ToDateTime(dateTo.Text);
                        rm.DateFrom = Convert.ToDateTime(dateFrom.Text);

                        if (wrap.GetHistoricalData(tickers, fields, BlgPeriodOpt.periodicity_sel, BlgPeriods.daily, rm.DateFrom, rm.DateTo))
                        {
                            res_histo = wrap.ResultHisto;
                            rm.Donnees1 = res_histo[fields.First()].Values.First();
                        }
                        else
                        {
                            string message = "";
                            foreach (var kvp in wrap.Errors)
                            {
                                message += kvp.Key + " : " + kvp.Value + Environment.NewLine;
                                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        // Computing the perfomance with the formula we got during the course
                        List<double> perf = new List<double>();
                        perf.Add(0); // We initialize the performance of the first date as 0%
                        for (int i = 1; i < rm.Donnees1.Count; i++)
                            perf.Add((rm.Donnees1[i].Price / rm.Donnees1[i - 1].Price) - 1);

                        for (int i = 0; i < rm.Donnees1.Count; i++)
                            rm.Donnees1[i].Price = perf[i];

                        pcprod.ItemsSource = null;
                        pcprod.Items.Clear();
                        pcprod.ItemsSource = rm.DonneesChartable;

                        #region BLOOMBERG FIELDS
                        /* We add the Bloomberg fields which we need to get the data
                           All these data will be save an ResultModel object
                           Downloading data with GetReferenceData method */
                        fields.Clear();
                        fields.Add("NAME");
                        fields.Add("CIE_DES");
                        fields.Add("PX_LAST");
                        fields.Add("INDUSTRY_SECTOR");
                        fields.Add("PRIMARY_EXCHANGE_NAME");
                        fields.Add("NUM_OF_EMPLOYEES");
                        fields.Add("CURR_ENTP_VAL");
                        fields.Add("RTG_SP_LT_LC_ISSUER_CREDIT");
                        fields.Add("ALL_CALENDAR_DAYS");
                        fields.Add("PREVIOUS_VALUE");
                        fields.Add("PX_OPEN");
                        fields.Add("PREV_CLOSE_VAL");
                        fields.Add("HIGH_52WEEK");
                        fields.Add("LOW_52WEEK");
                        #endregion

                        // Download all Bloomberg data fields
                        if (wrap.GetReferenceData(tickers, fields))
                        {
                            res_ref = wrap.ResultRef;

                            // Get the value with the corresponding Bloomberg field from the wrap
                            rm.Name = res_ref["NAME"].Values.FirstOrDefault();
                            rm.Description = res_ref["CIE_DES"].Values.FirstOrDefault();

                            // Get Description Data
                            rm.Sector = res_ref["INDUSTRY_SECTOR"].Values.FirstOrDefault(); // Sector
                            rm.PrimaryExchange = res_ref["PRIMARY_EXCHANGE_NAME"].Values.FirstOrDefault();
                            rm.Employee = res_ref["NUM_OF_EMPLOYEES"].Values.FirstOrDefault(); // Number of employees
                            rm.CompanyValue = res_ref["CURR_ENTP_VAL"].Values.FirstOrDefault(); // Value of the company
                            rm.Rating = res_ref["RTG_SP_LT_LC_ISSUER_CREDIT"].Values.FirstOrDefault(); // S&P Rating

                            // Get Data Data
                            rm.LastPrice = res_ref["PX_LAST"].Values.FirstOrDefault(); // Last current price
                            rm.Open = res_ref["PX_OPEN"].Values.FirstOrDefault(); // Open price
                            rm.PreviousClose = res_ref["PREV_CLOSE_VAL"].Values.FirstOrDefault(); // Close Price day D-1
                            rm.WKhigh = res_ref["HIGH_52WEEK"].Values.FirstOrDefault(); // Highest price last year
                            rm.WKlow = res_ref["LOW_52WEEK"].Values.FirstOrDefault(); // Lowest price last year
                        }
                        else
                        {
                            string message = "";
                            foreach (var kvp in wrap.Errors)
                            {
                                message += kvp.Key + " : " + kvp.Value + Environment.NewLine;
                                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        // Download Volatility Data (1 year)
                        if (wrap.GetHistoricalData(tickers, fields, BlgPeriodOpt.periodicity_sel, BlgPeriods.daily, DateTime.Now.AddDays(-365), DateTime.Now))
                        {
                            res_histo2 = wrap.ResultHisto;
                            List<HistoData> data = res_histo2[fields.First()].Values.First();
                            foreach (HistoData hd in data)
                                histoValues.Add(hd.Price);

                            // We use the function Volatility() to compute vol
                            rm.Volatility30D = Math.Round(Volatility(histoValues.GetRange(histoValues.Count - 20, 20)), 2).ToString();
                            rm.Volatility90D = Math.Round(Volatility(histoValues.GetRange(histoValues.Count - 60, 60)), 2).ToString();
                            rm.Volatility180D = Math.Round(Volatility(histoValues.GetRange(histoValues.Count - 120, 120)), 2).ToString();
                            rm.Volatility1Y = Math.Round(Volatility(histoValues.GetRange(histoValues.Count - 180, 180)), 2).ToString();
                        }
                        else
                        {
                            string message = "";
                            foreach (var kvp in wrap.Errors)
                            {
                                message += kvp.Key + " : " + kvp.Value + Environment.NewLine;
                                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }
                else
                    MessageBox.Show("Please fill in all the required fields");
            }
            else
                MessageBox.Show("Please verify dates");
        }

        // FUNCTION TO COMPUTE ALL NECESSARY DATA WE NEED
        // MEAN
        double Mean(List<double> list)
        {
            double sum = 0;
            for (int i = 0; i < list.Count; i++)
                sum += list[i];
            return sum / list.Count;
        }

        // VOL = E(xi - E(X))²
        double Volatility(List<double> list)
        {
            double sum = 0;
            double mean = Mean(list);
            for (int i = 0; i < list.Count; i++)
                sum += (list[i] - mean) * (list[i] - mean);
            return Math.Sqrt(sum / list.Count);
        }

        // Function to print de report
        // It opens a Dialog printing windows
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog().GetValueOrDefault(false))
            {
                printDialog.PrintVisual(this, this.Title);
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow fenetre = new MainWindow();
            fenetre.Show();
            this.Close();
        }

    }
}
