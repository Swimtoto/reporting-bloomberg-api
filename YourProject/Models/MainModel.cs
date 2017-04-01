using NeutralObjects;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourProject.Models
{
    public class MainModel : INotifyPropertyChanged
    {
        private string _ticker;
        private string _ticker1;
        private string _ticker2;
        private string _ticker3;
        private string _blgfield;

        private string _nbShare1;
        private string _nbShare2;
        private string _nbShare3;

        public string Blgfield
        {
            get
            {
                return _blgfield;
            }
            set
            {
                _blgfield = value;
                NotifyPropertyChanged("Blgfield");
            }
        }

        public string Ticker
        {
            get
            {
                return _ticker;
            }
            set
            {
                _ticker = value;
                NotifyPropertyChanged("Ticker");
            }
        }
        public string Ticker1
        {
            get
            {
                return _ticker1;
            }
            set
            {
                _ticker1 = value;
                NotifyPropertyChanged("Ticker1");
            }
        }
        public string Ticker2
        {
            get
            {
                return _ticker2;
            }
            set
            {
                _ticker2 = value;
                NotifyPropertyChanged("Ticker2");
            }
        }
        public string Ticker3
        {
            get
            {
                return _ticker3;
            }
            set
            {
                _ticker3 = value;
                NotifyPropertyChanged("Ticker3");
            }
        }

        public string NbShare1
        {
            get
            {
                return _nbShare1;
            }
            set
            {
                _nbShare1 = value;
                NotifyPropertyChanged("NbShare1");
            }
        }
        public string NbShare2
        {
            get
            {
                return _nbShare2;
            }
            set
            {
                _nbShare2 = value;
                NotifyPropertyChanged("NbShare2");
            }
        }
        public string NbShare3
        {
            get
            {
                return _nbShare3;
            }
            set
            {
                _nbShare3 = value;
                NotifyPropertyChanged("NbShare3");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }
}
