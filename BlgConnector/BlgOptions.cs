using NeutralObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlgConnector
{
    public class BlgOptions
    {
        public List<string> fields;
        public KeyValuePair<BlgPeriodOpt, BlgPeriods> periods;
        public Dictionary<BlgMiscOpt, dynamic> options;
        public List<string> tickers;
        public BlgRequests reqType;

        /// <summary>
        /// Constructor
        /// </summary>
        public BlgOptions()
        {
            fields = new List<string>();
            periods = new KeyValuePair<BlgPeriodOpt, BlgPeriods>();
            options = new Dictionary<BlgMiscOpt, dynamic>();
            tickers = new List<string>();
        }

        /// <summary>
        /// Clear all properties
        /// </summary>
        public void Clear()
        {
            fields.Clear();
            periods = new KeyValuePair<BlgPeriodOpt, BlgPeriods>();
            options.Clear();
            tickers.Clear();
        }
    }

    /// <summary>
    /// Request type
    /// </summary>
    public enum BlgRequests
    {
        [StringValue("HistoricalDataRequest")]
        historical = 0,
        [StringValue("ReferenceDataRequest")]
        reference = 1
    }

    /// <summary>
    /// Predefined periods
    /// </summary>
    public enum BlgPeriods
    {
        [StringValue("ACTUAL")]
        actual = 0,
        [StringValue("MONTHLY")]
        monthly = 1,
        [StringValue("DAILY")]
        daily = 2,
    }

    /// <summary>
    /// Period options
    /// </summary>
    public enum BlgPeriodOpt
    {
        [StringValue("periodicityAdjustment")]
        periodicity_adj = 0,
        [StringValue("periodicitySelection")]
        periodicity_sel = 1
    }

    /// <summary>
    /// Somme other options
    /// </summary>
    public enum BlgMiscOpt
    {
        [StringValue("startDate")]
        start_date = 0,
        [StringValue("endDate")]
        end_date = 1,
        [StringValue("maxDataPoints")]
        max_data_points = 2,
        [StringValue("returnEids")]
        return_eids = 3
    }
}

