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
using System.Windows.Shapes;
using BlgConnector;
using NeutralObjects;
using YourProject.Models;
using System.Text.RegularExpressions; // For Number of Shares Validation

namespace YourProject
{
    /// <summary>
    /// Logique d'interaction pour ReportingComplexe.xaml
    /// </summary>
    public partial class ReportingComplexe : Window
    {
        public ReportingComplexe()
        {
            InitializeComponent();

            // Initialize DatePicker control with the date of yesterday & today
            dateFrom.Text = DateTime.Now.AddDays(-1).ToString();
            dateTo.Text = DateTime.Now.ToString();

            // At the begining, the report is empty, there is nothing to print
            PrintButton.IsEnabled = false;
        }

        private void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            bool securityFilled = ((v_ticker1.Text != "") && (v_ticker2.Text != "") && (v_ticker3.Text != "")) ? true : false; // TRUE if v_ticker fields have been indicated

            // We first check if dates are correct (dateFrom < dateTo)
            if ((DateTime.Compare(Convert.ToDateTime(dateFrom.Text), Convert.ToDateTime(dateTo.Text)) < 0) && (DateTime.Compare(Convert.ToDateTime(dateTo.Text), Convert.ToDateTime(DateTime.Now)) < 0))
            {
                if (securityFilled)
                {
                    // Then if all number of shares are positive
                    if ((Convert.ToInt32(v_nbShares1.Text) > 0) && (Convert.ToInt32(v_nbShares2.Text) > 0) && (Convert.ToInt32(v_nbShares3.Text) > 0) &&
                        (v_nbShares1.Text != "") && (v_nbShares2.Text != "") && (v_nbShares3.Text != ""))
                    {
                        try
                        {
                            BlgWrapper wrap = new BlgWrapper();
                            List<string> tickers = new List<string>();
                            List<string> fields = new List<string>();
                            List<int> nbShares = new List<int>();

                            MainModel mm = GridMain.DataContext as MainModel;
                            ResultModel rm = GridResult.DataContext as ResultModel;

                            rm.DateTo = Convert.ToDateTime(dateTo.Text);
                            rm.DateFrom = Convert.ToDateTime(dateFrom.Text);

                            // Adding security & number of shares to the list of data we will download
                            tickers.Add(v_ticker1.Text);
                            tickers.Add(v_ticker2.Text);
                            tickers.Add(v_ticker3.Text);
                            nbShares.Add(Convert.ToInt32(v_nbShares1.Text));
                            nbShares.Add(Convert.ToInt32(v_nbShares2.Text));
                            nbShares.Add(Convert.ToInt32(v_nbShares3.Text));

                            fields.Add("PX_LAST");  // Data obtain from this Bloomberg fields will be plotted on the reporting

                            Dictionary<string, Dictionary<string, List<HistoData>>> res_histo2 = new Dictionary<string, Dictionary<string, List<HistoData>>>();
                            List<string> _ticker;
                            List<List<HistoData>> histoPX = new List<List<HistoData>>();

                            // Download data for each security
                            foreach (string ticker in tickers)
                            {
                                _ticker = new List<string>();
                                _ticker.Add(ticker);
                                if (wrap.GetHistoricalData(_ticker, fields, BlgPeriodOpt.periodicity_sel, BlgPeriods.daily, rm.DateFrom, rm.DateTo))
                                {
                                    res_histo2 = wrap.ResultHisto;
                                    List<HistoData> result = res_histo2[fields.First()].Values.First();
                                    histoPX.Add(result);

                                }
                                else
                                {
                                    MessageBox.Show("No data collected for the security " + ticker);
                                    return;
                                }
                            }

                            // Get the number of rows of the smallest List<HistoData>
                            int numberOfRows = histoPX[0].Count;
                            foreach (List<HistoData> hd in histoPX)
                            {
                                if (hd.Count < numberOfRows)
                                    numberOfRows = hd.Count;
                            }

                            #region COMPUTING THE PERFORMANCE OF THE PORTFOLIO FOR GRAPH

                            List<double> value_portfolio = new List<double>();
                            double sum = 0;
                            for (int i = 0; i < numberOfRows; i++)
                            {
                                sum = 0;
                                for (int j = 0; j < histoPX.Count; j++) //histoPX.Count = number of tickers
                                    sum += histoPX[j][i].Price * nbShares[j];
                                value_portfolio.Add(sum);
                            }

                            List<double> perf_portfolio = new List<double>();
                            perf_portfolio.Add(0);
                            //compute the perf of the portfolio
                            for (int i = 1; i < numberOfRows; i++)
                                perf_portfolio.Add((value_portfolio[i] / value_portfolio[i - 1]) - 1);

                            rm.Donnees1 = new List<HistoData>();

                            for (int i = 0; i < perf_portfolio.Count; i++)
                            {
                                HistoData data = new HistoData(res_histo2[fields.First()].Values.First()[i].Date, perf_portfolio[i]);
                                rm.Donnees1.Add(data);
                            }

                            graph_perf.ItemsSource = null;
                            graph_perf.Items.Clear();
                            graph_perf.ItemsSource = rm.DonneesChartable;

                            #endregion

                            #region COMPUTING THE VOLATILITY OF THE PORTFOLIO FOR GRAPH

                            rm.Donnees1 = new List<HistoData>();

                            for (int i = 0; i < numberOfRows; i++)
                            {
                                HistoData data = new HistoData(res_histo2[fields.First()].Values.First()[i].Date, Volatility(histoPX, nbShares, i));
                                rm.Donnees1.Add(data);
                            }
                            graph_volat.ItemsSource = null;
                            graph_volat.Items.Clear();
                            graph_volat.ItemsSource = rm.DonneesChartable;

                            #endregion

                            #region COMPUTING PERFOMANCE OF EACH SECURITY FOR GRAPH                  
                            // Perf of first security
                            rm.Donnees1 = new List<HistoData>();
                            HistoData hdTMP = new HistoData(histoPX[0][0].Date, 0);
                            rm.Donnees1.Add(hdTMP);
                            for (int i = 1; i < histoPX[0].Count; i++)
                            {
                                hdTMP = new HistoData(histoPX[0][i].Date, (histoPX[0][i].Price / histoPX[0][i - 1].Price) - 1);
                                rm.Donnees1.Add(hdTMP);
                            }

                            graph_1.ItemsSource = null;
                            graph_1.Items.Clear();
                            graph_1.ItemsSource = rm.DonneesChartable;

                            // Perf of second security
                            rm.Donnees1 = new List<HistoData>();
                            hdTMP = new HistoData(histoPX[1][0].Date, 0);
                            rm.Donnees1.Add(hdTMP);
                            for (int i = 1; i < histoPX[1].Count; i++)
                            {
                                hdTMP = new HistoData(histoPX[1][i].Date, (histoPX[1][i].Price / histoPX[1][i - 1].Price) - 1);
                                rm.Donnees1.Add(hdTMP);
                            }

                            graph_2.ItemsSource = null;
                            graph_2.Items.Clear();
                            graph_2.ItemsSource = rm.DonneesChartable;

                            // Perf of third security
                            rm.Donnees1 = new List<HistoData>();
                            hdTMP = new HistoData(histoPX[2][0].Date, 0);
                            rm.Donnees1.Add(hdTMP);
                            for (int i = 1; i < histoPX[2].Count; i++)
                            {
                                hdTMP = new HistoData(histoPX[2][i].Date, (histoPX[2][i].Price / histoPX[2][i - 1].Price) - 1);
                                rm.Donnees1.Add(hdTMP);
                            }
                            graph_3.ItemsSource = null;
                            graph_3.Items.Clear();
                            graph_3.ItemsSource = rm.DonneesChartable;

                            #endregion

                            #region COMPUTING THE INITIAL AND LAST VALUES OF THE PORTFOLIO AND THE P&L
                            double price_portfolio_begin = 0;
                            for (int i = 0; i < histoPX.Count; i++)
                                price_portfolio_begin += histoPX[i][0].Price * nbShares[i];
                            double price_portfolio_end = price_portfolio_begin;
                            for (int i = 0; i < perf_portfolio.Count; i++)
                                price_portfolio_end *= 1 + perf_portfolio[i];

                            v_priceportofolio_end.Text = Math.Round(price_portfolio_end, 2).ToString(); // We keep two significant digits
                            v_priceportofolio_begin.Text = Math.Round(price_portfolio_begin, 2).ToString();
                            // Compute P&L = Last_value - Initial_value
                            v_pl.Text = Math.Round(price_portfolio_end - price_portfolio_begin, 2).ToString();
                            if (price_portfolio_begin - price_portfolio_end >= 0)
                                v_pl.Background = Brushes.Red; // Case of negative P&L
                            else
                                v_pl.Background = Brushes.Green; // Case of postivie P&L
                            #endregion

                            // Once the reporting is complete, we can print it
                            PrintButton.IsEnabled = true;

                        }
                        catch (Exception error)
                        {
                            MessageBox.Show(error.Message);
                        }

                    }
                    else
                        MessageBox.Show("Enter positive values for the number of parts !");
                }
                else
                    MessageBox.Show("Please fill in all the required fields");
            }
            else
                MessageBox.Show("Please verify dates");
        }

        /*----------------------------------------
         *           FUNCTIONS
         *--------------------------------------*/
        #region FUNCTIONS
        // Allow the user to enter only integer in the Textblocks v_nbShares
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Compute the mean of a list
        double Mean(List<HistoData> list)
        {
            double sum = 0;
            for (int i = 0; i < list.Count; i++)
                sum += list[i].Price;
            return sum / list.Count;
        }

        // Compute the covariance
        double Cov(List<HistoData> x, List<HistoData> y)
        {
            double sum = 0;
            double mean_x = Mean(x);
            double mean_y = Mean(y);
            for (int i = 0; i < x.Count; i++)
                sum += (x[i].Price - mean_x) * (y[i].Price - mean_y);
            return sum / x.Count;
        }

        // Compute Volatillity
        double Volatility(List<List<HistoData>> histoPX, List<int> nbShares, int l)
        {
            double sum = 0;
            List<double> weigths;
            double current_portfolio_value;

            //histoPX.Count is supposed to be equal to nbShares.Count (obviously)
            for (int i = 0; i < histoPX.Count; i++)
            {
                current_portfolio_value = 0;
                weigths = new List<double>();

                #region Compute the new weigth for each share
                for (int j = 0; j < histoPX.Count; j++)
                {
                    weigths.Add(histoPX[j][i].Price * nbShares[j]);
                    current_portfolio_value += histoPX[j][i].Price * nbShares[j];
                }
                for (int j = 0; j < histoPX.Count; j++)
                    weigths[i] = weigths[i] / current_portfolio_value;
                #endregion

                // Formula for the volatility of the portfolio
                for (int k = 0; k < histoPX.Count; k++)
                    sum += weigths[i] * weigths[k] * Cov(histoPX[i].GetRange(0, l), histoPX[k].GetRange(0, l));
            }
            return Math.Sqrt(sum);
        }

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
        #endregion
    }
}
