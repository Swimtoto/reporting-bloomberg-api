using Bloomberglp.Blpapi;
using NeutralObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlgConnector
{
    public class BlgWrapper
    {
        private Dictionary<string, string> errors;
        public Dictionary<string, string> Errors { get { return errors; } }

        private Dictionary<string, Dictionary<string, string>> result_ref;
        public Dictionary<string, Dictionary<string, string>> ResultRef { get { return result_ref; } }

        private Dictionary<string, Dictionary<string, List<HistoData>>> result_histo;
        public Dictionary<string, Dictionary<string, List<HistoData>>> ResultHisto { get { return result_histo; } }

        private BlgOptions Options { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BlgWrapper()
        {
            result_ref = new Dictionary<string, Dictionary<string, string>>();
            result_histo = new Dictionary<string, Dictionary<string, List<HistoData>>>();
            errors = new Dictionary<string, string>();
            Options = new BlgOptions();
        }

        /// <summary>
        /// Get some historical data
        /// </summary>
        /// <param name="tickers">List of tickers</param>
        /// <param name="fields">List of fields</param>
        /// <param name="periodOpt">Period option</param>
        /// <param name="period">Period parameter</param>
        /// <param name="_startDate">Optional start date (01/01/1901 per default)</param>
        /// <param name="_endDate">Optional end date (today per default)</param>
        /// <returns>True if successed, false if failed. Results are contained into ResultHisto dictionary and error in Errors dictionary.</returns>
        public bool GetHistoricalData(List<string> tickers, List<string> fields, BlgPeriodOpt periodOpt, BlgPeriods period, DateTime? _startDate = null, DateTime? _endDate = null)
        {
            try
            {
                DateTime startDate = new DateTime();
                DateTime endDate = new DateTime();
                Options.Clear();
                result_histo.Clear();
                errors.Clear();

                foreach (string s in fields)
                {
                    result_histo.Add(s, new Dictionary<string, List<HistoData>>());
                    Options.fields.Add(s);
                    foreach (string s1 in tickers)
                        result_histo[s].Add(s1, new List<HistoData>());
                }

                foreach (string s in tickers)
                    Options.tickers.Add(s);

                BlgTransactor blg = new BlgTransactor(Options);
                Options.periods = new KeyValuePair<BlgPeriodOpt, BlgPeriods>(periodOpt, period);
                startDate = _startDate.GetValueOrDefault(new DateTime(1900, 1, 1));
                endDate = _endDate.GetValueOrDefault(DateTime.Now);

                string year = Convert.ToString(startDate.Year);
                string month = (startDate.Month < 10) ? "0" + Convert.ToString(startDate.Month) : Convert.ToString(startDate.Month);
                string day = (startDate.Day < 10) ? "0" + Convert.ToString(startDate.Day) : Convert.ToString(startDate.Day);
                string date = year + month + day;
                Options.options.Add(BlgMiscOpt.start_date, date);

                year = Convert.ToString(endDate.Year);
                month = (endDate.Month < 10) ? "0" + Convert.ToString(endDate.Month) : Convert.ToString(endDate.Month);
                day = (endDate.Day < 10) ? "0" + Convert.ToString(endDate.Day) : Convert.ToString(endDate.Day);
                date = year + month + day;
                Options.options.Add(BlgMiscOpt.end_date, date);

                Options.reqType = BlgRequests.historical;

                blg.ProcessData();

                foreach (var kvp in blg.res2)
                {
                    result_histo[kvp.Key[0]][kvp.Key[1]].Add(kvp.Value);
                }
                //Console.WriteLine("on est bien rentré dans  " + result_histo[fields.First()].Values.First().Count);
                errors = blg.errors;

                return true;
            }
            catch (Exception e)
            {
                errors.Add("Program", "Error : " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Get some reference data
        /// </summary>
        /// <param name="tickers">List of tickers</param>
        /// <param name="fields">List of fields</param>
        /// <returns>True if successed, false if failed. Results are contained into ResultRef dictionary and error in Errors dictionary.</returns>
        public bool GetReferenceData(List<string> tickers, List<string> fields)
        {
            try
            {
                Options.Clear();
                result_ref.Clear();
                errors.Clear();
                foreach (string s in fields)
                {
                    result_ref.Add(s, new Dictionary<string, string>());
                    Options.fields.Add(s);
                }

                foreach (string s in tickers)
                    Options.tickers.Add(s);
                Options.reqType = BlgRequests.reference;

                BlgTransactor blg = new BlgTransactor(Options);


                blg.ProcessData();

                foreach (var kvp in blg.res1)
                {
                    result_ref[kvp.Key[0]].Add(kvp.Key[1], kvp.Value);
                }
                errors = blg.errors;

                return true;
            }
            catch (Exception e)
            {
                errors.Add("Program", "Error : " + e.Message);
                return false;
            }

        }
    }
}
