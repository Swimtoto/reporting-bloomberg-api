using NeutralObjects;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourProject.Models
{
    public class ResultModel : INotifyPropertyChanged
    {
        private List<HistoData> _donnees1;
        private string _name;
        private string _description;
        private string _sector;
        private string _primaryExchange;
        private string _employee;
        private string _companyvalue;
        private string _rating;

        private string _volatility30D;
        private string _volatility90D;
        private string _volatility180D;
        private string _volatility1Y;

        private string _lastPrice;
        private string _open;
        private string _previousClose;
        private string _wkhigh;
        private string _wklow;

        private DateTime _dateFrom;
        private DateTime _dateTo;


        // GET & SET
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public string Sector
        {
            get
            {
                return _sector;
            }
            set
            {
                _sector = value;
                NotifyPropertyChanged("Sector");
            }
        }
        public string PrimaryExchange
        {
            get
            {
                return _primaryExchange;
            }
            set
            {
                _primaryExchange = value;
                NotifyPropertyChanged("PrimaryExchange");
            }
        }
        public string Employee
        {
            get
            {
                return _employee;
            }
            set
            {
                _employee = value;
                NotifyPropertyChanged("Employee");
            }
        }
        public string CompanyValue
        {
            get
            {
                return _companyvalue;
            }
            set
            {
                _companyvalue = value;
                NotifyPropertyChanged("CompanyValue");
            }
        }
        public string Rating
        {
            get
            {
                return _rating;
            }
            set
            {
                _rating = value;
                NotifyPropertyChanged("Rating");
            }
        }

        public string Volatility30D
        {
            get
            {
                return _volatility30D;
            }
            set
            {
                _volatility30D = value;
                NotifyPropertyChanged("Volatility30D");
            }
        }
        public string Volatility90D
        {
            get
            {
                return _volatility90D;
            }
            set
            {
                _volatility90D = value;
                NotifyPropertyChanged("Volatility90D");
            }
        }
        public string Volatility180D
        {
            get
            {
                return _volatility180D;
            }
            set
            {
                _volatility180D = value;
                NotifyPropertyChanged("Volatility180D");
            }
        }
        public string Volatility1Y
        {
            get
            {
                return _volatility1Y;
            }
            set
            {
                _volatility1Y = value;
                NotifyPropertyChanged("Volatility1Y");
            }
        }
        public string LastPrice
        {
            get
            {
                return _lastPrice;
            }
            set
            {
                _lastPrice = value;
                NotifyPropertyChanged("LastPrice");
            }
        }
        public string Open
        {
            get
            {
                return _open;
            }
            set
            {
                _open = value;
                NotifyPropertyChanged("Open");
            }
        }
        public string PreviousClose
        {
            get
            {
                return _previousClose;
            }
            set
            {
                _previousClose = value;
                NotifyPropertyChanged("PreviousClose");
            }
        }
        public string WKhigh
        {
            get
            {
                return _wkhigh;
            }
            set
            {
                _wkhigh = value;
                NotifyPropertyChanged("WKhigh");
            }
        }

        public string WKlow
        {
            get
            {
                return _wklow;
            }
            set
            {
                _wklow = value;
                NotifyPropertyChanged("WKlow");
            }
        }

        public DateTime DateFrom
        {
            get
            {
                return _dateFrom;
            }
            set
            {
                _dateFrom = value;
                NotifyPropertyChanged("DateFrom");
            }
        }
        public DateTime DateTo
        {
            get
            {
                return _dateTo;
            }
            set
            {
                _dateTo = value;
                NotifyPropertyChanged("DateTo");
            }

        }
      






        public List<HistoData> Donnees1
        {
            get
            {
                return _donnees1;
            }
            set
            {
                _donnees1 = value;
                NotifyPropertyChanged("Donnees1");
            }
        }

        public List<DataPoint> DonneesChartable
        {
            get
            {
                if (!_donnees1.Any())
                    return null;
                else
                {
                    return _donnees1.ConvertAll<DataPoint>(new Converter<HistoData, DataPoint>(HistoDataToDataPoint));
                }
            }
        }

        private DataPoint HistoDataToDataPoint(HistoData pf)
        {
            return new DataPoint(pf.Date.ToOADate(), pf.Price);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public ResultModel()
        {
            _donnees1 = new List<HistoData>();
        }
    }
}
